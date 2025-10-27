
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EQSTestScript : MonoBehaviour
{
    [SerializeField]
    private bool showGizmos = true;
    [SerializeField]
    private bool showFailed = true;
    [SerializeField]
    private bool showScore = true;
    [SerializeField]
    private bool showResults = true;
    
    [SerializeField]
    Transform eqsLayersTarget;
    
    [SerializeField]
    EQS eqs;

    [SerializeField]
    float percentile = 0.1f;
    [SerializeField]
    bool reverseOrder = false;

    [HideInInspector]
    List<Vector3> result = new List<Vector3>();

    public void GeneratePoints()
    {
        if (eqs == null)
            return;

        eqs.Clear();
        
        foreach (EQSLayer layer in eqs.layers)
        {
            Filter filter = layer as Filter;
            if (filter != null)
                filter.targetTransform = eqsLayersTarget;
        }

        result = eqs.RunEQS(transform.position, percentile, reverseOrder);
    }

    public EQS GetEQS()
    {
        return eqs;
    }

    public List<Vector3> GetResult()
    {
        return result;
    }

    public void OnDrawGizmosSelected()
    {
        if (!showGizmos || eqs == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(eqs.startPos, new Vector3(0.2f, 0.2f, 0.2f));

        Color alpha = new Color(1f, 1f, 1f, 0.65f);

        foreach (KeyValuePair<Vector3, float> pair in eqs.data)
        {
            Gizmos.color = Color.Lerp(Color.red * alpha, Color.green * alpha, pair.Value);
            Gizmos.DrawSphere(pair.Key, 0.2f);

            if (showScore)
                Handles.Label(pair.Key + new Vector3(0f, 0.25f), pair.Value.ToString("F3"));
        }

        if (showResults)
        {        
            Gizmos.color = Color.blue * alpha;
            foreach (Vector3 point in result)
                Gizmos.DrawCube(point + Vector3.up * 0.25f, new Vector3(0.1f, 0.5f, 0.1f));
        }
        
        if (!showFailed) return;
        foreach (Vector3 point in eqs.removedPoints)
        {
            Gizmos.color = Color.black * alpha;
            Gizmos.DrawSphere(point, 0.2f);

            if (showScore)
                Handles.Label(point + new Vector3(0f, 0.25f), "-1");
        }
    }
}

[CustomEditor(typeof(EQSTestScript))]
class EQSTestScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EQSTestScript eqsTest = (EQSTestScript)target;
        EQS eqs = eqsTest.GetEQS();

        DrawDefaultInspector();
        if (GUILayout.Button("Generate EQS"))
        {
            eqsTest.GeneratePoints();
        }

        int validPoints = eqs.data.Count;

        string statsText = "Total of valid points: " + validPoints + "\n"
            + "Total of results points: " + eqsTest.GetResult().Count + " (" + (float)eqsTest.GetResult().Count / validPoints * 100f + "%)\n"
            + "Total of failed points: " + eqs.removedPoints.Count + " (" + (float)eqs.removedPoints.Count / validPoints * 100f + "%)\n"
            + "Total execution time: " + (eqs.execTime + eqs.resultsTime).ToString("F3") + "ms \n" 
            + "EQS execution: " + eqs.execTime.ToString("F3") + "ms, results processing: " + eqs.resultsTime.ToString("F3") + "ms\n"
            + "Time per layer execution:\n";

        int i = 0;
        foreach(EQSLayer layer in eqs.layers) 
        {
            statsText += " -[" + i + "]" + layer.execTime + " ms\n";
            i++;
        }

        EditorGUILayout.TextArea(statsText);
    }
}
