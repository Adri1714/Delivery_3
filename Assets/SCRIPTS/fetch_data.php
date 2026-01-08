<?php
// fetch_data.php
include 'db_connect.php';

// 1. Validar que el nombre del evento llegue por GET
if (!isset($_GET['eventName'])) {
    http_response_code(400);
    die(json_encode(["error" => "Falta el parametro eventName"]));
}

$eventName = $_GET['eventName'];
// Limpiamos el nombre para que coincida con la estructura de tablas creada en setup.php
$safeEventName = preg_replace("/[^a-zA-Z0-9]+/", "", $eventName);
$tableName = "analytics_" . strtolower($safeEventName);

// 2. Consultar datos
$sql = "SELECT * FROM $tableName ORDER BY id DESC"; // Ordenados por el más reciente
$result = $conn->query($sql);

$rows = array();
if ($result) {
    while($r = $result->fetch_assoc()) {
        $rows[] = $r;
    }
}

// 3. Devolver JSON limpio
header('Content-Type: application/json');
echo json_encode($rows);
?>