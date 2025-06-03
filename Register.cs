using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class Register : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField surnameInput;
    public Button registerButton;
    public TMP_Text errorText;

    private string serverUrl = "https://ba92-213-109-232-49.ngrok-free.app/game-server/register.php";
    private string sessionUrl = "https://ba92-213-109-232-49.ngrok-free.app/game-server/get_session.php";

    public AudioClip clickSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        registerButton.onClick.AddListener(OnRegisterClicked);
    }

    void OnRegisterClicked()
    {
        PlayClickSound();
        StartCoroutine(SendRegisterRequest());
    }

    void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    IEnumerator SendRegisterRequest()
    {
        UnityWebRequest sessionRequest = UnityWebRequest.Get(sessionUrl);
        yield return sessionRequest.SendWebRequest();

        if (sessionRequest.result != UnityWebRequest.Result.Success)
        {
            errorText.text = "Не вдалося отримати сесію!";
            errorText.color = Color.red;
            yield break;
        }

        SessionResponse session = JsonUtility.FromJson<SessionResponse>(sessionRequest.downloadHandler.text);
        int sessionId = session.session_id;

        string fullName = nameInput.text.Trim() + " " + surnameInput.text.Trim();
        if (string.IsNullOrEmpty(fullName.Trim()))
        {
            errorText.text = "Введіть ім'я та прізвище!";
            errorText.color = Color.red;
            yield break;
        }

        errorText.text = "";

        WWWForm form = new WWWForm();
        form.AddField("username", fullName);
 

        UnityWebRequest www = UnityWebRequest.Post(serverUrl, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            errorText.text = "Помилка мережі або серверу";
            errorText.color = Color.red;
        }
        else
        {
            string responseText = www.downloadHandler.text;
            if (www.responseCode == 409)
            {
                ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(responseText);
                errorText.text = error.error;
                errorText.color = Color.red;
            }
            else
            {
                PlayerIdResponse response = JsonUtility.FromJson<PlayerIdResponse>(responseText);
                if (response != null && response.player_id > 0)
                {
                    PlayerPrefs.SetInt("player_id", response.player_id);
                    PlayerPrefs.SetInt("session_id", response.session_id);
                    PlayerPrefs.Save();
                    SceneManager.LoadScene("Waiting");
                }
                else
                {
                    errorText.text = "Не вдалося зареєструватися. Невірна відповідь сервера.";
                    errorText.color = Color.red;
                }
            }
        }
    }

    [System.Serializable]
    public class PlayerIdResponse
    {
        public int player_id;
        public int session_id;
    }

    [System.Serializable]
    public class ErrorResponse
    {
        public string error;
    }

    [System.Serializable]
    public class SessionResponse
    {
        public int session_id;
    }
}
