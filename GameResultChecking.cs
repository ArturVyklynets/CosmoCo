using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

[System.Serializable]
public class PlayerResult
{
    public int player_id;
    public int game_id;
    public int kronus;
    public int lyrion;
    public int mystara;
    public int eclipsia;
    public int fiora;
    public int score;
}

[System.Serializable]
public class RoundResultResponse
{
    public bool round_completed;
    public int submitted_players;
    public int expected_players;
    public PlayerResult[] results;
}

public class GameResultChecking : MonoBehaviour
{
    public string resultsUrl = "https://1339-213-109-233-127.ngrok-free.app/game-server/get_results.php";
    public TextMeshProUGUI statusText;
    public float checkInterval = 5f;

    private bool isRoundCompleted = false;

    void Start()
    {
        InvokeRepeating(nameof(StartCheckingResults), 0f, checkInterval);
    }

    public void StartCheckingResults()
    {
        StartCoroutine(CheckRoundCompletion());
    }

    IEnumerator CheckRoundCompletion()
    {
        UnityWebRequest www = UnityWebRequest.Get(resultsUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Помилка отримання результатів: " + www.error);
            statusText.text = "Помилка підключення до сервера...";
            yield break;
        }

        RoundResultResponse response = JsonUtility.FromJson<RoundResultResponse>(www.downloadHandler.text);

        isRoundCompleted = response.round_completed;

        if (statusText != null)
        {
            statusText.text = $"Очікуємо гравців... ({response.submitted_players} з {response.expected_players})";
        }

        if (isRoundCompleted)
        {
            Debug.Log("Раунд завершений — переходимо до результатів!");
            SceneManager.LoadScene("Results");
        }
    }
}
