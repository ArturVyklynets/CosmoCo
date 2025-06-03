<?php
require 'db.php';

$username = mysqli_real_escape_string($conn, $_POST['username']);

$sessionQuery = "SELECT id FROM sessions WHERE status = 'waiting' ORDER BY id DESC LIMIT 1";

$sessionResult = $conn->query($sessionQuery);

if (!$sessionResult || $sessionResult->num_rows === 0) {
    http_response_code(400);
    echo json_encode(['error' => 'Немає активної сесії для гри']);
    exit;
}

$session = $sessionResult->fetch_assoc();
$sessionId = $session['id'];

$checkQuery = "SELECT id FROM players WHERE username = '$username' AND session_id = $sessionId";
$result = $conn->query($checkQuery);

if ($result && $result->num_rows > 0) {
    http_response_code(409);
    echo json_encode(['error' => 'Користувач з таким іменем вже існує в цій сесії']);
    exit;
}

$sql = "INSERT INTO players (username, session_id) VALUES ('$username', $sessionId)";
if ($conn->query($sql)) {
    echo json_encode([
        'player_id' => $conn->insert_id,
        'session_id' => $sessionId
    ]);
} else {
    http_response_code(500);
    echo json_encode(['error' => 'Помилка при додаванні користувача']);
}
?>
