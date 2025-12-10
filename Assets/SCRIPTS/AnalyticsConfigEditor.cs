#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[CustomEditor(typeof(AnalyticsConfig))]
public class AnalyticsConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AnalyticsConfig config = (AnalyticsConfig)target;

        if (GUILayout.Button("Sincronizar Estructura con Servidor"))
        {
            SyncSchema(config);
        }
    }

    void SyncSchema(AnalyticsConfig config)
    {
        string json = JsonUtility.ToJson(config, true);
        string url = config.serverUrl + "setup.php";

        Debug.Log($"Conectando a: {url}...");

        // En Editor no podemos usar Corrutinas normales fácilmente, usamos una petición síncrona simple para tools
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();

        // Esperar a que termine (Hack para Editor)
        while (!operation.isDone) { }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error Sync: " + request.error + "\nRespuesta: " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("Éxito Sync: " + request.downloadHandler.text);
        }
        
        request.Dispose();
    }
}
#endif