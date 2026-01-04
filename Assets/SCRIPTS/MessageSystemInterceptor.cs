using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

/// <summary>
/// Intercepta los mensajes del sistema MessageSystem del juego
/// sin modificar el código original
/// </summary>
public class MessageSystemInterceptor : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool interceptDamageMessages = true;
    [SerializeField] private bool interceptDeathMessages = true;
    [SerializeField] private bool interceptCollectibleMessages = true;
    [SerializeField] private bool interceptAllMessages = false;

    private object messageSystemInstance;
    private Type messageSystemType;
    
    void Start()
    {
        InitializeInterceptor();
    }

    void Update()
    {
        // Polling del sistema de mensajes (si es necesario)
        CheckForMessages();
    }

    private void InitializeInterceptor()
    {
        Debug.Log("[MessageInterceptor] Inicializando interceptor de mensajes...");
        
        // Buscar el MessageSystem usando reflexión
        messageSystemType = FindTypeByName("MessageSystem", "Gamekit3D.Message.MessageSystem");
        
        if (messageSystemType != null)
        {
            // Intentar obtener la instancia singleton
            var instanceField = messageSystemType.GetProperty("instance", 
                BindingFlags.Public | BindingFlags.Static);
            
            if (instanceField != null)
            {
                messageSystemInstance = instanceField.GetValue(null);
                Debug.Log("[MessageInterceptor] MessageSystem encontrado!");
                
                // Intentar suscribirse a eventos
                SubscribeToMessageEvents();
            }
        }
        else
        {
            Debug.LogWarning("[MessageInterceptor] No se encontró MessageSystem. Usando método alternativo.");
        }
    }

    private void SubscribeToMessageEvents()
    {
        // Nota: Como no podemos modificar el código original,
        // usaremos polling o eventos de Unity como alternativa
        
        // Buscar tipos de mensajes comunes
        SubscribeToMessageType("Gamekit3D.Damageable+DamageMessage", OnDamageMessageReceived);
        SubscribeToMessageType("Gamekit3D.PlayerController+PlayerMessage", OnPlayerMessageReceived);
    }

    private void SubscribeToMessageType(string messageTypeName, Action<object> callback)
    {
        Type messageType = FindTypeByName(messageTypeName);
        
        if (messageType != null && messageSystemInstance != null)
        {
            try
            {
                // Intentar suscribirse usando reflexión
                var subscribeMethod = messageSystemType.GetMethod("Subscribe", 
                    BindingFlags.Public | BindingFlags.Instance);
                
                if (subscribeMethod != null)
                {
                    // Crear delegado genérico
                    var delegateType = typeof(Action<>).MakeGenericType(messageType);
                    var handler = Delegate.CreateDelegate(delegateType, this, 
                        callback.Method);
                    
                    subscribeMethod.Invoke(messageSystemInstance, new object[] { handler });
                    Debug.Log($"[MessageInterceptor] Suscrito a: {messageTypeName}");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MessageInterceptor] No se pudo suscribir a {messageTypeName}: {e.Message}");
            }
        }
    }

    private void CheckForMessages()
    {
        // Método alternativo: Buscar componentes Damageable activamente
        if (interceptDamageMessages)
        {
            CheckDamageableComponents();
        }
    }

    private void CheckDamageableComponents()
    {
        // Buscar todos los Damageable y verificar su estado
        var damageableType = FindTypeByName("Damageable", "Gamekit3D.Damageable");
        
        if (damageableType != null)
        {
            var damageables = FindObjectsOfType(damageableType);
            
            foreach (var damageable in damageables)
            {
                CheckDamageableState(damageable);
            }
        }
    }

    private Dictionary<object, float> lastHealthValues = new Dictionary<object, float>();

    private void CheckDamageableState(object damageable)
    {
        var type = damageable.GetType();
        
        // Obtener salud actual
        var healthField = type.GetField("currentHitPoints", 
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (healthField != null)
        {
            float currentHealth = (float)healthField.GetValue(damageable);
            
            // Verificar si cambió
            if (lastHealthValues.TryGetValue(damageable, out float lastHealth))
            {
                if (currentHealth < lastHealth)
                {
                    // Detectamos daño
                    OnDamageDetected(damageable, lastHealth - currentHealth);
                }
            }
            
            lastHealthValues[damageable] = currentHealth;
        }
    }

    #region Handlers de Mensajes

    private void OnDamageMessageReceived(object message)
    {
        if (!interceptDamageMessages) return;
        
        var data = ExtractDamageMessageData(message);
        
        if (data != null)
        {
            AnalyticsManager.Instance?.TrackEvent("DamageReceived", data);
        }
    }

    private void OnPlayerMessageReceived(object message)
    {
        var data = ExtractPlayerMessageData(message);
        
        if (data != null)
        {
            AnalyticsManager.Instance?.TrackEvent("PlayerMessage", data);
        }
    }

    private void OnDamageDetected(object damageable, float damageAmount)
    {
        var damageableComponent = damageable as Component;
        if (damageableComponent == null) return;
        
        var data = new Dictionary<string, object>
        {
            { "damageAmount", damageAmount },
            { "objectName", damageableComponent.gameObject.name },
            { "position", damageableComponent.transform.position },
            { "timestamp", Time.timeSinceLevelLoad }
        };
        
        // Verificar si es el jugador
        if (damageableComponent.CompareTag("Player") || 
            damageableComponent.GetComponent(FindTypeByName("PlayerController")) != null)
        {
            data["isPlayer"] = true;
            AnalyticsManager.Instance?.TrackEvent("PlayerDamaged", data);
        }
        else
        {
            data["isPlayer"] = false;
            AnalyticsManager.Instance?.TrackEvent("EnemyDamaged", data);
        }
    }

    #endregion

    #region Extracción de Datos

    private Dictionary<string, object> ExtractDamageMessageData(object message)
    {
        var type = message.GetType();
        var data = new Dictionary<string, object>();
        
        try
        {
            // Extraer campos comunes de DamageMessage
            var amountField = type.GetField("amount");
            var damagerField = type.GetField("damager");
            var damageSourceField = type.GetField("damageSource");
            
            if (amountField != null)
                data["amount"] = amountField.GetValue(message);
            
            if (damagerField != null)
            {
                var damager = damagerField.GetValue(message) as Component;
                if (damager != null)
                {
                    data["damagerName"] = damager.gameObject.name;
                    data["damagerPosition"] = damager.transform.position;
                }
            }
            
            if (damageSourceField != null)
                data["damageSource"] = damageSourceField.GetValue(message);
            
            data["timestamp"] = Time.timeSinceLevelLoad;
            
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[MessageInterceptor] Error extrayendo DamageMessage: {e.Message}");
            return null;
        }
    }

    private Dictionary<string, object> ExtractPlayerMessageData(object message)
    {
        var type = message.GetType();
        var data = new Dictionary<string, object>();
        
        try
        {
            // Extraer todos los campos públicos
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = field.GetValue(message);
                
                if (value is Vector3 v3)
                    data[field.Name] = v3;
                else if (value is float || value is int || value is bool || value is string)
                    data[field.Name] = value;
            }
            
            data["timestamp"] = Time.timeSinceLevelLoad;
            
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[MessageInterceptor] Error extrayendo PlayerMessage: {e.Message}");
            return null;
        }
    }

    #endregion

    #region Utilidades de Reflexión

    private Type FindTypeByName(params string[] possibleNames)
    {
        foreach (var name in possibleNames)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(name);
                if (type != null) return type;
                
                // Buscar sin namespace
                type = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == name || t.FullName == name);
                if (type != null) return type;
            }
        }
        
        return null;
    }

    private Component[] FindObjectsOfType(Type type)
    {
        var method = typeof(UnityEngine.Object).GetMethod("FindObjectsOfType", 
            new Type[] { });
        
        if (method != null)
        {
            var generic = method.MakeGenericMethod(type);
            return (Component[])generic.Invoke(null, null);
        }
        
        return new Component[0];
    }

    #endregion

    #region API Pública

    /// <summary>
    /// Trackea manualmente un mensaje personalizado
    /// </summary>
    public void TrackMessage(string messageType, Dictionary<string, object> data)
    {
        data["messageType"] = messageType;
        data["timestamp"] = Time.timeSinceLevelLoad;
        
        AnalyticsManager.Instance?.TrackEvent("CustomMessage", data);
    }

    #endregion
}

// Helper para usar desde otros scripts sin modificarlos
public static class MessageInterceptorHelper
{
    private static MessageSystemInterceptor instance;
    
    public static void TrackMessage(string messageType, Dictionary<string, object> data)
    {
        if (instance == null)
            instance = GameObject.FindObjectOfType<MessageSystemInterceptor>();
        
        instance?.TrackMessage(messageType, data);
    }
}