using System.Collections.Generic;
using UnityEngine;
public class HeatmapGenerator : MonoBehaviour 
{ 
    [Header("Heatmap Settings")] 

    public int gridResolution = 50; 
    public float cellSize = 1f; public 
    Gradient heatGradient; public 
    Renderer targetRenderer; // El plano donde pintamos el heatmap
    private int[,] heatmap; 
    public void GenerateHeatmap(List<Vector3> positions) 
    { 
        heatmap = new int[gridResolution, gridResolution]; 

        // 1. Obtener límites del mapa
        float minX = Mathf.Min(positions.ConvertAll(p => p.x).ToArray()); 
        float maxX = Mathf.Max(positions.ConvertAll(p => p.x).ToArray()); 
        float minZ = Mathf.Min(positions.ConvertAll(p => p.z).ToArray()); 
        float maxZ = Mathf.Max(positions.ConvertAll(p => p.z).ToArray()); 
        
        // 2. Rellenar la cuadrícula
        foreach (var pos in positions) 
        { 
            int x = Mathf.FloorToInt((pos.x - minX) / cellSize); 
            int z = Mathf.FloorToInt((pos.z - minZ) / cellSize); 
            
            if (x >= 0 && x < gridResolution && z >= 0 && z < gridResolution) heatmap[x, z]++; 
        } 
        // 3. Crear textura
        Texture2D tex = new Texture2D(gridResolution, gridResolution); 
        int maxCount = 1; 
        
        foreach (int count in heatmap) 
            if (count > maxCount) maxCount = count; 
        
        for (int x = 0; x < gridResolution; x++) 
        { 
            for (int z = 0; z < gridResolution; z++) 
            { 
                float t = (float)heatmap[x, z] / maxCount; 
                tex.SetPixel(x, z, heatGradient.Evaluate(t)); 
            } 
        } 
        tex.Apply(); 
        // 4. Asignar textura al plano
        targetRenderer.material.mainTexture = tex; 
    } 
}