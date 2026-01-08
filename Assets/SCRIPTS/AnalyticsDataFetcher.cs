using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class AnalyticsDataFetcher : MonoBehaviour
{
    [Header("Configuración del Servidor")]
    public string serverUrl = "https://citmalumnes.upc.es/~adriarj/analytics/";
    [SerializeField] private string endpointPath = "fetch_data.php";

    // --- Estructuras de Datos para parsear el JSON ---
    // Deben coincidir con los nombres de las columnas en tu base de datos

    [Serializable]
    public class PlayerPositionData {
        public float x;
        public float y;
        public float z;
        public int health;
        public string timestamp;
        public string session_id;
    }

    [Serializable]
    public class PlayerDamagedData {
        public float damage;
        public float x;
        public float y;
        public float z;
        public string timestamp;
        public string session_id;
    }
    [Serializable]
    public class PlayerKilledData {
        public float x;
        public float y;
        public float z;
        public string timestamp;
        public string session_id;
    }

    // --- Métodos de ejecución ---

    [ContextMenu("Fetch Player Positions")]
    public void GetPositions() {
        // IMPORTANTE: Enviamos "PlayerPosition", el PHP le pondrá el prefijo "analytics_"
        DownloadEventData("PlayerPosition", (json) => {
            PlayerPositionData[] data = JsonHelper.FromJson<PlayerPositionData>(json);
            Debug.Log($"<color=green>Datos de Posición recibidos: {data.Length}</color>");
            foreach(var item in data) {
                Debug.Log($" Pos: ({item.x}, {item.y}, {item.z}) | Salud: {item.health} | Sesión: {item.session_id}");
            }
        });
    }
    [ContextMenu("Fetch Player Damaged Events")]
    public void GetDamagedEvents() {
        DownloadEventData("PlayerDamaged", (json) => {
            PlayerDamagedData[] data = JsonHelper.FromJson<PlayerDamagedData>(json);
            Debug.Log($"<color=green>Eventos de Daño recibidos: {data.Length}</color>");
            foreach(var item in data) {
                Debug.Log($" Daño: {item.damage} | Pos: ({item.x}, {item.y}, {item.z}) | Sesión: {item.session_id}");
            }
        });
    }
    [ContextMenu("Fetch Player Killed Events")]
    public void GetKilledEvents() {
        DownloadEventData("PlayerKilled", (json) => {
            PlayerKilledData[] data = JsonHelper.FromJson<PlayerKilledData>(json);
            Debug.Log($"<color=green>Eventos de Muerte recibidos: {data.Length}</color>");
            foreach(var item in data) {
                Debug.Log($" Pos: ({item.x}, {item.y}, {item.z}) | Sesión: {item.session_id}");
            }
        });
    }

    public void DownloadEventData(string eventName, Action<string> callback) {
        StartCoroutine(GetRequest(eventName, callback));
    }

    private IEnumerator GetRequest(string eventName, Action<string> callback) {
        // Construimos la URL: .../fetch_data.php?eventName=PlayerPosition
        string url = $"{serverUrl.TrimEnd('/')}/{endpointPath}?eventName={UnityWebRequest.EscapeURL(eventName)}";
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url)) {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success) {
                Debug.LogError($"Error: {webRequest.error} | URL: {url}");
            } else {
                callback?.Invoke(webRequest.downloadHandler.text);
            }
        }
    }

    // Helper para convertir arrays JSON de Unity
    public static class JsonHelper {
        public static T[] FromJson<T>(string json) {
            string newJson = "{ \"items\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.items;
        }
        [Serializable]
        private class Wrapper<T> { public T[] items; }
    }
}