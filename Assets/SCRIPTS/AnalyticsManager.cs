using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance;

    [Header("Configuración")]
    public AnalyticsConfig config; // Arrastra tu ScriptableObject aquí
    public bool debugMode = true;  // Si es true, imprime logs en consola

    private string collectorUrl;
    private string sessionId; // Variable para la sesión actual

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Generamos un ID único al iniciar el juego (una nueva sesión)
            sessionId = System.Guid.NewGuid().ToString(); 
            
            if (config != null)
                collectorUrl = config.serverUrl + "collector.php";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Llama a esta función para trackear un evento.
    /// </summary>
    /// <param name="eventName">El nombre exacto del evento definido en el Config</param>
    /// <param name="parameters">Diccionario con los datos (nombre_param, valor)</param>
    public void TrackEvent(string eventName, Dictionary<string, object> parameters)
    {
        // 1. Validar si el evento existe en la config (Opcional, para evitar errores de typos)
        if (!IsEventDefined(eventName))
        {
            if (debugMode) Debug.LogWarning($"Analytics: El evento '{eventName}' no está definido en el Config.");
            return;
        }
        if (!parameters.ContainsKey("session_id"))
        {
            parameters.Add("session_id", sessionId);
        }

        // 2. Serializar los parámetros a JSON manualmente (Unity JsonUtility es malo con Diccionarios)
        string jsonPayload = SerializeDictionary(parameters);

        // 3. Enviar al servidor
        StartCoroutine(PostRequest(eventName, jsonPayload));
    }

    private IEnumerator PostRequest(string eventName, string jsonData)
    {
        WWWForm form = new WWWForm();
        form.AddField("eventName", eventName);
        form.AddField("data", jsonData);

        using (UnityWebRequest uwr = UnityWebRequest.Post(collectorUrl, form))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                if (debugMode) Debug.LogError("Analytics Error: " + uwr.error);
            }
            else
            {
                if (debugMode) Debug.Log($"Analytics Enviado: {eventName} -> {uwr.downloadHandler.text}");
            }
        }
    }

    private bool IsEventDefined(string name)
    {
        if (config == null) return false;
        foreach (var evt in config.events)
        {
            if (evt.eventName == name) return true;
        }
        return false;
    }



    #region JSON Serialization
    //----------------------------------------------------------//
    //------------- Serialización Manual a JSON ----------------//
    //----------------------------------------------------------//
    private string SerializeDictionary(Dictionary<string, object> dict)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        int count = 0;
        foreach (var pair in dict)
        {
            sb.Append($"\"{pair.Key}\":");
            sb.Append(SerializeValue(pair.Value));

            count++;
            if (count < dict.Count) sb.Append(",");
        }
        sb.Append("}");
        return sb.ToString();
    }
    private string SerializeValue(object value)
    {
        if (value == null) return "null";

        if (value is string s)
            return $"\"{EscapeString(s)}\"";

        if (value is bool b)
            return b ? "true" : "false";

        if (value is int i)
            return i.ToString();

        if (value is float f)
            return f.ToString(CultureInfo.InvariantCulture);

        if (value is double d)
            return d.ToString(CultureInfo.InvariantCulture);

        if (value is Vector3 v3)
            return $"{{\"x\":{v3.x.ToString(CultureInfo.InvariantCulture)},\"y\":{v3.y.ToString(CultureInfo.InvariantCulture)},\"z\":{v3.z.ToString(CultureInfo.InvariantCulture)}}}";

        if (value is Vector2 v2)
            return $"{{\"x\":{v2.x.ToString(CultureInfo.InvariantCulture)},\"y\":{v2.y.ToString(CultureInfo.InvariantCulture)}}}";

        if (value is Dictionary<string, object> dict)
            return SerializeDictionary(dict);

        if (value is IEnumerable enumerable)
            return SerializeArray(enumerable);
        if (value is System.DateTime dt)
            return $"\"{dt.ToString("o")}\"";

        // Fallback: usar ToString() como string
        return $"\"{EscapeString(value.ToString())}\"";
    }
    private string SerializeArray(IEnumerable enumerable)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        bool first = true;
        foreach (var item in enumerable)
        {
            if (!first) sb.Append(",");
            sb.Append(SerializeValue(item));
            first = false;
        }
        sb.Append("]");
        return sb.ToString();
    }
    private string EscapeString(string input)
    {
        return input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }
    #endregion
}