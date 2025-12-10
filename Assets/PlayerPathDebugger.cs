using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit3D.Message;
using System.IO;

namespace Gamekit3D
{
    public class PlayerPathDebugger : MonoBehaviour, IMessageReceiver
    {
        public enum PathDisplayMode
        {
            CurrentLife,
            CurrentSession,
            AllSessions
        }

        [Header("Path Settings")]
        [SerializeField] private PathDisplayMode displayMode = PathDisplayMode.CurrentLife;
        [SerializeField] private Color currentLifeColor = Color.green;
        [SerializeField] private Color previousLivesColor = Color.yellow;
        [SerializeField] private Color previousSessionsColor = Color.red;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private float recordInterval = 0.1f;
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private bool showPathPoints = true;
        [SerializeField] private float pointSize = 0.2f;

        [Header("Persistence")]
        [SerializeField] private bool persistBetweenSessions = true;
        [SerializeField] private string saveFileName = "PlayerPathData.json";

        private List<PathSession> allSessions = new List<PathSession>();
        private PathSession currentSession;
        private PathLife currentLife;
        private float nextRecordTime;
        private PlayerController player;
        private Damageable damageable;

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);

        [System.Serializable]
        private class PathSession
        {
            public List<PathLife> lives = new List<PathLife>();
            public string sessionStartTime;

            public PathSession()
            {
                sessionStartTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        [System.Serializable]
        private class PathLife
        {
            public List<Vector3> positions = new List<Vector3>();
            public string lifeStartTime;
            public string lifeEndTime;

            public PathLife()
            {
                lifeStartTime = System.DateTime.Now.ToString("HH:mm:ss");
            }

            public void EndLife()
            {
                lifeEndTime = System.DateTime.Now.ToString("HH:mm:ss");
            }
        }

        [System.Serializable]
        private class PathDataContainer
        {
            public List<PathSession> sessions = new List<PathSession>();
        }

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            damageable = GetComponent<Damageable>();
            
            if (player == null)
            {
                Debug.LogError("[PathDebugger] No se encontró PlayerController!");
            }
            if (damageable == null)
            {
                Debug.LogError("[PathDebugger] No se encontró Damageable!");
            }

            // Cargar datos persistentes
            if (persistBetweenSessions)
            {
                LoadPathData();
            }
        }

        private void OnEnable()
        {
            if (damageable != null)
            {
                damageable.onDamageMessageReceivers.Add(this);
                Debug.Log("[PathDebugger] Registrado en onDamageMessageReceivers");
            }

            StartNewSession();
        }

        private void OnDisable()
        {
            if (damageable != null)
            {
                damageable.onDamageMessageReceivers.Remove(this);
            }

            // Guardar datos al salir del play mode
            if (persistBetweenSessions)
            {
                SavePathData();
            }
        }

        private void OnApplicationQuit()
        {
            // Guardar datos al cerrar la aplicación
            if (persistBetweenSessions)
            {
                SavePathData();
            }
        }

        private void Update()
        {
            if (player == null || player.respawning)
                return;

            // Registrar posición del jugador
            if (Time.time >= nextRecordTime && currentLife != null)
            {
                RecordPosition();
                nextRecordTime = Time.time + recordInterval;
            }
        }

        private void StartNewSession()
        {
            currentSession = new PathSession();
            allSessions.Add(currentSession);
            StartNewLife();
            Debug.Log($"[PathDebugger] Nueva sesión iniciada. Total sesiones: {allSessions.Count}");
        }

        private void StartNewLife()
        {
            currentLife = new PathLife();
            if (currentSession != null)
            {
                currentSession.lives.Add(currentLife);
            }
            Debug.Log($"[PathDebugger] Nueva vida iniciada. Total vidas en sesión: {currentSession.lives.Count}");
        }

        private void RecordPosition()
        {
            if (currentLife != null && player != null)
            {
                currentLife.positions.Add(player.transform.position);
            }
        }

        public void OnReceiveMessage(MessageType type, object sender, object data)
        {
            Debug.Log($"[PathDebugger] Mensaje recibido: {type}");
            
            if (type == MessageType.DEAD)
            {
                OnPlayerDeath();
            }
            else if (type == MessageType.RESPAWN)
            {
                OnPlayerRespawn();
            }
        }

        private void OnPlayerDeath()
        {
            if (currentLife != null)
            {
                currentLife.EndLife();
                Debug.Log($"[PathDebugger] Vida terminada. Posiciones registradas: {currentLife.positions.Count}");
            }
        }

        private void OnPlayerRespawn()
        {
            Debug.Log("[PathDebugger] Mensaje RESPAWN recibido - Iniciando nueva vida");
            StartNewLife();
        }

        public void OnRespawnFinished()
        {
            Debug.Log("[PathDebugger] RespawnFinished llamado - Iniciando nueva vida");
            StartNewLife();
        }

        // Métodos públicos para el Inspector con Context Menu
        [ContextMenu("Clear All Paths (Delete All Sessions)")]
        public void ClearAllPaths()
        {
            allSessions.Clear();
            
            // Eliminar archivo de guardado
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log("[PathDebugger] Archivo de guardado eliminado");
            }
            
            StartNewSession();
            Debug.Log("[PathDebugger] Todos los paths han sido limpiados");
        }

        [ContextMenu("Clear Current Session Only")]
        public void ClearCurrentSession()
        {
            if (allSessions.Count > 0)
            {
                allSessions.RemoveAt(allSessions.Count - 1);
            }
            StartNewSession();
            Debug.Log("[PathDebugger] Sesión actual limpiada");
        }

        [ContextMenu("Save Path Data")]
        public void SavePathDataManually()
        {
            SavePathData();
            Debug.Log($"[PathDebugger] Datos guardados manualmente en: {SaveFilePath}");
        }

        [ContextMenu("Load Path Data")]
        public void LoadPathDataManually()
        {
            LoadPathData();
            Debug.Log($"[PathDebugger] Datos cargados manualmente desde: {SaveFilePath}");
        }

        [ContextMenu("Show Save File Path")]
        public void ShowSaveFilePath()
        {
            Debug.Log($"[PathDebugger] Ruta del archivo: {SaveFilePath}");
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.RevealInFinder(Application.persistentDataPath);
            #endif
        }

        private void SavePathData()
        {
            try
            {
                PathDataContainer container = new PathDataContainer();
                container.sessions = allSessions;

                string json = JsonUtility.ToJson(container, true);
                File.WriteAllText(SaveFilePath, json);
                
                Debug.Log($"[PathDebugger] Datos guardados: {allSessions.Count} sesiones en {SaveFilePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PathDebugger] Error al guardar datos: {e.Message}");
            }
        }

        private void LoadPathData()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    PathDataContainer container = JsonUtility.FromJson<PathDataContainer>(json);
                    
                    if (container != null && container.sessions != null)
                    {
                        allSessions = container.sessions;
                        Debug.Log($"[PathDebugger] Datos cargados: {allSessions.Count} sesiones desde {SaveFilePath}");
                    }
                }
                else
                {
                    Debug.Log("[PathDebugger] No existe archivo de guardado previo");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PathDebugger] Error al cargar datos: {e.Message}");
            }
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos || allSessions == null || allSessions.Count == 0)
                return;

            switch (displayMode)
            {
                case PathDisplayMode.CurrentLife:
                    DrawCurrentLife();
                    break;
                case PathDisplayMode.CurrentSession:
                    DrawCurrentSession();
                    break;
                case PathDisplayMode.AllSessions:
                    DrawAllSessions();
                    break;
            }
        }

        private void DrawCurrentLife()
        {
            if (currentLife != null && currentLife.positions.Count > 1)
            {
                DrawPath(currentLife.positions, currentLifeColor);
            }
        }

        private void DrawCurrentSession()
        {
            if (currentSession == null)
                return;

            for (int i = 0; i < currentSession.lives.Count; i++)
            {
                PathLife life = currentSession.lives[i];
                if (life.positions.Count > 1)
                {
                    Color color = (life == currentLife) ? currentLifeColor : previousLivesColor;
                    DrawPath(life.positions, color);
                }
            }
        }

        private void DrawAllSessions()
        {
            for (int sessionIndex = 0; sessionIndex < allSessions.Count; sessionIndex++)
            {
                PathSession session = allSessions[sessionIndex];
                bool isCurrentSession = (session == currentSession);

                for (int lifeIndex = 0; lifeIndex < session.lives.Count; lifeIndex++)
                {
                    PathLife life = session.lives[lifeIndex];
                    if (life.positions.Count > 1)
                    {
                        Color color;
                        if (life == currentLife)
                        {
                            color = currentLifeColor;
                        }
                        else if (isCurrentSession)
                        {
                            color = previousLivesColor;
                        }
                        else
                        {
                            color = previousSessionsColor;
                        }
                        DrawPath(life.positions, color);
                    }
                }
            }
        }

        private void DrawPath(List<Vector3> positions, Color color)
        {
            Gizmos.color = color;

            // Dibujar líneas entre posiciones
            for (int i = 0; i < positions.Count - 1; i++)
            {
                Gizmos.DrawLine(positions[i], positions[i + 1]);
            }

            // Dibujar puntos si está habilitado
            if (showPathPoints)
            {
                foreach (Vector3 pos in positions)
                {
                    Gizmos.DrawSphere(pos, pointSize);
                }
            }

            // Indicador de inicio (esfera más grande en blanco)
            if (positions.Count > 0)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(positions[0], pointSize * 2f);
            }

            // Indicador de fin (esfera más grande en negro)
            if (positions.Count > 0)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(positions[positions.Count - 1], pointSize * 2f);
            }
        }
    }
}