using UnityEngine;


//PARA CONECTARSE:
//Servidor: sftp://citmalumnes.upc.es
//usuario: adriarj
//contraseña: gAxcGUE7Czfb

public class DataExport : MonoBehaviour
{
    public static DataExport instance { get; private set; }
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
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

}