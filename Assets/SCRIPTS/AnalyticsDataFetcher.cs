using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class AnalyticsDataFetcher : MonoBehaviour
{
    public string serverUrl = "https://citmalumnes.upc.es/~adriarj/"; // Cambia a tu URL real

    // Estructura para parsear el JSON de posición (ajusta según tus columnas)
    [System.Serializable]
    public class PositionEventData
    {
        public string id;
        public string x; // El servidor suele devolver todo como string inicialmente
        public string y;
        public string z;
        public string timestamp;
    }

    // Helper para convertir la lista de JSON a una lista de objetos
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{ \"items\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.items;
        }

        [System.Serializable]
        private class Wrapper<T> { public T[] items; }
    }

    public void DownloadEventData(string eventName, System.Action<string> callback)
    {
        StartCoroutine(GetRequest(eventName, callback));
    }

    private IEnumerator GetRequest(string eventName, System.Action<string> callback)
    {
        string url = serverUrl + "fetch_data.php?eventName=" + eventName;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error descargando datos: " + webRequest.error);
            }
            else
            {
                callback?.Invoke(webRequest.downloadHandler.text);
            }
        }
    }
}