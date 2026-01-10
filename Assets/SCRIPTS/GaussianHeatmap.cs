using UnityEngine;
using System.Collections.Generic;

public class GaussianHeatmap : MonoBehaviour
{
    [Header("Heatmap Settings")]
    public int resolution = 512;
    public float radius = 8f;
    public Gradient gradient;

    [Header("Map Settings")]
    public MeshRenderer mapRenderer;
    public Material heatmapMaterial;

    [Header("Height Settings")]
    public float heightOffset = 0.5f;

    private Renderer spawnedRenderer;
    private Material instancedMaterial;
    private float baseGroundHeight;
    private float lastHeightOffset = -1f;

    public void UpdateFromInteractionControl()
    {
        if (spawnedRenderer == null || InteractionControl.Instance == null) return;

        // Mostrar / ocultar heatmap
        spawnedRenderer.enabled = InteractionControl.Instance.showHeatmap;

        // Actualizar altura si cambió el offset
        if (!Mathf.Approximately(lastHeightOffset, InteractionControl.Instance.heatmapHeightOffset))
        {
            lastHeightOffset = InteractionControl.Instance.heatmapHeightOffset;

            Vector3 pos = spawnedRenderer.transform.position;
            spawnedRenderer.transform.position = new Vector3(
                pos.x,
                baseGroundHeight + lastHeightOffset,
                pos.z
            );
        }
    }

    public void Generate(List<Vector3> positions)
    {
        if (mapRenderer == null)
        {
            Debug.LogError("Debes asignar el MeshRenderer del mapa.");
            return;
        }

        // Valores dinámicos desde InteractionControl
        int currentResolution = InteractionControl.Instance != null ? InteractionControl.Instance.heatmapResolution : resolution;
        float currentRadius = InteractionControl.Instance != null ? InteractionControl.Instance.heatmapBlurRadius : radius;
        int smoothingPasses = InteractionControl.Instance != null ? InteractionControl.Instance.heatmapSmoothing : 1;

        // 1. Límites del mapa
        Bounds b = mapRenderer.bounds;

        float minX = b.min.x;
        float maxX = b.max.x;
        float minZ = b.min.z;
        float maxZ = b.max.z;

        //  Crear textura
        Texture2D tex = new Texture2D(currentResolution, currentResolution);
        float[,] buffer = new float[currentResolution, currentResolution];

        //  Pintar blur gaussiano
        foreach (var pos in positions)
        {
            int cx = Mathf.RoundToInt(Mathf.InverseLerp(minX, maxX, pos.x) * (currentResolution - 1));
            int cy = Mathf.RoundToInt(Mathf.InverseLerp(minZ, maxZ, pos.z) * (currentResolution - 1));

            int rad = Mathf.RoundToInt(currentRadius);

            for (int x = -rad; x <= rad; x++)
            {
                for (int y = -rad; y <= rad; y++)
                {
                    int px = cx + x;
                    int py = cy + y;

                    if (px < 0 || px >= currentResolution || py < 0 || py >= currentResolution)
                        continue;

                    float dist = Mathf.Sqrt(x * x + y * y);
                    float value = Mathf.Exp(-(dist * dist) / (2 * currentRadius * currentRadius));

                    buffer[px, py] += value;
                }
            }
        }

        // Normalización logarítmica (evita que un punto domine todo)
        float maxVal = 0f;
        foreach (float v in buffer)
            if (v > maxVal) maxVal = v;

        for (int x = 0; x < currentResolution; x++)
        {
            for (int y = 0; y < currentResolution; y++)
            {
                float t = maxVal > 0
                    ? Mathf.Log(1 + buffer[x, y]) / Mathf.Log(1 + maxVal)
                    : 0f;

                tex.SetPixel(x, y, gradient.Evaluate(t));
            }
        }

        tex.Apply();

        // Suavizado opcional
        if (smoothingPasses > 0)
            tex = ApplySmoothing(tex, smoothingPasses);

        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        // Spawnear el heatmap
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

        // Raycast para encontrar el suelo real
        float rayHeight = 500f;
        Vector3 rayOrigin = new Vector3(centerX, rayHeight, centerZ);

        RaycastHit hit;
        baseGroundHeight = 0f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity))
            baseGroundHeight = hit.point.y;
        else
            baseGroundHeight = mapRenderer.bounds.min.y;

        // Offset inicial
        float initialOffset = InteractionControl.Instance != null ? InteractionControl.Instance.heatmapHeightOffset : heightOffset;
        lastHeightOffset = initialOffset;

        plane.transform.position = new Vector3(centerX, baseGroundHeight + initialOffset, centerZ);

        spawnedRenderer = plane.GetComponent<Renderer>();

        instancedMaterial = new Material(heatmapMaterial);
        instancedMaterial.mainTexture = tex;
        spawnedRenderer.material = instancedMaterial;

        UpdateFromInteractionControl();

        Debug.Log("Heatmap spawneado con normalización logarítmica y altura dinámica");
    }

    private Texture2D ApplySmoothing(Texture2D source, int passes)
    {
        int width = source.width;
        int height = source.height;
        Color[] pixels = source.GetPixels();
        Color[] newPixels = new Color[pixels.Length];

        for (int pass = 0; pass < passes; pass++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color sum = Color.black;
                    int count = 0;

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                sum += pixels[ny * width + nx];
                                count++;
                            }
                        }
                    }

                    newPixels[y * width + x] = sum / count;
                }
            }

            System.Array.Copy(newPixels, pixels, pixels.Length);
        }

        source.SetPixels(newPixels);
        source.Apply();
        return source;
    }
}
