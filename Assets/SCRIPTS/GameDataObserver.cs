using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

/// <summary>
/// Sistema de observación no invasivo para extraer datos del juego
/// sin modificar el código original.
/// </summary>
public class GameDataObserver : MonoBehaviour
{
    [Header("Configuración de Observación")]
    [Tooltip("Tipos de datos que quieres observar")]
    public List<ObservationType> observationTypes = new List<ObservationType>();
    
    [Header("Intervalos de Captura")]
    [SerializeField] private float positionCaptureInterval = 1f;
    [SerializeField] private float healthCaptureInterval = 0.5f;
    [SerializeField] private float generalCaptureInterval = 2f;

    [Header("Referencias (Auto-detectadas)")]
    [SerializeField] private GameObject player;
    [SerializeField] private List<GameObject> enemies = new List<GameObject>();

    private Dictionary<string, float> captureTimers = new Dictionary<string, float>();

    [System.Obsolete]
    void Start()
    {
        InitializeObserver();
        
        // Configurar timers
        captureTimers["position"] = 0f;
        captureTimers["health"] = 0f;
        captureTimers["general"] = 0f;
    }

    void Update()
    {
        // Sistema de captura por intervalos
        foreach (var obsType in observationTypes)
        {
            switch (obsType)
            {
                case ObservationType.PlayerPosition:
                    CaptureOnInterval("position", positionCaptureInterval, CapturePlayerPosition);
                    break;
                    
                case ObservationType.PlayerHealth:
                    CaptureOnInterval("health", healthCaptureInterval, CapturePlayerHealth);
                    break;
                    
                case ObservationType.EnemyPositions:
                    CaptureOnInterval("general", generalCaptureInterval, CaptureEnemyPositions);
                    break;
                    
                case ObservationType.PlayerActions:
                    // Las acciones se capturan por eventos, no por intervalo
                    break;
            }
        }
        
        // Detectar acciones del jugador mediante Input
        DetectPlayerActions();
    }

    #region Inicialización y Auto-detección

    [System.Obsolete]
    private void InitializeObserver()
    {
        Debug.Log("[GameDataObserver] Inicializando sistema de observación...");
        
        // Auto-detectar player
        if (player == null)
        {
            player = FindPlayerByTag();
            if (player == null)
            {
                player = FindPlayerByComponent();
            }
        }
        
        // Auto-detectar enemigos
        if (enemies.Count == 0)
        {
            FindAllEnemies();
        }
        
        Debug.Log($"[GameDataObserver] Player detectado: {player?.name ?? "No encontrado"}");
        Debug.Log($"[GameDataObserver] Enemigos detectados: {enemies.Count}");
    }

    private GameObject FindPlayerByTag()
    {
        // Intenta encontrar por tags comunes
        string[] commonPlayerTags = { "Player", "player", "PlayerCharacter" };
        
        foreach (var tag in commonPlayerTags)
        {
            try
            {
                var obj = GameObject.FindGameObjectWithTag(tag);
                if (obj != null) return obj;
            }
            catch { }
        }
        return null;
    }

    private GameObject FindPlayerByComponent()
    {
        // Busca por componentes comunes de player
        var playerController = FindAnyObjectByType<MonoBehaviour>()
            ?.GetComponent(GetTypeByName("PlayerController"));
        
        if (playerController != null)
            return ((MonoBehaviour)playerController).gameObject;
        
        // Busca por CharacterController
        var characterController = FindAnyObjectByType<CharacterController>();
        if (characterController != null)
            return characterController.gameObject;
        
        return null;
    }

    [System.Obsolete]
    private void FindAllEnemies()
    {
        // Buscar por tags
        string[] enemyTags = { "Enemy", "enemy", "Enemigo" };
        
        foreach (var tag in enemyTags)
        {
            try
            {
                var found = GameObject.FindGameObjectsWithTag(tag);
                enemies.AddRange(found);
            }
            catch { }
        }
        
        // Buscar por componentes enemigos comunes
        var enemyComponents = FindObjectsOfType<MonoBehaviour>()
            .Where(mb => mb.GetType().Name.ToLower().Contains("enemy"))
            .Select(mb => mb.gameObject)
            .Distinct();
        
        foreach (var enemy in enemyComponents)
        {
            if (!enemies.Contains(enemy))
                enemies.Add(enemy);
        }
    }

    #endregion

    #region Captura de Datos

    private void CaptureOnInterval(string timerName, float interval, System.Action captureAction)
    {
        captureTimers[timerName] += Time.deltaTime;
        
        if (captureTimers[timerName] >= interval)
        {
            captureAction?.Invoke();
            captureTimers[timerName] = 0f;
        }
    }

    private void CapturePlayerPosition()
    {
        if (player == null) return;
        
        var data = new Dictionary<string, object>
        {
            { "position", player.transform.position },
            { "rotation", player.transform.rotation.eulerAngles },
            { "timestamp", Time.timeSinceLevelLoad }
        };
        
        AnalyticsManager.Instance?.TrackEvent("PlayerPosition", data);
    }

    private void CapturePlayerHealth()
    {
        if (player == null) return;
        
        // Intentar obtener la salud por reflexión
        float? health = GetHealthFromObject(player);
        
        if (health.HasValue)
        {
            var data = new Dictionary<string, object>
            {
                { "health", health.Value },
                { "timestamp", Time.timeSinceLevelLoad },
                { "position", player.transform.position }
            };
            
            AnalyticsManager.Instance?.TrackEvent("PlayerHealth", data);
        }
    }

    private void CaptureEnemyPositions()
    {
        if (enemies.Count == 0) return;
        
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null) continue;
            
            var data = new Dictionary<string, object>
            {
                { "enemyIndex", i },
                { "enemyName", enemies[i].name },
                { "position", enemies[i].transform.position },
                { "distanceToPlayer", player != null ? 
                    Vector3.Distance(player.transform.position, enemies[i].transform.position) : 0f },
                { "timestamp", Time.timeSinceLevelLoad }
            };
            
            AnalyticsManager.Instance?.TrackEvent("EnemyPosition", data);
        }
    }

    //---------------------------//
    //¡¡¡¡¡¡¡ESTE FUNCIONA!!!!!!!//
    //---------------------------//
    private void DetectPlayerActions()
    {
        if (player == null) return;
        
        // Detectar disparos/ataques
        if (Input.GetButtonDown("Fire1") || Input.GetMouseButtonDown(0))
        {
            TrackPlayerAction("Attack", "Primary");
        }
        
        if (Input.GetButtonDown("Fire2") || Input.GetMouseButtonDown(1))
        {
            TrackPlayerAction("Attack", "Secondary");
        }
        
        // Detectar saltos
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            TrackPlayerAction("Jump", "");
        }
        
        // Detectar movimiento
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            // Solo trackear cada cierto tiempo para no saturar
            if (captureTimers.ContainsKey("movement"))
            {
                captureTimers["movement"] += Time.deltaTime;
                if (captureTimers["movement"] < 1f) return;
                captureTimers["movement"] = 0f;
            }
            else
            {
                captureTimers["movement"] = 0f;
            }
            
            var data = new Dictionary<string, object>
            {
                { "action", "Movement" },
                { "position", player.transform.position },
            };
            
            AnalyticsManager.Instance?.TrackEvent("PlayerMovement", data);
        }
    }

    private void TrackPlayerAction(string actionType, string actionDetail)
    {
        var data = new Dictionary<string, object>
        {
            { "actionType", actionType },
            { "actionDetail", actionDetail },
            { "position", player.transform.position },
            { "timestamp", Time.timeSinceLevelLoad }
        };
        
        AnalyticsManager.Instance?.TrackEvent("PlayerAction", data);
    }

    #endregion

    #region Utilidades de Reflexión

    private float? GetHealthFromObject(GameObject obj)
    {
        // Nombres comunes de componentes de salud
        string[] healthComponentNames = { 
            "Damageable", "Health", "HealthSystem", "PlayerHealth", 
            "CharacterHealth", "HealthComponent" 
        };
        
        foreach (var componentName in healthComponentNames)
        {
            var component = obj.GetComponent(GetTypeByName(componentName));
            if (component != null)
            {
                var health = GetFieldOrPropertyValue<float>(component, 
                    "currentHitPoints", "health", "currentHealth", "hitPoints", "hp");
                
                if (health.HasValue)
                    return health.Value;
            }
        }
        
        return null;
    }

    private T? GetFieldOrPropertyValue<T>(object obj, params string[] possibleNames) where T : struct
    {
        var type = obj.GetType();
        
        foreach (var name in possibleNames)
        {
            // Buscar campo
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null && field.FieldType == typeof(T))
            {
                return (T)field.GetValue(obj);
            }
            
            // Buscar propiedad
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null && property.PropertyType == typeof(T))
            {
                return (T)property.GetValue(obj);
            }
        }
        
        return null;
    }

    private System.Type GetTypeByName(string typeName)
    {
        // Buscar en todos los assemblies cargados
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type != null) return type;
            
            // Buscar con namespace
            type = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
            if (type != null) return type;
        }
        
        return null;
    }

    #endregion

    #region Métodos Públicos para Observación Manual

    /// <summary>
    /// Trackea cualquier evento personalizado desde fuera
    /// </summary>
    public void TrackCustomEvent(string eventName, Dictionary<string, object> data)
    {
        AnalyticsManager.Instance?.TrackEvent(eventName, data);
    }

    /// <summary>
    /// Obtiene el valor de cualquier campo/propiedad de un objeto
    /// </summary>
    public object GetValueFromObject(GameObject obj, string componentName, string fieldName)
    {
        var component = obj.GetComponent(GetTypeByName(componentName));
        if (component == null) return null;
        
        var type = component.GetType();
        
        var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) return field.GetValue(component);
        
        var property = type.GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null) return property.GetValue(component);
        
        return null;
    }

    #endregion
}

public enum ObservationType
{
    PlayerPosition,
    PlayerHealth,
    PlayerActions,
    EnemyPositions,
    EnemyHealth,
    CollisionEvents,
    SceneTransitions
}