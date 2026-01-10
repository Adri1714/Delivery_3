using UnityEngine;
using System.Collections.Generic;

public class DamageMarkerSpawner : MonoBehaviour
{
    [Header("Prefab del marcador de daño")]
    public GameObject damageMarkerPrefab;

    [Header("Altura sobre el suelo")]
    public float heightOffset = 0.2f;

    [Header("Desfase para evitar solapamientos")]
    public float overlapOffsetRadius = 0.25f; // Radio del desplazamiento

    private List<GameObject> spawnedMarkers = new List<GameObject>();

    public void SpawnMarkers(List<Vector3> positions)
    {
        ClearMarkers();

        // Diccionario para contar cuántas veces aparece cada posición
        Dictionary<Vector3, int> positionCounts = new Dictionary<Vector3, int>();

        foreach (var pos in positions)
        {
            Vector3 basePos = pos;

            // Contar cuántas veces se ha usado esta posición
            if (!positionCounts.ContainsKey(basePos))
                positionCounts[basePos] = 0;

            int count = positionCounts[basePos];
            positionCounts[basePos]++;

            // Si count > 0, significa que ya hay un marcador en esa posición
            // Aplicamos un pequeño desplazamiento circular
            Vector3 offset = Vector3.zero;

            if (count > 0)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float radius = overlapOffsetRadius * count; // más repetidos ? más separación

                offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            }

            Vector3 spawnPos = basePos + offset;

            // Ajustar altura con raycast
            if (Physics.Raycast(spawnPos + Vector3.up * 100f, Vector3.down, out RaycastHit hit, Mathf.Infinity))
            {
                spawnPos = hit.point + Vector3.up * heightOffset;
            }
            else
            {
                spawnPos.y += heightOffset;
            }

            GameObject marker = Instantiate(damageMarkerPrefab, spawnPos, Quaternion.identity);
            spawnedMarkers.Add(marker);
        }

        Debug.Log($"Instanciados {spawnedMarkers.Count} marcadores de daño ");
    }

    public void ClearMarkers()
    {
        foreach (var m in spawnedMarkers)
            if (m != null) Destroy(m);

        spawnedMarkers.Clear();
    }
    public void SetVisible(bool visible)
    {
        foreach (var m in spawnedMarkers)
            if (m != null) m.SetActive(visible);
    }
}
