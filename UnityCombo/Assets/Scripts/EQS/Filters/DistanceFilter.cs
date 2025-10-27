
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.Diagnostics;
#endif

[CreateAssetMenu(fileName = "Distance Filter", menuName = "ScriptableObjects/EQS Filters/Distance Filter", order = 1)]
public class DistanceFilter : Filter
{
    public float cutoff = 5f;

    public DistanceFilter(float cutoff = 5f, FilterType filterType = FilterType.SCORING, bool inverseCondition = false) 
        : base(filterType, inverseCondition)
    {
        this.cutoff = cutoff;
    }

    public override EQS ExecuteLayer(EQS eqs)
    {

#if UNITY_EDITOR
        Stopwatch sw = Stopwatch.StartNew();
#endif

        float max = -10000000f;
        float min = 10000000f;
        float cutoffSq = cutoff * cutoff;

        Vector3 centerPos = targetTransform != null ? targetTransform.position : eqs.GetCenter();

        // copy of the dict keys because you can't change a dictionnary values in a loop referencing it (xDDDDDD)
        List<Vector3> keys = new List<Vector3>(eqs.data.Keys);
        foreach (Vector3 key in keys)
        {
            float distSq = Vector3.SqrMagnitude(key - centerPos);

            if (filterType == FilterType.FILTER)
            {
                if ((inverseCondition && distSq > cutoffSq) || (!inverseCondition && distSq < cutoffSq))
                {
                    eqs.data.Remove(key);
                    eqs.removedPoints.Add(key);
                }
                continue;
            }

            eqs.data[key] += distSq;

            if (filterType == FilterType.SCORING)
            {
                if (eqs.data[key] > max)
                    max = eqs.data[key];

                if (eqs.data[key] < min)
                    min = eqs.data[key];
            }
        }

        if (filterType == FilterType.SCORING)
            foreach (Vector3 key in keys)
            {
                if (inverseCondition)
                    eqs.data[key] = 1f - (eqs.data[key] - min) / (max - min);
                else
                   eqs.data[key] = (eqs.data[key] - min) / (max - min);
            }

#if UNITY_EDITOR
        sw.Stop();
        execTime = (float)sw.Elapsed.TotalMilliseconds;
#endif

        return eqs;
    }
}
