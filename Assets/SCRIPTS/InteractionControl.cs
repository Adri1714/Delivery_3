using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class InteractionControl : MonoBehaviour
{
    public static InteractionControl Instance;

    [Header("🔗 Referencias")]
    [Tooltip("Arrastra aquí tu script ServerPathVisualizer")]
    public ServerPathVisualizer visualizer;

    [Header("1. Color & Intensity - Sesión Actual")]
    [Tooltip("Gradiente de color para la sesión actual")]
    public Gradient pathGradient;
    [Tooltip("Color para eventos de muerte")]
    public Color deathColor = Color.red;
    [Tooltip("Tamaño de los puntos de la sesión actual")]
    [Range(0.1f, 2f)] public float visualIntensity = 0.3f;

    [Header("2. Sesiones Anteriores")]
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

    [Header("4. Heatmap Settings")]
    [Tooltip("Mostrar/Ocultar el heatmap")]
    public bool showHeatmap = true;
    [Tooltip("Altura del heatmap sobre el suelo")]
    [Range(0f, 5f)]
    public float heatmapHeightOffset = 0.5f;
    [Tooltip("Tamaño de los cuadrados (menor = más preciso pero más pesado)")]
    [Range(64, 2048)]
    public int heatmapResolution = 512;
    [Tooltip("Radio del blur gaussiano (afecta el tamaño de las zonas de calor)")]
    [Range(1f, 20f)]
    public float heatmapBlurRadius = 8f;
    [Tooltip("Suavizado del heatmap (reduce ruido visual, 0 = sin suavizar)")]
    [Range(0, 3)]
    public int heatmapSmoothing = 1;

    [Header("5. Captura de Datos (Durante el Juego)")]
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
}