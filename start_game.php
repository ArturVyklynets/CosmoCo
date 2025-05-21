<?php
require 'db.php';

$result = $conn->query("SELECT COUNT(*) as count FROM players");
$data = $result->fetch_assoc();

$count = (int)$data['count'];

if ($count >= 2) {
    echo json_encode([
        'players_count' => $count,
        'can_start' => true
    ]);
} else {
    echo json_encode([
        'players_count' => $count,
        'can_start' => false
    ]);
}
?>
