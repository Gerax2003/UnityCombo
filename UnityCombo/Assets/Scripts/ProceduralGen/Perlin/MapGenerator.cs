using Palmmedia.ReportGenerator.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MapGenerator : MonoBehaviour
{
    [SerializeField, Range(1, 2048)]
    public int size = 64;

    [SerializeField, Range(0.1f, 16f)]
    public float frequency = 4f;

    [SerializeField]
    public Vector2 offset = Vector2.zero;

    [SerializeField]
    public Vector2 octavesInfluence = Vector2.zero;


    [HideInInspector]
    public float[,] map = null;

    // Start is called before the first frame update
    void Start()
    {
        if (map==null)
            map = Noise.GenerateNoiseMap(size, frequency, offset, octavesInfluence);
    }

    public Color[] GetMapAsColor()
    {
        if (map==null)
            return new Color[0];

        Color[] colorMap = new Color[size * size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
                colorMap[i + j * size] = new Color(map[i, j], map[i, j], map[i, j]);
        }

        return colorMap;
    }

    public Vector2 GetMax()
    {
        Vector2 res = new Vector2(1, 0);
        for (int i = 0; i < size;i++)
        {
            for(int j = 0;j < size;j++)
            {
                res.x = Mathf.Min(map[i, j], res.x);
                res.y = Mathf.Max(map[i, j], res.y);
            }
        }
        return res;
    }

    public void ReverseMap()
    {
        float[,] temp = new float[size, size];

        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                temp[i,j] = map[size - 1 - i, size - 1 - j];

        map = temp;
    }
}

[CustomEditor(typeof(MapGenerator))]
class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Generate"))
        {
            MapGenerator generator = (MapGenerator)target;

            if (generator != null)
                generator.map = Noise.GenerateNoiseMap(generator.size, generator.frequency, generator.offset, generator.octavesInfluence);

            MapDisplay display = generator.gameObject.GetComponent<MapDisplay>();

            display.RefreshMap();
        }
    }
}