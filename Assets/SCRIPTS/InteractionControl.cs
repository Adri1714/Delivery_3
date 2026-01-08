using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class InteractionControl : MonoBehaviour
{
    public static InteractionControl Instance;

    [Header("🔗 Referencias")]
    [Tooltip("Arrastra aquí tu script ServerPathVisualizer")]
    public ServerPathVisualizer visualizer;

    [Header("1. Grid Adjustments")]
    [SerializeField] private bool showGrid = true;
    [Range(1f, 10f)] public float gridSize = 5.0f;
    public Color gridColor = new Color(1, 1, 1, 0.2f);
    public int gridDimensions = 50; // Tamaño del grid en metros (50x50)

    [Header("2. Color & Intensity - Sesión Actual")]
    [Tooltip("Gradiente de color para la sesión actual")]
    public Gradient pathGradient;
    [Tooltip("Color para eventos de muerte")]
    public Color deathColor = Color.red;
    [Tooltip("Tamaño de los puntos de la sesión actual")]
    [Range(0.1f, 2f)] public float visualIntensity = 0.3f;

    [Header("2b. Sesiones Anteriores")]
    [Tooltip("Mostrar paths de sesiones anteriores")]
    public bool showOldSessions = true;
    [Tooltip("Gradiente de color para sesiones anteriores (más tenue)")]
    public Gradient oldPathGradient;
    [Tooltip("Tamaño de los puntos de sesiones anteriores")]
    [Range(0.1f, 2f)] public float oldVisualIntensity = 0.2f;
    [Tooltip("Transparencia de las sesiones anteriores (0 = invisible, 1 = opaco)")]
    [Range(0f, 1f)] public float oldSessionsAlpha = 0.4f;

    [Header("3. Filters")]
    public bool showPaths = true;
    public bool showDeaths = true;
    public bool showDamage = false;

    [Header("4. Captura de Datos (Durante el Juego)")]
    [Tooltip("Segundos entre cada captura de posición. Valores menores = más puntos (más precisión pero más datos)")]
    [Range(0.1f, 5f)]
    public float positionInterval = 1.5f;

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnValidate()
    {
        if (visualizer != null)
        {
            visualizer.OnSettingsChanged();
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGrid) return;

        Gizmos.color = gridColor;
        Vector3 startPos = transform.position;
        startPos.y = 0.1f; // Un poco elevado para que no se solape con el suelo

        for (float x = -gridDimensions; x <= gridDimensions; x += gridSize)
        {
            Gizmos.DrawLine(new Vector3(x, startPos.y, -gridDimensions), new Vector3(x, startPos.y, gridDimensions));
        }
        for (float z = -gridDimensions; z <= gridDimensions; z += gridSize)
        {
            Gizmos.DrawLine(new Vector3(-gridDimensions, startPos.y, z), new Vector3(gridDimensions, startPos.y, z));
        }
    }
}