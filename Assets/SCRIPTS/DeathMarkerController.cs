using UnityEngine;
using System.Collections.Generic;

public class DeathMarkerController : MonoBehaviour
{
    private AnalyticsDataFetcher fetcher;
    private DeathMarkerSpawner spawner;

    [Header("Auto-generar al inicio")]
    public bool autoGenerate = true;

    private void Start()
    {
        fetcher = FindObjectOfType<AnalyticsDataFetcher>();
        spawner = FindObjectOfType<DeathMarkerSpawner>();

        if (autoGenerate)
            GenerateDeathMarkers();
    }

    public void GenerateDeathMarkers()
    {
        if (fetcher == null)
        {
            Debug.LogError("No se encontró AnalyticsDataFetcher");
            return;
        }

        fetcher.DownloadEventData("PlayerKilled", OnDeathDataDownloaded);
    }

    private void OnDeathDataDownloaded(string json)
    {
        var data = AnalyticsDataFetcher.JsonHelper.FromJson<AnalyticsDataFetcher.PlayerKilledData>(json);

        List<Vector3> positions = new List<Vector3>();
        foreach (var item in data)
            positions.Add(new Vector3(item.x, item.y, item.z));

        spawner.SpawnMarkers(positions);

        Debug.Log($"Marcadores de muerte generados: {positions.Count}");
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
