<?php
require 'db.php';

header('Content-Type: application/json');


$json = file_get_contents('php://input');
$data = json_decode($json, true);

if (!$data) {
    http_response_code(400);
    echo json_encode(['error' => 'Неправильний формат даних']);
    exit;
}


$player_id = isset($data['player_id']) ? (int)$data['player_id'] : null;
$kronus = isset($data['Kronus']) ? (int)$data['Kronus'] : null;
$lyrion = isset($data['Lyrion']) ? (int)$data['Lyrion'] : null;
$mystara = isset($data['Mystara']) ? (int)$data['Mystara'] : null;
$eclipsia = isset($data['Eclipsia']) ? (int)$data['Eclipsia'] : null;
$fiora = isset($data['Fiora']) ? (int)$data['Fiora'] : null;


if ($player_id === null || $player_id <= 0) {
    http_response_code(400);
    echo json_encode(['error' => 'Відсутній або некоректний player_id']);
    exit;
}


if ($kronus === null || $lyrion === null || $mystara === null || $eclipsia === null || $fiora === null) {
    http_response_code(400);
    echo json_encode(['error' => 'Відсутні дані']);
    exit;
}

if (!($kronus >= $lyrion && $lyrion >= $mystara && $mystara >= $eclipsia && $eclipsia >= $fiora)) {
    http_response_code(400);
    echo json_encode(['error' => 'Порушено порядок значень']);
    exit;
}

$total = $kronus + $lyrion + $mystara + $eclipsia + $fiora;
if ($total != 1000) {
    http_response_code(400);
    echo json_encode(['error' => 'Сума повинна бути 1000']);
    exit;
}


$score = $total;


$stmt = $conn->prepare("INSERT INTO games (player_id, kronus, lyrion, mystara, eclipsia, fiora) VALUES (?, ?, ?, ?, ?, ?)");
$stmt->bind_param("iiiiii", $player_id, $kronus, $lyrion, $mystara, $eclipsia, $fiora);

if ($stmt->execute()) {
    echo json_encode(['message' => 'Хід прийнято', 'game_id' => $stmt->insert_id]);
} else {
    http_response_code(500);
    echo json_encode(['error' => 'Помилка запису в БД']);
}

$stmt->close();
$conn->close();
?>
