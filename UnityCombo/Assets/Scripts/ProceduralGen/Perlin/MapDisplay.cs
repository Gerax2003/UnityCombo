using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(MapGenerator))]
public class MapDisplay : MonoBehaviour
{
    [SerializeField]
    Color color1 = Color.white;
    [SerializeField]
    Color color2 = Color.black;
    [SerializeField]
    float maxHeight = 2f;

    [SerializeField]
    GameObject plane;

    Texture2D texture;

    MapGenerator generator;

    // Start is called before the first frame update
    void Start()
    {
        generator = GetComponent<MapGenerator>();

        if (plane == null)
            return;

        RefreshMap();
    }

    public void RefreshMap()
    {
        if (generator == null)
            generator = GetComponent<MapGenerator>();

        if (plane == null)
            return;

        texture = new Texture2D(generator.size, generator.size);
        texture.SetPixels(generator.GetMapAsColor());
        texture.filterMode = FilterMode.Point;
        texture.Apply();

        Material mat = plane.GetComponent<Renderer>().sharedMaterial;
        mat.mainTexture = texture;
        mat.SetVector("_Color1", color1);
        mat.SetVector("_Color2", color2);

        GenerateDefResPlane(100);
        DisplaceVertices();
    }

    void GenerateDefResPlane(int res)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Noise Terrain";

        List<Vector3> vert = new List<Vector3>(); // Vertices representing the plane
        List<Vector2> uvs = new List<Vector2>(); // UVs are used to display the noise on our mesh
        List<Vector3> normals = new List<Vector3>(); // normals are only used to not break the mesh
        List<int> indices = new List<int>(); // Indices telling the GPU how the triangles are made

        for (int i = 0; i < res; i++)
        {
            float uvX = (float)i / (res-1);
            for (int j = 0; j < res; j++)
            {
                float uvY = (float)j / (res-1);

                normals.Add(Vector3.up); 
                uvs.Add(new Vector2(uvX, uvY)); 
                vert.Add(new Vector3(-5f + uvX * 10f, 0, -5f + uvY * 10f)); // The Unity plane is in range [-5, 5], and displacement was made first with these dimensions so it's easier to debug the scale

                if (i >= res-1 ||j  >= res-1)
                    continue; // if last row/column, no triangles are made starting from this vertex

                int quadStart = i + res * j;
                // tri 1
                indices.Add(quadStart); // top left corner
                indices.Add(quadStart + res + 1); // bot right corner
                indices.Add(quadStart + 1); // top right corner

                // tri 2
                indices.Add(quadStart); // top left corner
                indices.Add(quadStart+res); // bot left corner
                indices.Add(quadStart+res+1); // bot right corner
            }
        }

        // Fill mesh with arrays
        mesh.vertices = vert.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();

        plane.GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void DisplaceVertices()
    {
        Mesh mesh = plane.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vert = mesh.vertices;
        
        // We sample the texture for our noise as the base array is somehow different from it
        for (int i = 0; i < vert.Length; i++)
            vert[i].y = texture.GetPixel((int)(mesh.uv[i].x * (texture.Size().x - 1)), (int)(mesh.uv[i].y * (texture.Size().x - 1))).r * maxHeight;
        
        mesh.vertices = vert;
    }
}
