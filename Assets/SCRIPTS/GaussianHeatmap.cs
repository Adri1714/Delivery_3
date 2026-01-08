using UnityEngine;
using System.Collections.Generic;

public class GaussianHeatmap : MonoBehaviour
{
    public int resolution = 256;
    public float radius = 8f;
    public Gradient gradient;
    public Renderer targetRenderer;

    public void Generate(List<Vector3> positions)
    {
        Texture2D tex = new Texture2D(resolution, resolution);
        float[,] buffer = new float[resolution, resolution];

        // 1. Obtener límites del mapa
        float minX = Mathf.Min(positions.ConvertAll(p => p.x).ToArray());
        float maxX = Mathf.Max(positions.ConvertAll(p => p.x).ToArray());
        float minZ = Mathf.Min(positions.ConvertAll(p => p.z).ToArray());
        float maxZ = Mathf.Max(positions.ConvertAll(p => p.z).ToArray());

        // 2. Pintar un blur gaussiano por cada punto
        foreach (var pos in positions)
        {
            int cx = Mathf.RoundToInt(Mathf.InverseLerp(minX, maxX, pos.x) * (resolution - 1));
            int cy = Mathf.RoundToInt(Mathf.InverseLerp(minZ, maxZ, pos.z) * (resolution - 1));

            int rad = Mathf.RoundToInt(radius);

            for (int x = -rad; x <= rad; x++)
            {
                for (int y = -rad; y <= rad; y++)
                {
                    int px = cx + x;
                    int py = cy + y;

                    if (px < 0 || px >= resolution || py < 0 || py >= resolution)
                        continue;

                    float dist = Mathf.Sqrt(x * x + y * y);
                    float value = Mathf.Exp(-(dist * dist) / (2 * radius * radius));

                    buffer[px, py] += value;
                }
            }
        }

        // 3. Normalizar y colorear
        float maxVal = 0f;
        foreach (float v in buffer)
            if (v > maxVal) maxVal = v;

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float t = buffer[x, y] / maxVal;
                tex.SetPixel(x, y, gradient.Evaluate(t));
            }
        }

        tex.Apply();
        targetRenderer.material.mainTexture = tex;
    }
}
