# Instrucciones para el Visualizador de Paths

## Configuración Inicial

### 1. Configurar la Escena

En Unity, necesitas tener dos GameObjects:

#### GameObject 1: Analytics Visualizer
1. Crea un GameObject vacío llamado "Analytics Visualizer"
2. Añade los siguientes componentes:
   - `AnalyticsDataFetcher`
   - `ServerPathVisualizer`
3. En el componente `AnalyticsDataFetcher`:
   - Verifica que la URL del servidor esté correcta

#### GameObject 2: Interaction Controller
1. Crea otro GameObject vacío llamado "Interaction Controller"
2. Añade el componente `InteractionControl`
3. En el Inspector, arrastra el componente `ServerPathVisualizer` del GameObject anterior al campo "Visualizer"

### 2. Verificar Referencias

En el Inspector del GameObject "Analytics Visualizer":
- El componente `ServerPathVisualizer` debería tener:
  - **Data Fetcher**: Auto-asignado al AnalyticsDataFetcher del mismo GameObject
  - **Control**: Auto-asignado al InteractionControl que creaste

## Cómo Usar el Sistema

### Paso 1: Recopilar Datos del Juego

1. Asegúrate de que el GameObject con `GameAnalyticsCollector` esté en la escena
2. Juega el juego normalmente (el sistema guarda automáticamente la posición del jugador cada 1.5 segundos)
3. Los datos se envían automáticamente al servidor

### Paso 2: Cargar y Visualizar Datos

Hay dos formas de cargar los datos:

#### Opción A: Automática
- Los datos se cargan automáticamente cuando inicias el juego en el Editor

#### Opción B: Manual
1. Selecciona el GameObject "Analytics Visualizer"
2. En el componente `ServerPathVisualizer`, haz clic derecho
3. Selecciona "Fetch All Data" en el menú contextual

### Paso 3: Ver los Paths en la Scene View

1. Asegúrate de estar en la **Scene View** (no en Game View)
2. Los paths deberían aparecer como:
   - **Esferas de colores**: Representan el camino del jugador (con gradiente)
   - **Cubos rojos**: Representan muertes
   - **Esferas amarillas**: Representan daño recibido
3. Los puntos están conectados con líneas para mostrar el recorrido

### Paso 4: Personalizar la Visualización

En el GameObject "Interaction Controller", puedes ajustar:

#### Grid Settings:
- **Show Grid**: Mostrar/ocultar el grid de referencia
- **Grid Size**: Tamaño de las celdas del grid
- **Grid Color**: Color del grid
- **Grid Dimensions**: Extensión del grid en metros

#### Color & Intensity:
- **Path Gradient**: Gradiente de colores para los paths (inicio → fin)
- **Death Color**: Color de los marcadores de muerte
- **Visual Intensity**: Tamaño de los marcadores (0.1 - 2.0)

#### Filters:
- **Show Paths**: Mostrar/ocultar los paths del jugador ✓
- **Show Deaths**: Mostrar/ocultar marcadores de muerte ✓
- **Show Damage**: Mostrar/ocultar marcadores de daño ✓

#### Time Window Filtering:
- **Time Window Start**: Mostrar datos desde este % de la sesión (0 = inicio)
- **Time Window End**: Mostrar datos hasta este % de la sesión (1 = final)

## Solución de Problemas

### No veo ningún path

1. **Verifica la consola**: Deberías ver mensajes como:
   ```
   [ServerPathVisualizer] Iniciando descarga de datos...
   [ServerPathVisualizer] ParseData Path: X items recibidos
   [ServerPathVisualizer] Total puntos cargados: X
   [ServerPathVisualizer] Filtrados X puntos de X totales
   ```

2. **Si dice "JSON vacío"**: No hay datos en el servidor
   - Juega el juego primero para generar datos
   - Verifica que `GameAnalyticsCollector` esté activo

3. **Si dice "Error parseando JSON"**: Problema con el formato de datos
   - Verifica la consola para ver el error específico
   - Comprueba que el servidor esté respondiendo correctamente

4. **Si los puntos se cargan pero no los ves**:
   - Verifica que estés en **Scene View**
   - Verifica que **Show Paths** esté activado (✓)
   - Aumenta el **Visual Intensity** a 1.0 o más
   - Verifica que **Time Window** esté en 0.0 - 1.0
   - Asegúrate de que Gizmos esté activado en la Scene View (botón arriba a la derecha)

5. **Si los paths están muy lejos**:
   - Los puntos aparecen en las coordenadas mundiales del juego
   - Mueve la cámara de Scene View a donde está tu nivel
   - Presiona F con el GameObject seleccionado para enfocar

### Los paths no se actualizan al cambiar configuración

- Asegúrate de que el campo "Visualizer" en `InteractionControl` esté asignado
- Los cambios en el Inspector deberían actualizar automáticamente
- Si no funciona, usa el menú "Fetch All Data" para recargar

### Errores de compilación

Si ves errores sobre campos faltantes, verifica que las clases de datos en `AnalyticsDataFetcher` tengan estos campos:
```csharp
public float x;
public float y;
public float z;
public string timestamp;
public string session_id;
```

## Flujo de Trabajo Recomendado

1. **Recopilación**: Juega el juego → Los datos se guardan automáticamente
2. **Visualización**: En el Editor, los datos se cargan automáticamente al Start
3. **Análisis**: Ajusta filtros y colores en el Inspector para ver patrones
4. **Iteración**: Haz cambios en el nivel y repite

## Consejos

- Usa colores contrastantes en el Path Gradient para ver claramente la progresión
- Ajusta Visual Intensity según la densidad de puntos (menos intensidad si hay muchos puntos)
- Usa Time Window para analizar partes específicas de las sesiones
- El grid ayuda a identificar áreas del mapa fácilmente
