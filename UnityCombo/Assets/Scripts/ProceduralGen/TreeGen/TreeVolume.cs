
using System.Collections.Generic;
using UnityEngine;

public class TreeVolume : MonoBehaviour
{
    [HideInInspector]
    public List<Vector3> points = new List<Vector3>();

    [SerializeField]
    public int pointsNum;

    [SerializeField]
    bool showPoints = true;

    virtual public void InitPoints(){}

    virtual protected void DrawVolumeGizmo(){}

    public void OnDrawGizmosSelected()
    {
        DrawVolumeGizmo();

        if (points.Count <= 0 || !showPoints)
            return;

        foreach (Vector3 point in points) 
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(point, 0.1f);
        }
    }
}
