<?php
require 'db.php';

header('Content-Type: application/json');

$sessionResult = $conn->query("SELECT id FROM sessions WHERE status = 'playing' ORDER BY id DESC LIMIT 1");

if (!$sessionResult || $sessionResult->num_rows === 0) {
  http_response_code(400);
  echo json_encode(['error' => 'Немає активної сесії для завершення']);
  exit;
}

$session = $sessionResult->fetch_assoc();
$sessionId = (int)$session['id'];

$stmt = $conn->prepare("UPDATE sessions SET status = 'finished' WHERE id = ?");

$stmt->bind_param("i", $sessionId);

if ($stmt->execute()) {
    echo json_encode(['message' => 'Сесію успішно завершено', 'session_id' => $sessionId]);
} else {
    http_response_code(500);
    echo json_encode(['error' => 'Не вдалося завершити сесію']);
}
