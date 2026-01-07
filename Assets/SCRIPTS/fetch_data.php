<?php
// fetch_data.php
include 'db_connect.php';

// Verificamos que se pida un evento
if (!isset($_GET['eventName'])) {
    die(json_encode(["error" => "Falta el parametro eventName"]));
}

$eventName = $_GET['eventName'];
$safeEventName = preg_replace("/[^a-zA-Z0-9]+/", "", $eventName);
$tableName = "analytics_" . strtolower($safeEventName);

// Consultar todos los datos de esa tabla
$sql = "SELECT * FROM $tableName";
$result = $conn->query($sql);

$rows = array();
if ($result && $result->num_rows > 0) {
    while($r = $result->fetch_assoc()) {
        $rows[] = $r;
    }
}

// Devolver los datos como JSON para que Unity los entienda
header('Content-Type: application/json');
echo json_encode($rows);
?>