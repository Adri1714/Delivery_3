<?php
// collector.php
include 'db_connect.php';

// Verificamos que lleguen datos básicos
if (!isset($_POST['eventName']) || !isset($_POST['data'])) {
    die("Error: Faltan parametros eventName o data.");
}

$eventName = $_POST['eventName'];
$jsonData = $_POST['data'];
$params = json_decode($jsonData, true);

if ($params === null) {
    die("Error: JSON invalido.");
}

// Limpiamos el nombre de la tabla
$safeEventName = preg_replace("/[^a-zA-Z0-9]+/", "", $eventName);
$tableName = "analytics_" . strtolower($safeEventName);

// Construcción dinámica de la Query SQL
$columns = array();
$values = array();

foreach ($params as $key => $value) {
    // 1. Nombre de la columna (Sanitizado)
    $columns[] = $conn->real_escape_string($key);

    // 2. Valor de la columna
    // AQUI ESTABA EL ERROR: Primero miramos si es array
    if (is_array($value)) {
        // Si es un Vector/Array, lo convertimos a JSON String PRIMERO
        $stringifiedValue = json_encode($value);
        // Y LUEGO lo limpiamos para SQL
        $values[] = "'" . $conn->real_escape_string($stringifiedValue) . "'";
    } 
    else {
        // Si ya es texto o número, lo limpiamos directamente
        $values[] = "'" . $conn->real_escape_string($value) . "'";
    }
}

$columnsStr = implode(", ", $columns);
$valuesStr = implode(", ", $values);

$sql = "INSERT INTO $tableName ($columnsStr) VALUES ($valuesStr)";

if ($conn->query($sql) === TRUE) {
    echo "Dato guardado correctamente";
} else {
    echo "Error SQL: " . $conn->error;
}
?>