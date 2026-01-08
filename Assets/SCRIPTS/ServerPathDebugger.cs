using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ServerPathVisualizer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AnalyticsDataFetcher dataFetcher;

    [Header("Ajustes de Tiempo Real")]
    [SerializeField] private bool autoRefresh = true;
    [Tooltip("Segundos entre cada actualización de datos del servidor")]
    [SerializeField] private float refreshInterval = 3f; 

    [Header("Ajustes de Visualización")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private float pointSize = 0.2f;
    [SerializeField] private Color currentSessionColor = Color.green;
    [SerializeField] private Color pastSessionsColor = Color.red;

    [Header("Filtros")]
    [SerializeField] private bool showPastSessions = true;

    private class SessionGroup
    {
        public string id;
        public List<Vector3> points = new List<Vector3>();
        public bool isCurrent;
    }

    private List<SessionGroup> sessionPaths = new List<SessionGroup>();
    private float nextRefreshTime;
    private bool isFetching = false;

    private void Start()
    {
        if (dataFetcher == null) dataFetcher = GetComponent<AnalyticsDataFetcher>();
        
        // Hacemos una descarga inicial inmediata
        RefreshPaths();
    }

    private void Update()
    {
        // Si el auto-refresco está activo, no estamos descargando ya, y ha pasado el tiempo...
        if (autoRefresh && !isFetching && Time.time >= nextRefreshTime)
        {
            RefreshPaths();
            nextRefreshTime = Time.time + refreshInterval;
        }
    }

    [ContextMenu("Forzar Actualización Manual")]
    public void RefreshPaths()
    {
        if (dataFetcher == null) return;

        isFetching = true;
        // Usamos el método de tu fetcher original
        dataFetcher.DownloadEventData("PlayerPosition", (json) => {
            ParseAndOrganizeData(json);
            isFetching = false;
        });
    }

    private void ParseAndOrganizeData(string json)
    {
        if (string.IsNullOrEmpty(json) || json == "[]") return;

        // Utilizamos tu JsonHelper existente
        AnalyticsDataFetcher.PlayerPositionData[] allData = AnalyticsDataFetcher.JsonHelper.FromJson<AnalyticsDataFetcher.PlayerPositionData>(json);
        
        if (allData == null || allData.Length == 0) return;

        // Organizamos los datos por sesión
        var groups = allData.GroupBy(d => d.session_id);
        
        // Obtenemos la ID de la sesión actual desde el AnalyticsManager
        string currentSessionId = "";
        if (AnalyticsManager.Instance != null)
        {
            currentSessionId = GetSessionIdRef();
        }

        // Limpiamos y reconstruimos la lista de rutas
        List<SessionGroup> newPaths = new List<SessionGroup>();

        foreach (var group in groups)
        {
            SessionGroup newSession = new SessionGroup
            {
                id = group.Key,
                isCurrent = (group.Key == currentSessionId),
                // Ordenamos por timestamp cronológicamente
                points = group.OrderBy(d => d.timestamp) 
                              .Select(d => new Vector3(d.x, d.y, d.z))
                              .ToList()
            };
            newPaths.Add(newSession);
        }

        sessionPaths = newPaths;
    }

    private string GetSessionIdRef()
    {
        // Acceso por reflexión a la variable privada sessionId del AnalyticsManager
        var field = typeof(AnalyticsManager).GetField("sessionId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(AnalyticsManager.Instance) as string ?? "";
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || sessionPaths == null || sessionPaths.Count == 0) return;

        foreach (var session in sessionPaths)
        {
            if (!session.isCurrent && !showPastSessions) continue;

            Gizmos.color = session.isCurrent ? currentSessionColor : pastSessionsColor;

            for (int i = 0; i < session.points.Count - 1; i++)
            {
                Gizmos.DrawLine(session.points[i], session.points[i + 1]);
                Gizmos.DrawSphere(session.points[i], pointSize);
            }

            // Indicador de inicio de sesión (Cubo blanco)
            if (session.points.Count > 0)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(session.points[0], Vector3.one * pointSize * 2);
            }
        }
    }
}