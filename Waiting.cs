using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class GameStartChecker : MonoBehaviour
{
    public TMP_Text statusText;
    public TMP_Text timerText;

    private float checkInterval = 1f;
    private bool gameStarted = false;

    private void Start()
    {
        StartCoroutine(CheckGameStart());
    }

    private IEnumerator CheckGameStart()
    {
        while (!gameStarted)
        {
            using (UnityWebRequest www = UnityWebRequest.Get("https://ba92-213-109-232-49.ngrok-free.app/game-server/start_game.php"))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string json = www.downloadHandler.text;
                    Debug.Log("Відповідь від сервера: " + json);

                    if (json.Contains("\"error\""))
                    {
                        statusText.fontSize = 36f;
                        statusText.color = Color.red;
                        statusText.text = "Сервер повернув помилку: " + json;
                        yield break;
                    }

                    GameStatus status = JsonUtility.FromJson<GameStatus>(json);

                    statusText.text = $"Кількість гравців: {status.players_count}";
                    timerText.text = $"Час до початку: {Mathf.Ceil(status.time_left)}s";

                    if (status.can_start)
                    {
                        gameStarted = true;
                        timerText.text = "";
                        statusText.color = Color.green;
                        statusText.fontSize = 36f;
                        statusText.text = "Гра починається!";
                        yield return new WaitForSeconds(1f);
                        StartGame();
                        yield break;
                    }
                    if (status.time_left <= 0f && !status.can_start)
                    {
                        statusText.color = Color.red;
                        statusText.fontSize = 36f;
                        statusText.text = "Недостатня кількість гравців. Спробуйте пізніше.";
                        SessionManager.Instance.EndSession();
                        timerText.text = "";
                        yield break;
                    }
                }
                else
                {
                    statusText.fontSize = 36f;
                    statusText.color = Color.red;
                    statusText.text = "Помилка приєднання до сервера";
                    SessionManager.Instance.EndSession();
                    yield break;
                }
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    private void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
        Debug.Log("Game is starting!");
    }

    [Serializable]
    private class GameStatus
    {
        public int players_count;
        public bool can_start;
        public float time_left;
    }
}
