<?php
require 'db.php';

$username = mysqli_real_escape_string($conn, $_POST['username']);


$checkQuery = "SELECT id FROM players WHERE username = '$username'";
$result = $conn->query($checkQuery);

if ($result && $result->num_rows > 0) {
    http_response_code(409); // 409 Conflict
    echo json_encode(['error' => 'Користувач з таким іменем вже існує']);
    exit;
}

$sql = "INSERT INTO players (username) VALUES ('$username')";
if ($conn->query($sql)) {
    echo json_encode(['player_id' => $conn->insert_id]);
} else {
    http_response_code(500);
    echo json_encode(['error' => 'Помилка при додаванні користувача']);
}
?>
