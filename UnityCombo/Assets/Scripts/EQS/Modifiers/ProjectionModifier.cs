
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using System.Linq;

#if UNITY_EDITOR
using System.Diagnostics;
#endif

[CreateAssetMenu(fileName = "Projection Modifier", menuName = "ScriptableObjects/EQS Modifiers/Projection Modifier", order = 1)]
public class ProjectionModifier : Modifier
{
    public Vector2 projectionRange = new Vector2(-1f, 1f);
    public LayerMask layerMask;
    public float groundClearance = 0.1f;

    public override EQS ExecuteLayer(EQS eqs = null)
    {
#if UNITY_EDITOR
        Stopwatch sw = Stopwatch.StartNew();
#endif

        List<Vector3> keys = new List<Vector3>(eqs.data.Keys);

        // 1 results/hit because we don't want to hit back faces
        var results = new NativeArray<RaycastHit>(keys.Count(), Allocator.TempJob);

        // Perform one raycast per key
        var commands = new NativeArray<RaycastCommand>(keys.Count(), Allocator.TempJob);

        for (int i = 0; i < keys.Count(); i++)
        {
            commands[i] = new RaycastCommand(keys[i] + Vector3.up * projectionRange.y, Vector3.down,
                new QueryParameters(layerMask, false, QueryTriggerInteraction.UseGlobal, false), projectionRange.y - projectionRange.x);
        }

        JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 1, 1);

        // Wait for the batch processing job to complete
        handle.Complete();

        Dictionary<Vector3, float> newData = new Dictionary<Vector3, float>();

        for (int i = 0; i < keys.Count(); i++)
        {
            Vector3 key = keys[i];

            RaycastHit hit = results[i];

            //if (i >= results.Length)
            //    hit = results[results.Length - 1];

            //hit = results[i];

            bool hasHit = hit.collider != null;

            if (hit.collider == null)
            {
                eqs.data.Remove(key);
                eqs.removedPoints.Add(key);
                continue;
            }

            newData.Add(hit.point + Vector3.up * groundClearance, 1f);
        }

        // Dispose the buffers
        results.Dispose();
        commands.Dispose();

        eqs.data = newData;

#if UNITY_EDITOR
        sw.Stop();
        execTime = (float)sw.Elapsed.TotalMilliseconds;
#endif
        return eqs;
    }
}
