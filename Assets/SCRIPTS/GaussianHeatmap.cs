using UnityEngine;
using System.Collections.Generic;

public class GaussianHeatmap : MonoBehaviour
{
    [Header("Heatmap Settings")]
    public int resolution = 512;          // Resolución de la textura
    public float radius = 8f;             // Radio del blur gaussiano
    public Gradient gradient;             // Colores del heatmap

    [Header("Map Settings")]
    public MeshRenderer mapRenderer;      // Mesh del nivel
    public Material heatmapMaterial;      // Material para el Quad del heatmap

    [Header("Height Settings")] 
    public float heightOffset = 0.5f; // Ajustable desde el inspector

    private Renderer spawnedRenderer;

    public void Generate(List<Vector3> positions)
    {
        if (mapRenderer == null)
        {
            Debug.LogError("Debes asignar el MeshRenderer del mapa.");
            return;
        }

        // 1. Obtener límites del mapa
        Bounds b = mapRenderer.bounds;

        float minX = b.min.x;
        float maxX = b.max.x;
        float minZ = b.min.z;
        float maxZ = b.max.z;

        // 2. Crear textura del heatmap
        Texture2D tex = new Texture2D(resolution, resolution);
        float[,] buffer = new float[resolution, resolution];

        // 3. Pintar blur gaussiano por cada punto
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

        // 4. Normalizar y colorear
        float maxVal = 0f;
        foreach (float v in buffer)
            if (v > maxVal) maxVal = v;

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float t = maxVal > 0 ? buffer[x, y] / maxVal : 0f;
                tex.SetPixel(x, y, gradient.Evaluate(t));
            }
        }

        tex.Apply();

        // 5. Spawnear el plano del heatmap
        SpawnHeatmapPlane(tex, minX, maxX, minZ, maxZ);
    }

    private void SpawnHeatmapPlane(Texture2D tex, float minX, float maxX, float minZ, float maxZ)
    {
        if (spawnedRenderer != null)
            Destroy(spawnedRenderer.gameObject);

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.name = "HeatmapPlane";

        plane.transform.rotation = Quaternion.Euler(90, 0, 0);

        float width = maxX - minX;
        float height = maxZ - minZ;

        plane.transform.localScale = new Vector3(width, height, 1);

        float centerX = (minX + maxX) / 2f;
        float centerZ = (minZ + maxZ) / 2f;

        // 1. Raycast para encontrar el suelo real
        float rayHeight = 500f;
        Vector3 rayOrigin = new Vector3(centerX, rayHeight, centerZ);

        RaycastHit hit;
        float groundY = 0f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity))
        {
            groundY = hit.point.y;
        }
        else
        {
            groundY = mapRenderer.bounds.min.y;
        }

        // 2. Usamos heightOffset para subirlo más
        plane.transform.position = new Vector3(centerX, groundY + heightOffset, centerZ);

        spawnedRenderer = plane.GetComponent<Renderer>();
        spawnedRenderer.material = new Material(heatmapMaterial);
        spawnedRenderer.material.mainTexture = tex;

        Debug.Log("<color=green>Heatmap spawneado por encima del suelo con offset.</color>");
    }


}
