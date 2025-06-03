<?php
require 'db.php';

$sessionQuery = "SELECT id FROM sessions WHERE status = 'waiting' ORDER BY id DESC LIMIT 1";
$sessionResult = $conn->query($sessionQuery);

if ($sessionResult && $sessionResult->num_rows > 0) {
    $session = $sessionResult->fetch_assoc();
    echo json_encode(['session_id' => (int)$session['id']]);
    exit;
}

$insertQuery = "INSERT INTO sessions (status, created_at) VALUES ('waiting', NOW())";

if ($conn->query($insertQuery)) {
    $newSessionId = $conn->insert_id;
    echo json_encode(['session_id' => (int)$newSessionId]);
} else {
    http_response_code(500);
    echo json_encode(['error' => 'Не вдалося створити нову сесію']);
}
?>
