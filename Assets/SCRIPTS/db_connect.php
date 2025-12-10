<?php
// footer.php


$servername = "localhost";
$username   = "adriarj";  
$password   = "gAxcGUE7Czfb";
$dbname     = "adriarj";

// Crear conexión
$conn = new mysqli($servername, $username, $password, $dbname);

// Verificar conexión
if ($conn->connect_error) {
    // Si falla, mata el proceso y muestra el error (útil para debug)
    die("Fallo en la conexión: " . $conn->connect_error);
}

// Opcional: Asegurar que los caracteres especiales (tildes, ñ) se guarden bien
$conn->set_charset("utf8");
?>