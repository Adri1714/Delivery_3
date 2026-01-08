using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[ExecuteInEditMode]
public class ServerPathVisualizer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AnalyticsDataFetcher dataFetcher;
    [SerializeField] private InteractionControl control;

    private class VisualizationPoint
    {
        public Vector3 position;
        public string type; // "Path", "Death", "Damage"
        public DateTime timestamp;
        public string sessionId;
    }

    private List<VisualizationPoint> allPoints = new List<VisualizationPoint>();
    
    private List<VisualizationPoint> filteredPoints = new List<VisualizationPoint>();
    
    private string currentSessionId = "";

    private void OnEnable()
    {
        if (control == null) control = FindFirstObjectByType<InteractionControl>();
        if (dataFetcher == null) dataFetcher = GetComponent<AnalyticsDataFetcher>();
    }
    
    private void Start()
    {
        RefreshAllData();
    }
    
    private void Update()
    {
    }

    [ContextMenu("Fetch All Data")]
    public void RefreshAllData()
    {
        if (dataFetcher == null) return;
        
        allPoints.Clear();
        filteredPoints.Clear();

        dataFetcher.DownloadEventData("PlayerPosition", (json) => {
            ParseData<AnalyticsDataFetcher.PlayerPositionData>(json, "Path");
        });
        
        dataFetcher.DownloadEventData("PlayerDeath", (json) => {
            ParseData<AnalyticsDataFetcher.PlayerKilledData>(json, "Death");
        });

        dataFetcher.DownloadEventData("PlayerDamaged", (json) => {
            ParseData<AnalyticsDataFetcher.PlayerDamagedData>(json, "Damage");
        });
    }

    private void ParseData<T>(string json, string eventType)
    {
        if (string.IsNullOrEmpty(json) || json == "[]") return;

        try
        {
            T[] items = AnalyticsDataFetcher.JsonHelper.FromJson<T>(json);

            foreach (var item in items)
            {
                try 
                {
                    var type = item.GetType();
                    
                    float x = Convert.ToSingle(type.GetField("x").GetValue(item));
                    float y = Convert.ToSingle(type.GetField("y").GetValue(item));
                    float z = Convert.ToSingle(type.GetField("z").GetValue(item));
                    string sessionId = type.GetField("session_id").GetValue(item).ToString();
                    string timestamp = type.GetField("timestamp").GetValue(item).ToString();
                    
                    VisualizationPoint p = new VisualizationPoint();
                    p.position = new Vector3(x, y, z);
                    p.type = eventType;
                    p.sessionId = sessionId;
                    
                    if(DateTime.TryParse(timestamp, out DateTime dt))
                        p.timestamp = dt;
                    else
                        p.timestamp = DateTime.Now;
                    
                    allPoints.Add(p);
                }
                catch (System.Exception) { }
            }

            OnSettingsChanged();
        }
        catch (System.Exception) { }
    }

    public void OnSettingsChanged()
    {
        if (control == null) return;
        if (allPoints.Count == 0) return;

        filteredPoints.Clear();

        var sorted = allPoints.OrderBy(p => p.sessionId).ThenBy(p => p.timestamp).ToList();
        if (sorted.Count == 0) return;
        
        var sortedByTime = allPoints.OrderBy(p => p.timestamp).ToList();
        currentSessionId = sortedByTime.LastOrDefault()?.sessionId ?? "";

        foreach (var p in allPoints)
        {
            if (p.type == "Path" && !control.showPaths) continue;
            if (p.type == "Death" && !control.showDeaths) continue;
            if (p.type == "Damage" && !control.showDamage) continue;

            filteredPoints.Add(p);
        }
    }

    private void OnDrawGizmos()
    {
        if (filteredPoints == null || control == null) return;
        if (filteredPoints.Count == 0) return;

        int pathCount = 0;
        int deathCount = 0;
        int damageCount = 0;
        
        var pathsBySession = filteredPoints.Where(p => p.type == "Path").GroupBy(p => p.sessionId).ToList();
        
        for (int i = 0; i < filteredPoints.Count; i++)
        {
            var p = filteredPoints[i];
            bool isCurrentSession = p.sessionId == currentSessionId;
            
            if (!isCurrentSession && !control.showOldSessions && p.type == "Path") continue;
            
            if (p.type == "Path")
            {
                pathCount++;
                
                var sessionPoints = filteredPoints.Where(pt => pt.sessionId == p.sessionId && pt.type == "Path").ToList();
                int sessionIndex = sessionPoints.IndexOf(p);
                float t = sessionPoints.Count > 1 ? (float)sessionIndex / (sessionPoints.Count - 1) : 0.5f;
                
                Gradient gradient = isCurrentSession ? control.pathGradient : control.oldPathGradient;
                float intensity = isCurrentSession ? control.visualIntensity : control.oldVisualIntensity;
                
                Color color;
                if (gradient != null)
                {
                    color = gradient.Evaluate(t);
                }
                else
                {
                    if (control.pathGradient != null)
                    {
                        color = control.pathGradient.Evaluate(t);
                    }
                    else
                    {
                        color = Color.Lerp(Color.blue, Color.red, t);
                    }
                }
                
                if (!isCurrentSession)
                {
                    color.a *= control.oldSessionsAlpha;
                }
                
                Gizmos.color = color;
                Gizmos.DrawSphere(p.position, intensity * 0.5f);
                
                if (i > 0 && filteredPoints[i-1].type == "Path" && filteredPoints[i-1].sessionId == p.sessionId)
                {
                    Gizmos.DrawLine(filteredPoints[i-1].position, p.position);
                }
            }
            else if (p.type == "Death")
            {
                deathCount++;
                Gizmos.color = control.deathColor;
                Gizmos.DrawCube(p.position, Vector3.one * control.visualIntensity); // Cubo para muertes
                Gizmos.DrawWireCube(p.position, Vector3.one * control.visualIntensity * 1.5f);
            }
            else if (p.type == "Damage")
            {
                damageCount++;
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(p.position, control.visualIntensity * 0.8f);
            }
        }
        
    }
}