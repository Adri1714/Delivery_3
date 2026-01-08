using UnityEngine;
using System.Collections.Generic;
using Gamekit3D; // Acceso a las clases del juego original
using Gamekit3D.Message;

public class GameAnalyticsCollector : MonoBehaviour
{
    [Header("Configuración de Recopilación")]
    [Tooltip("Segundos entre cada captura de posición para el heatmap")]
    public float positionInterval = 1.5f; 
    
    private PlayerController player;
    private Damageable playerDamageable;
    private InventoryController playerInventory;
    private float nextPositionTime;

    // Para rastrear cambios sin eventos
    private int lastHealth;
    private HashSet<string> itemsKnown = new HashSet<string>();

    void Start()
    {
        // En Gamekit3D, el PlayerController tiene un Singleton estático
        player = PlayerController.instance;
        
        if (player != null)
        {
            playerDamageable = player.GetComponent<Damageable>();
            playerInventory = player.GetComponent<InventoryController>();
            
            // Suscribirse a eventos de Unity sin modificar el código original
            if (playerDamageable != null)
            {
                playerDamageable.OnReceiveDamage.AddListener(OnPlayerDamaged);
                playerDamageable.OnDeath.AddListener(OnPlayerDeath);
                lastHealth = playerDamageable.currentHitPoints;
            }
            Debug.Log("[Collector] Vinculado a Ellen correctamente.");
        }
    }

    void Update()
    {
        if (player == null) return;

        // 1. Rastreo de Posición (Heatmaps)
        if (Time.time >= nextPositionTime)
        {
            TrackPosition();
            nextPositionTime = Time.time + positionInterval;
        }
    }

    private void TrackPosition()
    {
        var data = new Dictionary<string, object>
        {
            { "x", player.transform.position.x },
            { "y", player.transform.position.y },
            { "z", player.transform.position.z },
            { "health", playerDamageable != null ? playerDamageable.currentHitPoints : 0 }
        };
        AnalyticsManager.Instance.TrackEvent("PlayerPosition", data);
    }

    private void OnPlayerDamaged()
    {
        var data = new Dictionary<string, object>
        {
            { "pos", player.transform.position },
            { "damage", lastHealth - playerDamageable.currentHitPoints }
        };
        AnalyticsManager.Instance.TrackEvent("PlayerDamaged", data);
        lastHealth = playerDamageable.currentHitPoints;
    }

    private void OnPlayerDeath()
    {
        AnalyticsManager.Instance.TrackEvent("PlayerDeath", new Dictionary<string, object> 
        { 
            { "pos", player.transform.position } 
        });
    }
}