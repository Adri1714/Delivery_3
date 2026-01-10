using UnityEngine;
using System.Collections.Generic;

public class DeathMarkerSpawner : MonoBehaviour
{
    [Header("Prefab del marcador de muerte")]
    public GameObject deathMarkerPrefab;

    [Header("Altura sobre el suelo")]
    public float heightOffset = 0.2f;

    [Header("Desfase para evitar solapamientos")]
    public float overlapOffsetRadius = 0.25f;

    private List<GameObject> spawnedMarkers = new List<GameObject>();

    public void SpawnMarkers(List<Vector3> positions)
    {
        ClearMarkers();

        Dictionary<Vector3, int> positionCounts = new Dictionary<Vector3, int>();

        foreach (var pos in positions)
        {
            Vector3 basePos = pos;

            if (!positionCounts.ContainsKey(basePos))
                positionCounts[basePos] = 0;

            int count = positionCounts[basePos];
            positionCounts[basePos]++;

            Vector3 offset = Vector3.zero;

            if (count > 0)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float radius = overlapOffsetRadius * count;
                offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            }

            Vector3 spawnPos = basePos + offset;

            if (Physics.Raycast(spawnPos + Vector3.up * 100f, Vector3.down, out RaycastHit hit, Mathf.Infinity))
            {
                spawnPos = hit.point + Vector3.up * heightOffset;
            }
            else
            {
                spawnPos.y += heightOffset;
            }

            GameObject marker = Instantiate(deathMarkerPrefab, spawnPos, Quaternion.identity);
            spawnedMarkers.Add(marker);
        }

        Debug.Log($"Instanciados {spawnedMarkers.Count} marcadores de muerte");
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
