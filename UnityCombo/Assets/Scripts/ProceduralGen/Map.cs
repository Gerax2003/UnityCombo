using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Map : MonoBehaviour
{
    private float[,,] EmptyMap = new float[0, 0, 0];
    public Vector3Int Dimensions = new Vector3Int(0,0,0);
    public float[,,] MapPoints = new float[0,0,0];
    private MeshFilter Mf;
    private MeshCollider Mc;
    [SerializeField][Range(0f, 0.9999f)] float Threshold = 0.9999f;

    private List<Vector3> Vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    
    public bool IsVisible = false;
    public bool MapBuilt = false;

    private void OnValidate()
    {
        UpdateMap();
    }

    public void UpdateMap()
    {
        if (MapBuilt)
            return;
        MarchingCube(); 
        SetMesh();
        
        MapBuilt = true;
    }

    void MarchingCube()
    {
        triangles.Clear();
        Vertices.Clear();

        for (int x = 0; x < Dimensions.x; x++)
        {
            for (int y = 0; y < Dimensions.y; y++)
            {
                for (int z = 0; z < Dimensions.z; z++)
                {
                    float[] Corners = new float[8];
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int corner = new Vector3Int(x, y, z) + MarchingTable.Corners[i];
                        Corners[i] = MapPoints[corner.x, corner.y, corner.z];
                    }
                    MarchCube(new Vector3(x,y,z),GetConfigIndex(Corners));
                }
            }
        }
    }

    void MarchCube(Vector3 pos, int configID)
    {
        if (configID == 0 || configID == 255)
            return;
        int edgeID = 0;
        for (int t = 0; t < 5; t++) //t stands for Triangles
        {
            for (int v = 0; v < 3; v++) //v stands for Vertices
            {
                int trisTableValue = MarchingTable.Triangles[configID, edgeID];

                if (trisTableValue == -1)
                    return;

                Vector3 edgeStart = pos + MarchingTable.Edges[trisTableValue, 0];
                Vector3 edgeEnd = pos + MarchingTable.Edges[trisTableValue, 1];

                Vector3 vertex = (edgeStart + edgeEnd) / 2;
                
                Vertices.Add(vertex);
                triangles.Add(Vertices.Count - 1);
                
                edgeID++;
            }
        }
    }

    private int GetConfigIndex(float[] corners)
    {
        int index = 0;
        
        for (int i = 0; i < 8; i++)
        {
            if (corners[i] > Threshold)
            {
                index |= 1 << i;
            }
        }

        return index;
    }

    void SetMesh()
    {
        //Ensures mesh filter and collider are present in the script
        if(!Mf)
            Mf = GetComponent<MeshFilter>();
        if(!Mc)
            Mc = GetComponent < MeshCollider>();
        
        Mesh mesh = new Mesh();
        mesh.vertices = Vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        Mf.mesh = mesh;
        Mc.sharedMesh = mesh;
    }
    
    void OnDrawGizmos()
    {
        bool b =
            MapPoints.Rank == EmptyMap.Rank &&
            Enumerable.Range(0,MapPoints.Rank).All(dimension => MapPoints.GetLength(dimension) == EmptyMap.GetLength(dimension)) &&
            MapPoints.Cast<double>().SequenceEqual(EmptyMap.Cast<double>());
        if (b || !IsVisible || Dimensions == Vector3Int.zero) // Ensuring that the map has valid map data
        {
            IsVisible = false;
            return;
        }
        
        for (int x = 0; x < Dimensions.x; x++)
        {
            for (int y = 0; y < Dimensions.y; y++)
            {
                for (int z = 0; z < Dimensions.z; z++)
                    if(MapPoints[x,y,z]<=Threshold)
                    {
                        Gizmos.color = new Color(MapPoints[x, y, z],MapPoints[x, y, z],MapPoints[x, y, z]);
                        if (Gizmos.color ==Color.black)
                            continue;
                        Gizmos.DrawSphere(new Vector3(x,y,z),0.1f);
                    }
            }
        }
    }
}
