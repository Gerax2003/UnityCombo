
using UnityEditor;
using UnityEngine;

public class SphereTreeVolume : TreeVolume
{
    [SerializeField]
    private float radius = 4f;

    override public void InitPoints() 
    {
        points.Clear();        

        for (int i = 0; i < pointsNum; ++i) 
        {
            Vector3 dir = Random.insideUnitSphere.normalized;
            // Distribution skewed toward the edges, avoids concentration near the center
            float dist = Mathf.Pow(Mathf.Sin(RNG.Rand(0f, 1) * (Mathf.PI / 2f)), 0.8f) * radius;
            points.Add(transform.position + dir * dist);
        }
    }

    override protected void DrawVolumeGizmo()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

[CustomEditor(typeof(SphereTreeVolume))]
class SphereTreeVolumeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Generate points"))
        {
            SphereTreeVolume vol = (SphereTreeVolume)target;

            vol.InitPoints();
        }
    }
}
