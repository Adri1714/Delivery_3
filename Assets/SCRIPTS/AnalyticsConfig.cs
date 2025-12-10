using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AnalyticsConfig", menuName = "Tools/Analytics Config")]
public class AnalyticsConfig : ScriptableObject
{
    public string serverUrl = "https://citmalumnes.upc.es/~adriarj/analytics/";
    public List<AnalyticsEvent> events = new List<AnalyticsEvent>();
}

[System.Serializable]
public class AnalyticsEvent
{
    public string eventName;
    public List<Parameter> parameters;
}

[System.Serializable]
public class Parameter
{
    public string paramName;
    public ParamType type;
}

public enum ParamType { String, Int, Float, Bool, Vector2, Vector3, DateTime }