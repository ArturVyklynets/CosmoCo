<?php
require 'db.php';
header('Content-Type: application/json');

// Отримуємо кількість зареєстрованих гравців
$resultCount = $conn->query("SELECT COUNT(*) as total FROM players");
$rowCount = $resultCount->fetch_assoc();
$expectedPlayers = intval($rowCount['total']);

// Отримуємо унікальні player_id, які вже є в таблиці games
$sqlPlayers = "SELECT DISTINCT player_id FROM games";
$resultPlayers = $conn->query($sqlPlayers);

$results = [];
$submittedPlayers = 0;

if ($resultPlayers->num_rows > 0) {
    while ($player = $resultPlayers->fetch_assoc()) {
        $player_id = intval($player['player_id']);

        // Беремо останній запис цього гравця
        $sqlLastMove = "SELECT kronus, lyrion, mystara, eclipsia, fiora, score, game_id
                        FROM games
                        WHERE player_id = ?
                        ORDER BY game_id DESC
                        LIMIT 1";

        $stmt = $conn->prepare($sqlLastMove);
        $stmt->bind_param("i", $player_id);
        $stmt->execute();

        $resultMove = $stmt->get_result();
        if ($row = $resultMove->fetch_assoc()) {
            $submittedPlayers++;
            $results[] = [
                'player_id' => $player_id,
                'game_id' => intval($row['game_id']),
                'kronus' => intval($row['kronus']),
                'lyrion' => intval($row['lyrion']),
                'mystara' => intval($row['mystara']),
                'eclipsia' => intval($row['eclipsia']),
                'fiora' => intval($row['fiora']),
                'score' => 0 // тимчасово, перерахуємо далі
            ];
        }

        $stmt->close();
    }
}

$roundCompleted = $submittedPlayers >= $expectedPlayers;

if ($roundCompleted) {
    // Рахуємо бали
    $planetNames = ['kronus', 'lyrion', 'mystara', 'eclipsia', 'fiora'];
    $scores = [];

    foreach ($results as $player) {
        $scores[$player['player_id']] = 0;
    }

    foreach ($planetNames as $planet) {
        $max = max(array_column($results, $planet));

        // Хто має максимум
        $winners = [];
        foreach ($results as $player) {
            if ($player[$planet] === $max) {
                $winners[] = $player['player_id'];
            }
        }

        foreach ($results as $player) {
            $id = $player['player_id'];
            if (in_array($id, $winners)) {
                $scores[$id] += (count($winners) === 1) ? 2 : 1;
            }
        }
    }

    // Оновлюємо дані в базі та в масиві
    foreach ($results as &$player) {
        $player_id = $player['player_id'];
        $game_id = $player['game_id'];
        $score = $scores[$player_id];
        $player['score'] = $score;

        $updateStmt = $conn->prepare("UPDATE games SET score = ? WHERE game_id = ?");
        $updateStmt->bind_param("ii", $score, $game_id);
        $updateStmt->execute();
        $updateStmt->close();
    }
    unset($player);
}

// Формуємо відповідь
$response = [
    'round_completed' => $roundCompleted,
    'submitted_players' => $submittedPlayers,
    'expected_players' => $expectedPlayers,
    'results' => $results
];

echo json_encode($response);
$conn->close();
?>
