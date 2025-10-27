
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using System.Linq;

#if UNITY_EDITOR
using System.Diagnostics;
#endif

[CreateAssetMenu(fileName = "Raycast Filter", menuName = "ScriptableObjects/EQS Filters/Raycast Filter", order = 1)]
public class RaycastFilter : Filter
{
    public LayerMask layerMask;

    public RaycastFilter(FilterType filterType = FilterType.SCORING, bool inverseCondition = false) : base(filterType, inverseCondition)
    {

    }

    public EQS ExecuteLayerMono(EQS eqs)
    {

#if UNITY_EDITOR
        Stopwatch sw = Stopwatch.StartNew();
#endif

        // copy of the dict keys because you can't change a dictionnary values in a loop referencing it (xDDDDDD)
        List<Vector3> keys = new List<Vector3>(eqs.data.Keys);
        foreach (Vector3 key in keys)
        {
            RaycastHit hit = new RaycastHit();
            Ray ray = new Ray();

            ray.origin = eqs.GetCenter();
            ray.direction = Vector3.Normalize(key - eqs.GetCenter());

            bool hasHit = Physics.Raycast(ray, out hit, (key - eqs.GetCenter()).magnitude, layerMask);

            if (filterType == FilterType.FILTER)
            {
                if ((!inverseCondition && hasHit) || (inverseCondition && !hasHit))
                {
                    eqs.data.Remove(key);
                    eqs.removedPoints.Add(key);
                }

                continue;
            }

            if (filterType == FilterType.SCORING)
            {
                eqs.data[key] = eqs.data[key] * 0.5f;
                if ((inverseCondition && !hasHit) || (!inverseCondition && hasHit))
                {
                    eqs.data[key] += 1f;
                }
                else
                    eqs.data[key] = 0f;
            }
        }

        // clamp score values, need to figure out how to score using raycasts
        eqs = base.ExecuteLayer(eqs);

#if UNITY_EDITOR
        sw.Stop();
        execTime = (float)sw.Elapsed.TotalMilliseconds;
#endif

        return eqs;
    }

    public override EQS ExecuteLayer(EQS eqs)
    {

#if UNITY_EDITOR
        Stopwatch sw = Stopwatch.StartNew();
#endif
        List<Vector3> keys = new List<Vector3>(eqs.data.Keys);

        // 2 results/hit because we want to hit back faces too
        var results = new NativeArray<RaycastHit>(keys.Count() * 2, Allocator.TempJob);

        // Perform one raycast per key
        var commands = new NativeArray<RaycastCommand>(keys.Count(), Allocator.TempJob);

        Vector3 centerPos = targetTransform != null ? targetTransform.position : eqs.GetCenter();

        for (int i = 0; i  < keys.Count(); i++)
        {
            commands[i] = new RaycastCommand(centerPos, Vector3.Normalize(keys[i] - centerPos),
                new QueryParameters(layerMask, false, QueryTriggerInteraction.UseGlobal, true), ( keys[i] - centerPos).magnitude);
        }

        JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 1, 2);

        // Wait for the batch processing job to complete
        handle.Complete();

        for (int i = 0; i < keys.Count(); i++)
        {
            Vector3 key = keys[i];

            RaycastHit hit = results[i * 2];
            if (i * 2 >= results.Length)
                hit = results[results.Length - 1];

            hit = results[i * 2];

            bool hasHit = hit.collider != null;

            if (filterType == FilterType.FILTER)
            {
                if ((!inverseCondition && hasHit) || (inverseCondition && !hasHit))
                {
                    eqs.data.Remove(key);
                    eqs.removedPoints.Add(key);
                }

                continue;
            }

            if (filterType == FilterType.SCORING)
            {
                eqs.data[key] = eqs.data[key] * 0.5f;
                if ((inverseCondition && !hasHit) || (!inverseCondition && hasHit))
                {
                    eqs.data[key] += 1f;
                }
                else
                    eqs.data[key] = 0f;
            }
        }

        // Dispose the buffers
        results.Dispose();
        commands.Dispose();

        // clamp score values, need to figure out how to score using raycasts
        eqs = base.ExecuteLayer(eqs);
#if UNITY_EDITOR
        sw.Stop();
        execTime = (float)sw.Elapsed.TotalMilliseconds;
#endif

        return eqs;
    }

}
