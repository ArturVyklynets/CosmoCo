using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Results : MonoBehaviour
{
    private string resultsUrl = "https://ba92-213-109-232-49.ngrok-free.app/game-server/get_results.php";

    public Transform resultsContainer;
    public GameObject playerResultPrefab;

    public string textComponentName = "Text";


    void Start()
    {
        int sessionId = PlayerPrefs.GetInt("session_id", -1);
        if (sessionId == -1)
        {
            Debug.LogError("Помилка: не знайдено session_id.");
            return;
        }
        StartCoroutine(FetchResults(sessionId));
        SessionManager.Instance.EndSession();
    }

    IEnumerator FetchResults(int sessionId)
    {
        string url = $"{resultsUrl}?session_id={sessionId}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Помилка при отриманні результатів: " + www.error);

            yield break;
        }

        RoundResultResponse response = JsonUtility.FromJson<RoundResultResponse>(www.downloadHandler.text);

        if (response.results == null || response.results.Length == 0)
        {
            Debug.Log("Результати не знайдені.");
            yield break;
        }

        List<PlayerResult> sortedResults = new List<PlayerResult>(response.results);
        sortedResults.Sort((a, b) => b.score.CompareTo(a.score));
        int maxScore = sortedResults[0].score;

        for (int i = 0; i < sortedResults.Count; i++)
        {
            PlayerResult player = sortedResults[i];
            bool isWinner = player.score == maxScore;

            GameObject item = Instantiate(playerResultPrefab, resultsContainer);

            TextMeshProUGUI textComp = item.transform.Find(textComponentName)?.GetComponent<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = $"{i + 1}. Гравець {player.username}: {player.score} балів";

                if (isWinner)
                {
                    textComp.fontSize += 15;
                    textComp.color = Color.yellow;
                }
            }
        }
    }
    [System.Serializable]
    public class RoundResultResponse
    {
        public bool round_completed;
        public int submitted_players;
        public int expected_players;
        public PlayerResult[] results;
    }

    [System.Serializable]
    public class PlayerResult
    {
        public int player_id;
        public string username;
        public int game_id;
        public int kronus;
        public int lyrion;
        public int mystara;
        public int eclipsia;
        public int fiora;
        public int score;
    }
}
