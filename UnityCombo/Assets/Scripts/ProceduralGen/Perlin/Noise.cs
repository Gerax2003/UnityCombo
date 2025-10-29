using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int size = 512, float scale = 1f, Vector2 offset = default, Vector2 octavesInfluence = default)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
            {
                float n = Mathf.PerlinNoise((float)(i + offset.x) / size * scale, (float)(j + offset.y) / size * scale)
                    + octavesInfluence.x * Mathf.PerlinNoise((float)(i + offset.x) / size * scale * octavesInfluence.x, (float)(j + offset.y) / size * scale * octavesInfluence.x)
                    + octavesInfluence.y * Mathf.PerlinNoise((float)(i + offset.x) / size * scale * octavesInfluence.y, (float)(j + offset.y) / size * scale * octavesInfluence.y);
                
                map[i, j] = n / (1 + octavesInfluence.x + octavesInfluence.y);
            }

        return map;
    }
}
