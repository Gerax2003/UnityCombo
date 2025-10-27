using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using Debug = UnityEngine.Debug;

public enum IKType
{
    CCD,
    TRIANGULATION,
}

public class IKSolver : MonoBehaviour
{
    [SerializeField]
    TMP_Text statsText;

    int chainLen = 0;

    [SerializeField]
    IKType type = IKType.CCD;

    [SerializeField]
    bool useConstraints = true;
    [SerializeField]
    bool useAngleLimit = true;

    [SerializeField]
    bool stableAngleLimit = false;

    Bone lastBone;
    Bone firstBone;

    [SerializeField]
    Transform target;

    // Start is called before the first frame update
    void Start()
    {
        firstBone = GetComponentInChildren<Bone>();
        Bone currBone = firstBone;
        while (currBone != null)
        {
            chainLen++;

            if (currBone.childBone != null)
            {
                currBone = currBone.childBone;
                continue;
            }

            lastBone = currBone;
            break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        statsText.text = "Type: " + type.ToString() + "\n"
            + "Length: " + chainLen + "\n"
            + "Angle limit: " + useAngleLimit.ToString() + "\n"
            + "Stable limiter: " + stableAngleLimit.ToString() + "\n"
            + "Axis constraints: " + useConstraints.ToString() + "\n";

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        switch (type)
        {
            case IKType.CCD:
                if (useConstraints)
                    CCDConstraints();
                else
                    CCDNoConstraints();
                break;
            case IKType.TRIANGULATION:
                CCDTriangulation();
                break;
        }
        stopwatch.Stop();

        statsText.text += "Total time: " + stopwatch.Elapsed.TotalMilliseconds + " ms\n";
    }

#region CCD
    void CCDNoConstraints()
    {
        Stopwatch stopwatch = new Stopwatch();
        double totalTime = 0d;
        int i = 0;

        Bone bone = lastBone;

        while (bone != null)
        {
            stopwatch.Start();
            bone.transform.rotation = bone.transform.rotation = CCDRotateToPointLast(bone);

            // Go up in chain
            bone = bone.parentBone;

            stopwatch.Stop();
            totalTime += stopwatch.Elapsed.TotalMilliseconds;
            i++;
        }

        statsText.text += "Avg iteration: " + totalTime / i + " ms\n";
    }

    void CCDConstraints()
    {
        Stopwatch constraintStopwatch = new Stopwatch();
        Stopwatch stopwatch = new Stopwatch();
        double totalTime = 0d;
        double totalConstraintTime = 0d;
        int i = 0;

        Bone bone = lastBone;

        while (bone != null)
        {
            stopwatch.Start();
            bone.transform.rotation = CCDRotateToPointLast(bone);

            CCDConstraintCalculation(bone, constraintStopwatch, ref totalConstraintTime);

            // Go up in chain
            bone = bone.parentBone;

            stopwatch.Stop();
            totalTime += stopwatch.Elapsed.TotalMilliseconds;
            i++;
        }

        statsText.text += "Avg constraint: " + totalConstraintTime / i + " ms\n";
        statsText.text += "Avg iteration: " + totalTime / i + " ms\n";
    }

    void CCDTriangulation()
    {
        Stopwatch stopwatch = new Stopwatch();
        double totalTime = 0d;
        Stopwatch constraintStopwatch = new Stopwatch();
        double totalConstraintTime = 0d;
        int i = 0;

        // triangulation starts from beginning, not end (closer to reality)
        Bone bone = firstBone;

        while (bone != null)
        {
            stopwatch.Start();

            Vector3 toTarget = target.position - bone.transform.position;

            float a = bone.length; // a is the length of the current joint
            float b = GetBLength(bone); // Chain length from the end of current joint to end of chain
            float c = toTarget.magnitude; // length from effector to target

            // abc triangle is impossible because distance to effector is longer than the whole remaining chain, we force the rotation to get chain's end closer
            // in other words, the necessary rotation was made so we make our way back to the target
            if (c > a + b)
            {
                bone.transform.forward = toTarget.normalized;
            }
            // abc triangle is impossible because distance to effector is not the longest side of the triangle, we force the rotation to get chain's end further
            // in other words, the necessary rotation needs to be done further so we go away from the target
            else if (c < Mathf.Abs(a - b))
            {
                bone.transform.forward = -toTarget.normalized;
            }
            // triangle can be made, therefore we can find a rotation that solves the chain at this joint
            else
            {
                // angle of the abc triangle, used to calculate angle needed for joint to rotate
                float deltaB = Mathf.Acos(-(b * b - a * a - c * c) / (2 * a * c));

                // get angle to rotate to form abc triangle, fullAngle - triangleAngle
                float theta = Mathf.Acos(Vector3.Dot(bone.transform.forward, toTarget.normalized)) - deltaB;

                Vector3 axis = Vector3.up;
                if (bone.transform.forward != toTarget.normalized && bone.transform.forward != -toTarget.normalized)
                    axis = Vector3.Cross(bone.transform.forward, toTarget.normalized);

                Debug.Log(bone.name + ", theta = " + theta.ToString() + ", axis = " + axis.ToString());
#pragma warning disable CS0618 // AxisAngle is deprecated because it uses radians. I don't want to convert to deg in this one to maintain performance & clarity
                bone.transform.rotation = Quaternion.AxisAngle(axis, theta) * bone.transform.rotation;
#pragma warning restore CS0618 
            }

            if (useConstraints)
                TriangulationConstraint(bone, constraintStopwatch, ref totalConstraintTime);

            // Go down in chain
            bone = bone.childBone;

            stopwatch.Stop();
            totalTime += stopwatch.Elapsed.TotalMilliseconds;
            i++;
        }

        statsText.text += "Avg constraint: " + totalConstraintTime / i + " ms\n";
        statsText.text += "Avg iteration: " + totalTime / i + " ms\n";
    }

    // This function constrains the rotation around the bone's axis by the angle limit defined
    void CCDConstraintCalculation(Bone bone, Stopwatch constraintStopwatch, ref double totalConstraintTime)
    {
        // If the bone has an axis to constrain on, constrain
        if (bone.jointAxis != Vector3.zero)
        {
            constraintStopwatch.Start();
            Vector3 currAxis = bone.transform.rotation * bone.jointAxis;
            Vector3 currParentAxis = Vector3.zero;

            if (bone.parentBone != null)
                currParentAxis = bone.parentBone.transform.rotation * bone.jointAxis;
            else
                currParentAxis = Quaternion.identity * bone.jointAxis;

            bone.transform.rotation = Quaternion.FromToRotation(currAxis, currParentAxis) * bone.transform.rotation;

            if (useAngleLimit)
            {

                // Bloated implementation but very stable, used for benchmark performance and quality of other implementations, taken from:
                // https://github.com/zalo/MathUtilities/blob/master/Assets/Constraints/Constraints.cs | https://github.com/zalo/MathUtilities/blob/master/Assets/IK/CCDIK/CCDIKJoint.cs
                // !! Only takes max angle into account, needs a positive max! !!
                if (stableAngleLimit)
                {
                    // Find compensating rotation and apply it to unclamped rotation
                    bone.transform.rotation = CCDAngleStableImpl(bone);
                }
                else // just clamp the rotation quaternion's angle around its own axis
                     // Known instabilities: stutters on spawn/fast movement, inability to solve then teleportation and reverse joints if range does not cross 0
                {
                    Quaternion clampRotation = ClampRotation(bone.transform.localRotation, bone.jointMinLimits, bone.jointMaxLimits);
                    bone.transform.localRotation = clampRotation;
                }

                constraintStopwatch.Stop();
                totalConstraintTime += constraintStopwatch.Elapsed.TotalMilliseconds;
            }
        }
    }

    // First step for CCD, point current bone in a way that moves last bone as close as the target as possible
    Quaternion CCDRotateToPointLast(Bone bone)
    {
        // Get rotation to from bone->end to bone->target, points end to target
        Vector3 toEnd = (lastBone.transform.position - bone.transform.position).normalized;
        Vector3 toTarget = (target.position - bone.transform.position).normalized;
        Quaternion rot = Quaternion.FromToRotation(toEnd, toTarget);

        // Update bone rotation to point end to target
        Quaternion oldRot = bone.transform.rotation;
        Quaternion newRot = rot * oldRot;
        return newRot;
    }

    #region TRIANGULATION UTILS
    // Same as CCD constraints, just using child instead of parent
    void TriangulationConstraint(Bone bone, Stopwatch constraintStopwatch, ref double totalConstraintTime)
    {
        // If the bone has an axis to constrain on, constrain
        if (bone.jointAxis != Vector3.zero)
        {
            constraintStopwatch.Start();
            Vector3 currAxis = bone.transform.rotation * bone.jointAxis;
            Vector3 currChildAxis = Vector3.zero;

            if (bone.childBone != null)
                currChildAxis = bone.childBone.transform.rotation * bone.jointAxis;
            else
                currChildAxis = Quaternion.identity * bone.jointAxis;

            bone.transform.rotation = Quaternion.FromToRotation(currChildAxis, currAxis) * bone.transform.rotation;

            if (useAngleLimit)
            {
                // Bloated implementation but very stable, used for benchmark performance and quality of other implementations, taken from:
                // https://github.com/zalo/MathUtilities/blob/master/Assets/Constraints/Constraints.cs | https://github.com/zalo/MathUtilities/blob/master/Assets/IK/CCDIK/CCDIKJoint.cs
                // !! Only takes max angle into account, needs a positive max! !!
                if (stableAngleLimit)
                {
                    // Find compensating rotation and apply it to unclamped rotation
                    bone.transform.rotation = CCDAngleStableImpl(bone);
                }
                else // just clamp the rotation quaternion's angle around its own axis
                {
                    Quaternion clampRotation = ClampRotation(bone.transform.localRotation, bone.jointMinLimits, bone.jointMaxLimits);
                    bone.transform.localRotation = clampRotation;
                }

                constraintStopwatch.Stop();
                totalConstraintTime += constraintStopwatch.Elapsed.TotalMilliseconds;
            }
        }
    }

    float GetBLength(Bone bone)
    {
        float length = 0.000001f;

        Bone currBone = bone.childBone;
        while (currBone != null)
        {
            length += currBone.length;
            currBone = currBone.childBone;
        }

        return length;
    }
    #endregion

    #region CCD STABLE
    // Bloated implementation but very stable, used for benchmark performance and quality of other implementations, taken from:
    // https://github.com/zalo/MathUtilities/blob/master/Assets/Constraints/Constraints.cs | https://github.com/zalo/MathUtilities/blob/master/Assets/IK/CCDIK/CCDIKJoint.cs
    // !! Only takes max angle into account, needs a positive max! !!
    Quaternion CCDAngleStableImpl(Bone bone)
    {
        Quaternion rot = bone.transform.rotation;
        Quaternion parentRot = bone.parentBone != null ? bone.parentBone.transform.rotation : Quaternion.identity;
        // basically the joint's forwards?
        Vector3 perpendicular = Perpendicular(bone.jointAxis);
        // Find the vector that represents the clamped rotation(?) using current direction and parent's direction (????)
        Vector3 constraintVector = ConstrainToNormal(rot * perpendicular, parentRot * perpendicular, bone.jointMaxLimits);
        // Find compensating rotation to apply to unclamped rotation
        return Quaternion.FromToRotation(rot * perpendicular, constraintVector) * rot;
    }

    // used by stable implementation, not mine
    public Vector3 Perpendicular(Vector3 vec)
    {
        return Mathf.Abs(vec.x) > Mathf.Abs(vec.z) ? new Vector3(-vec.y, vec.x, 0f)
                                                   : new Vector3(0f, -vec.z, vec.y);
    }
    // used by stable implementation, not mine
    public Vector3 ConstrainToNormal(Vector3 direction, Vector3 normalDirection, float maxAngle)
    {
        if (maxAngle <= 0f) return normalDirection.normalized * direction.magnitude; if (maxAngle >= 180f) return direction;

        float dot = Mathf.Clamp(Vector3.Dot(direction.normalized, normalDirection.normalized), -1f, 1f);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        // direction represents the vector without a rotation, normalDirection the maximum and "(angle - maxAngle) / angle" calculates the clamp??
        Vector3 ret = Vector3.Slerp(direction.normalized, normalDirection.normalized, (angle - maxAngle) / angle);
        return ret * direction.magnitude;
    }
    #endregion

#endregion

#region UTILITIES
    Vector3 DivideVector(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x/v2.x, v1.y/v2.y, v1.z/v2.z);
    }

    Quaternion ClampRotation(Quaternion rotation, float minAngle, float maxAngle)
    {
        float angle;
        Vector3 axis;

        rotation.ToAngleAxis(out angle, out axis);

        // clamp range is -180.180, unity angle range is 0.360 so we convert ranges
        angle = Angle360To180(angle);
        angle = Mathf.Clamp(angle, minAngle, maxAngle);
        angle = Angle180To360(angle);

        Quaternion ret = Quaternion.AngleAxis(angle, axis);

        return ret;
    }
    // Change angle from 0.360 range to -180.180 range
    float Angle360To180(float angle)
    {
        if (angle <= 180f)
            return angle;

        return -(360f - angle);
    }
    // Change angle from -180.180 range to 0.360 range
    float Angle180To360(float angle)
    {
        if (angle >= 0f)
            return angle;

        return 360f + angle;
    }
#endregion
}
