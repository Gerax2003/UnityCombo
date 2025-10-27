
using UnityEngine;
#if UNITY_EDITOR
using System.Diagnostics;
#endif

[CreateAssetMenu(fileName = "Grid Generator", menuName = "ScriptableObjects/EQS Generators/Grid Generator", order = 1)]
public class GridGenerator : Generator
{
    public float gridSize = 30f;
    public float pointsSpacing = 2f;

    public GridGenerator(float gridSize, float pointsSpacing)
    {
        this.gridSize = gridSize;
        this.pointsSpacing = pointsSpacing;
    }

    public override EQS ExecuteLayer(EQS eqs = null)
    {
#if UNITY_EDITOR
        Stopwatch sw = Stopwatch.StartNew();
#endif

        if (eqs == null) 
            eqs = new EQS();

        Vector3 globalOffset = eqs.GetCenter() + offset;

        for (float i = -gridSize * 0.5f; i <= gridSize * 0.5f; i += pointsSpacing)
            for (float j = -gridSize * 0.5f; j <= gridSize * 0.5f; j += pointsSpacing)
                eqs.data.Add(new Vector3(i, 0f, j) + globalOffset, 1f);

#if UNITY_EDITOR
        sw.Stop();
        execTime = (float)sw.Elapsed.TotalMilliseconds;
#endif
        return eqs;
    }
}
