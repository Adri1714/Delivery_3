using UnityEngine;
using System.Collections.Generic;

public class HeatmapController : MonoBehaviour
{
    private AnalyticsDataFetcher fetcher;
    private GaussianHeatmap heatmap;

    private void Start()
    {
        fetcher = FindObjectOfType<AnalyticsDataFetcher>();
        heatmap = FindObjectOfType<GaussianHeatmap>();

        if (fetcher == null || heatmap == null)
        {
            Debug.LogError("Faltan componentes: AnalyticsDataFetcher o GaussianHeatmap.");
            return;
        }

        // Llamamos al fetcher sin modificarlo
        fetcher.DownloadEventData("PlayerPosition", OnPositionsDownloaded);
    }

    private void OnPositionsDownloaded(string json)
    {
        var data = AnalyticsDataFetcher.JsonHelper.FromJson<AnalyticsDataFetcher.PlayerPositionData>(json);

        List<Vector3> positions = new List<Vector3>();
        foreach (var item in data)
            positions.Add(new Vector3(item.x, item.y, item.z));

        heatmap.Generate(positions);

        Debug.Log("<color=cyan>Heatmap Gaussiano generado automáticamente al iniciar la escena.</color>");
    }
}
