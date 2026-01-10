using UnityEngine;
using System.Collections.Generic;

public class HeatmapController : MonoBehaviour
{
    [Header("Referencias")]
    private AnalyticsDataFetcher fetcher;
    private GaussianHeatmap heatmap;

    [Header("Auto-generar al inicio")]
    [Tooltip("Descargar y generar heatmap automáticamente al iniciar")]
    public bool autoGenerate = true;

    private void Start()
    {
        fetcher = FindObjectOfType<AnalyticsDataFetcher>();
        heatmap = FindObjectOfType<GaussianHeatmap>();

        if (autoGenerate)
        {
            GenerateHeatmap();
        }
    }

    private void Update()
    {
        // Actualizar controles desde InteractionControl
        if (heatmap != null)
        {
            heatmap.UpdateFromInteractionControl();
        }
    }

    public void GenerateHeatmap()
    {
        if (fetcher == null)
        {
            Debug.LogError("No se encontró AnalyticsDataFetcher en la escena");
            return;
        }

        fetcher.DownloadEventData("PlayerPosition", OnPositionsDownloaded);
    }

    private void OnPositionsDownloaded(string json)
    {
        var data = AnalyticsDataFetcher.JsonHelper.FromJson<AnalyticsDataFetcher.PlayerPositionData>(json);

        List<Vector3> positions = new List<Vector3>();
        foreach (var item in data)
            positions.Add(new Vector3(item.x, item.y, item.z));

        if (heatmap != null)
        {
            heatmap.Generate(positions);
            Debug.Log($"<color=cyan>Heatmap generado con {positions.Count} posiciones</color>");
        }
    }

    public void ShowHeatmap()
    {
        if (InteractionControl.Instance != null)
            InteractionControl.Instance.showHeatmap = true;
    }

    public void HideHeatmap()
    {
        if (InteractionControl.Instance != null)
            InteractionControl.Instance.showHeatmap = false;
    }

    public void ToggleHeatmap()
    {
        if (InteractionControl.Instance != null)
            InteractionControl.Instance.showHeatmap = !InteractionControl.Instance.showHeatmap;
    }
}
