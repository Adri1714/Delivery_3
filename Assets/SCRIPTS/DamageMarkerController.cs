using UnityEngine;
using System.Collections.Generic;

public class DamageMarkerController : MonoBehaviour
{
    private AnalyticsDataFetcher fetcher;
    private DamageMarkerSpawner spawner;

    [Header("Auto-generar al inicio")]
    public bool autoGenerate = true;

    private void Start()
    {
        fetcher = FindObjectOfType<AnalyticsDataFetcher>();
        spawner = FindObjectOfType<DamageMarkerSpawner>();

        if (autoGenerate)
            GenerateDamageMarkers();
    }

    public void GenerateDamageMarkers()
    {
        if (fetcher == null)
        {
            Debug.LogError("No se encontró AnalyticsDataFetcher");
            return;
        }

        fetcher.DownloadEventData("PlayerDamaged", OnDamageDataDownloaded);
    }

    private void OnDamageDataDownloaded(string json)
    {
        var data = AnalyticsDataFetcher.JsonHelper.FromJson<AnalyticsDataFetcher.PlayerDamagedData>(json);

        List<Vector3> positions = new List<Vector3>();
        foreach (var item in data)
            positions.Add(new Vector3(item.x, item.y, item.z));

        spawner.SpawnMarkers(positions);

        Debug.Log($"Marcadores de daño generados: {positions.Count}");
    }

    public void ShowMarkers()
    {
        spawner.SetVisible(true);
    }

    public void HideMarkers()
    {
        spawner.SetVisible(false);
    }

    public void ToggleMarkers()
    {
        if (spawner != null)
            spawner.SetVisible(!spawner.gameObject.activeSelf);
    }
}
