
#if UNITY_EDITOR
using System.Diagnostics;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "Rings Generator", menuName = "ScriptableObjects/EQS Generators/Rings Generator", order = 1)]
public class RingsGenerator : Generator
{

    public int rings = 3;
    public int pointsPerRing = 8;
    public float inRadius = 3f;
    public float outRadius = 10f;
    public float offsetRings = 0f;

    public override EQS ExecuteLayer(EQS eqs = null)
    {
#if UNITY_EDITOR
        Stopwatch sw = Stopwatch.StartNew();
#endif

        if (eqs == null)
            eqs = new EQS();

        float radiusIncrement = (outRadius - inRadius) / rings;
        float angle = (Mathf.PI * 2) / pointsPerRing;
        float ringOffset = angle / rings;

        Vector3 globalOffset = eqs.GetCenter() + offset;

        for (int i = 0; i < pointsPerRing; i++)
        {
            float currAngle = i * angle;

            for (int j = 0; j < rings; j++)
            {
                currAngle += ringOffset * offsetRings; 

                Vector3 pos = new Vector3(Mathf.Cos(currAngle), 0f, Mathf.Sin(currAngle));
                pos *= j * radiusIncrement + inRadius;
                eqs.data.Add(pos + globalOffset, 1f);
            }
        }

#if UNITY_EDITOR
        sw.Stop();
        execTime = (float)sw.Elapsed.TotalMilliseconds;
#endif
        return eqs;
    }
}
