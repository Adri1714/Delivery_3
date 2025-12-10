<?php
// setup.php

// 1. Aquí estaba el error: ahora buscamos el nombre correcto
include 'db_connect.php'; 

// Recibimos el JSON desde Unity
$json = file_get_contents('php://input');
$data = json_decode($json, true);

if ($data === null) {
    die("Error: No se han recibido datos JSON validos.");
}

foreach ($data['events'] as $event) {
    // Limpiamos el nombre para evitar caracteres raros
    $safeEventName = preg_replace("/[^a-zA-Z0-9]+/", "", $event['eventName']);
    $tableName = "analytics_" . strtolower($safeEventName);
    
    // 2. Crear tabla si no existe
    $sql = "CREATE TABLE IF NOT EXISTS $tableName (
        id INT AUTO_INCREMENT PRIMARY KEY,
        timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
    )";
    
    // Verificamos si conn existe antes de usarlo
    if(isset($conn)) {
        $conn->query($sql);
    } else {
        die("Error: La variable de conexión \$conn no existe. Revisa db_connect.php");
    }

    // 3. Revisar columnas y añadirlas dinámicamente
    foreach ($event['parameters'] as $param) {
        $colName = preg_replace("/[^a-zA-Z0-9_]+/", "", $param['paramName']);
        $colType = "";
        
        // Mapear tipos de Unity a SQL (ACTUALIZADO CON TUS NUEVOS TIPOS)
        switch ($param['type']) {
            case 0: $colType = "VARCHAR(255)"; break; // String
            case 1: $colType = "INT"; break;          // Int
            case 2: $colType = "FLOAT"; break;        // Float
            case 3: $colType = "TINYINT"; break;      // Bool
            case 4: $colType = "VARCHAR(255)"; break; // Vector2
            case 5: $colType = "VARCHAR(255)"; break; // Vector3
            case 6: $colType = "DATETIME"; break;     // DateTime
            default: $colType = "TEXT"; break;
        }

        $alterSql = "ALTER TABLE $tableName ADD COLUMN $colName $colType";
        try {
            $conn->query($alterSql); 
        } catch (Exception $e) {
            // La columna ya existía, continuamos
        }
    }
}
echo "Estructura sincronizada correctamente.";
?>