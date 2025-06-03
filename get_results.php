<?php
require 'db.php';
header('Content-Type: application/json');

$session_id = isset($_GET['session_id']) ? $_GET['session_id'] : null;
if (!$session_id) {
    echo json_encode(['error' => 'No session_id provided']);
    exit;
}

// Кількість очікуваних гравців у сесії
$sqlExpected = "SELECT COUNT(DISTINCT id) AS expected_players FROM players WHERE session_id = ?";
$stmtExpected = $conn->prepare($sqlExpected);
$stmtExpected->bind_param("s", $session_id);
$stmtExpected->execute();
$resultExpected = $stmtExpected->get_result();
$row = $resultExpected->fetch_assoc();

if (!$row) {
    echo json_encode(['error' => 'Session not found or no players in session']);
    exit;
}

$expectedPlayers = intval($row['expected_players']);
$stmtExpected->close();

// Отримуємо унікальні player_id з таблиці games
$sqlPlayers = "SELECT DISTINCT player_id FROM games WHERE session_id = ?";
$stmtPlayers = $conn->prepare($sqlPlayers);
$stmtPlayers->bind_param("s", $session_id);
$stmtPlayers->execute();
$resultPlayers = $stmtPlayers->get_result();

$results = [];
$submittedPlayers = 0;

if ($resultPlayers->num_rows > 0) {
    while ($player = $resultPlayers->fetch_assoc()) {
        $player_id = intval($player['player_id']);

        // Отримуємо username з таблиці players за player_id і session_id
        $sqlUsername = "SELECT username FROM players WHERE id = ? AND session_id = ? LIMIT 1";
        $stmtUsername = $conn->prepare($sqlUsername);
        $stmtUsername->bind_param("is", $player_id, $session_id);
        $stmtUsername->execute();
        $resultUsername = $stmtUsername->get_result();
        $usernameRow = $resultUsername->fetch_assoc();
        $username = $usernameRow ? $usernameRow['username'] : "Unknown";
        $stmtUsername->close();

        // Отримуємо останній запис гри цього гравця у цій сесії
        $sqlLastMove = "SELECT kronus, lyrion, mystara, eclipsia, fiora, score, game_id
                        FROM games
                        WHERE player_id = ? AND session_id = ?
                        ORDER BY game_id DESC
                        LIMIT 1";

        $stmt = $conn->prepare($sqlLastMove);
        $stmt->bind_param("is", $player_id, $session_id);
        $stmt->execute();
        $resultMove = $stmt->get_result();

        if ($row = $resultMove->fetch_assoc()) {
            $submittedPlayers++;
            $results[] = [
                'player_id' => $player_id,
                'username' => $username,
                'game_id' => intval($row['game_id']),
                'kronus' => intval($row['kronus']),
                'lyrion' => intval($row['lyrion']),
                'mystara' => intval($row['mystara']),
                'eclipsia' => intval($row['eclipsia']),
                'fiora' => intval($row['fiora']),
                'score' => 0
            ];
        }

        $stmt->close();
    }
}
$stmtPlayers->close();

$roundCompleted = $submittedPlayers >= $expectedPlayers;

if ($roundCompleted) {
    $planetNames = ['kronus', 'lyrion', 'mystara', 'eclipsia', 'fiora'];
    $scores = [];

    foreach ($results as $player) {
        $scores[$player['player_id']] = 0;
    }

    foreach ($planetNames as $planet) {
        for ($i = 0; $i < count($results); $i++) {
            for ($j = $i + 1; $j < count($results); $j++) {
                $playerA = $results[$i];
                $playerB = $results[$j];

                $valA = $playerA[$planet];
                $valB = $playerB[$planet];

                if ($valA > $valB) {
                    $scores[$playerA['player_id']] += 2;
                } elseif ($valA < $valB) {
                    $scores[$playerB['player_id']] += 2;
                } else {
                    $scores[$playerA['player_id']] += 1;
                    $scores[$playerB['player_id']] += 1;
                }
            }
        }
    }

    foreach ($results as &$player) {
        $player_id = $player['player_id'];
        $game_id = $player['game_id'];
        $score = $scores[$player_id];
        $player['score'] = $score;

        $updateStmt = $conn->prepare("UPDATE games SET score = ? WHERE game_id = ? AND session_id = ?");
        $updateStmt->bind_param("iis", $score, $game_id, $session_id);
        $updateStmt->execute();
        $updateStmt->close();
    }
    unset($player);
}

$response = [
    'round_completed' => $roundCompleted,
    'submitted_players' => $submittedPlayers,
    'expected_players' => $expectedPlayers,
    'results' => $results
];

echo json_encode($response);
$conn->close();
