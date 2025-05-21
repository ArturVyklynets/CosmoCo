using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class GameStartChecker : MonoBehaviour
{
    public TMP_Text statusText;
    public TMP_Text timerText;

    private float waitTime = 10f;
    private float checkInterval = 5f;

    private int latestPlayerCount = 0;
    private string latestMessage = "";

    private void Start()
    {
        StartCoroutine(CheckGameStartAfterDelay());
    }

    private IEnumerator CheckGameStartAfterDelay()
    {
        float elapsed = 0f;

        while (elapsed < waitTime)
        {
            using (UnityWebRequest www = UnityWebRequest.Get("https://1339-213-109-233-127.ngrok-free.app/game-server/start_game.php"))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string json = www.downloadHandler.text;
                    GameStatus status = JsonUtility.FromJson<GameStatus>(json);

                    latestPlayerCount = status.players_count;
                    latestMessage = status.message;

                    statusText.text = $"Кількість гравців: {latestPlayerCount}\n{latestMessage}";
                }
                else
                {
                    statusText.text = "";
                    timerText.text = "";
                    statusText.fontSize = 36f;
                    statusText.color = Color.red;
                    statusText.text = "Помилка приєднання до сервера";
                }
            }

            timerText.text = $"Час до початку: {Mathf.Ceil(waitTime - elapsed)}s";

            yield return new WaitForSeconds(checkInterval);
            elapsed += checkInterval;
        }

        timerText.text = "";

        if (latestPlayerCount >= 2)
        {
            statusText.text = "";
            timerText.text = "";
            statusText.color = Color.green;
            statusText.fontSize = 36f;
            statusText.text += "Гра починається!";
            yield return new WaitForSeconds(1f);
            StartGame();
        }
        else
        {
            statusText.text = "";
            timerText.text = "";
            statusText.fontSize = 36f;
            statusText.color = Color.red;
            statusText.text = "Недостатня кількість гравців. Спробуйте пізніше.";
        }
    }

    private void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
        Debug.Log("Game is starting!");
    }

    [System.Serializable]
    private class GameStatus
    {
        public int players_count;
        public string message;
        public bool can_start;
    }
}
