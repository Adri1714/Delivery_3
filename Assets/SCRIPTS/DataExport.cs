using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Gamekit3D.Message;
using Gamekit3D;
using System.Collections.Generic;

//PARA CONECTARSE:
//Servidor: sftp://citmalumnes.upc.es
//usuario: adriarj
//contraseña: gAxcGUE7Czfb

public class DataExport : MonoBehaviour
{
    [Header("PHP URL")]
    [SerializeField] private string phpUrl = "https://citmalumnes.upc.es/~adriarj/footer.php";
    // Ejemplo de datos a enviar
    [System.Serializable]
    public class ExportData
    {
        //MessageType type;
        public string usuario;
        public int puntuacion;
        public float tiempo;
    }

    // Llama a esto para enviar datos
    public void EnviarDatos(MessageType type, Damageable.DamageMessage data)
    {
        var fields = new Dictionary<string, string>();
        switch (type)
        {
            case MessageType.DAMAGED:
                fields = new Dictionary<string, string>
                {
                    { "Amount", data.amount.ToString()},
                    { "Position", data.damageSource.ToString() }
                };
                break;

        }
        StartCoroutine(Upload(phpUrl, type.ToString(), fields));
    }

     private IEnumerator Upload(string url, string eventName, Dictionary<string, string> fields)
    {
        var form = new WWWForm();
        form.AddField("event", eventName);
        Debug.Log($"Adding event field: 'event' = '{eventName}'");

        foreach (var kvp in fields)
        {
            form.AddField(kvp.Key, kvp.Value);
            Debug.Log($"Adding field: '{kvp.Key}' = '{kvp.Value}'");
        }

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            // Accept opcional si el servidor responde JSON
            request.SetRequestHeader("Accept", "application/json");

            Debug.Log($"Uploading event: {eventName} via POST");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error al exportar: {request.responseCode} - {request.error}\n{request.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"Exportación OK ({request.responseCode}). Respuesta: {request.downloadHandler.text}");
            }
        }
    }
}