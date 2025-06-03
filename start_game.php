<?php 
require 'db.php';
header('Content-Type: application/json');

date_default_timezone_set('Europe/Kyiv');

$WAIT_DURATION_SECONDS = 10;

$sessionResult = $conn->query("SELECT id, created_at FROM sessions WHERE is_active = 1 ORDER BY id DESC LIMIT 1");

if (!$sessionResult || $sessionResult->num_rows === 0) {
    http_response_code(400);
    echo json_encode(['error' => 'Немає активної сесії для гри']);
    exit;
}

$session = $sessionResult->fetch_assoc();
$sessionId = (int)$session['id'];
$createdAt = strtotime($session['created_at']);
$currentTime = time();

$timePassed = $currentTime - $createdAt;
$timeLeft = max(0, $WAIT_DURATION_SECONDS - $timePassed);

$stmt = $conn->prepare("SELECT COUNT(*) as count FROM players WHERE session_id = ?");
$stmt->bind_param("i", $sessionId);
$stmt->execute();
$result = $stmt->get_result();

if (!$result) {
    http_response_code(500);
    echo json_encode(['error' => 'Помилка бази даних']);
    exit;
}

$data = $result->fetch_assoc();
$count = (int)$data['count'];

if ($timeLeft === 0 && $count >= 2) {
    $conn->query("UPDATE sessions SET status = 'playing' WHERE id = $sessionId");
} elseif ($timeLeft === 0 && $count < 2) {
    $conn->query("UPDATE sessions SET status = 'canceled' WHERE id = $sessionId");
}

$response = [
    'players_count' => $count,
    'can_start' => $timeLeft === 0 && $count >= 2,
    'time_left' => $timeLeft
];

echo json_encode($response);
exit;
?>