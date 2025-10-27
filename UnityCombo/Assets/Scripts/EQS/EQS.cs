
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

#if UNITY_EDITOR
using System.Diagnostics;
#endif

[CreateAssetMenu(fileName = "EQS", menuName = "ScriptableObjects/EQS", order = 1)]
public class EQS : ScriptableObject
{
    public Vector3 startPos;

    public List<EQSLayer> layers = new List<EQSLayer>();

    public Dictionary<Vector3, float> data = new Dictionary<Vector3, float>();

    [HideInInspector]
    public List<Vector3> removedPoints = new List<Vector3>();

    protected Vector3 center = Vector3.zero;

    // Stats for debug
#if UNITY_EDITOR
    [HideInInspector]
    public float execTime = 0f;
    [HideInInspector]
    public float resultsTime = 0f;
#endif

    public Vector3 GetCenter()
    {
        return center;
    }

    // Position is the start pos (global offset) of the eqs
    // Percentile is the top percentile of points we want to get, 0-1, <0 returns best point only
    // ReverseOrder means we get the worst points, based on percentile
    public List<Vector3> RunEQS(Vector3 position, float percentile = 0f, bool reverseOrder = false)
    {
        center = startPos + position;

#if UNITY_EDITOR
        Stopwatch sw = Stopwatch.StartNew();
#endif

        foreach (var layer in layers) 
            layer.ExecuteLayer(this);

#if UNITY_EDITOR
        sw.Stop();
        execTime = (float)sw.Elapsed.TotalMilliseconds;

        sw.Reset();
        sw.Start();
#endif

        IOrderedEnumerable<KeyValuePair<Vector3, float>> sortedData;
        if (reverseOrder)    
            sortedData = data.OrderBy(x => x.Value);
        else
            sortedData = data.OrderByDescending(x => x.Value);
    
        List<Vector3> result = new List<Vector3>();

        if (data.Count() <= 0)
            return result;

        int size = (int)(sortedData.Count() * percentile);

        if (percentile <= 0f || size <= 0 || size >= sortedData.Count())
        {
            result.Add(sortedData.First().Key);

#if UNITY_EDITOR
            sw.Stop();
            resultsTime = (float)sw.Elapsed.TotalMilliseconds;
#endif
            return result;
        }

        int i = 0;
        foreach (KeyValuePair<Vector3, float> pair in sortedData)
        {
            if (i >= size)
                break;

            result.Add(pair.Key);
            i++;
        }

#if UNITY_EDITOR
        sw.Stop();
        resultsTime = (float)sw.Elapsed.TotalMilliseconds;
#endif
        return result;
    }

    public void Clear()
    { 
        data.Clear(); 
        removedPoints.Clear(); 
    }

}
