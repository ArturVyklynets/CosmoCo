using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class DroneInputValidator : MonoBehaviour
{
    public TMP_InputField inputKronus, inputLyrion, inputMystara, inputEclipsia, inputFiora;
    public TMP_Text errorText;

    public AudioClip clickSound;
    public AudioClip successSound;
    public AudioClip failSound;
    public AudioSource audioSource;

    private string submitMoveUrl = "https://1339-213-109-233-127.ngrok-free.app/game-server/submit_move.php";

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource компонент не знайдено на цьому об'єкті!");
        }
    }

    public void OnClickSound()
    {
        audioSource.PlayOneShot(clickSound);
    }

    public void ValidateInputs()
    {
        int kronus = 0, lyrion = 0, mystara = 0, eclipsia = 0, fiora = 0;
        errorText.color = Color.red;
        bool parsed =
            int.TryParse(inputKronus.text, out kronus) &&
            int.TryParse(inputLyrion.text, out lyrion) &&
            int.TryParse(inputMystara.text, out mystara) &&
            int.TryParse(inputEclipsia.text, out eclipsia) &&
            int.TryParse(inputFiora.text, out fiora);

        if (!parsed)
        {
            errorText.text = "Введіть тільки числа!";
            audioSource.PlayOneShot(failSound);
            return;
        }

        if (kronus < 0 || lyrion < 0 || mystara < 0 || eclipsia < 0 || fiora < 0 ||
            kronus > 1000 || lyrion > 1000 || mystara > 1000 || eclipsia > 1000 || fiora > 1000)
        {
            errorText.text = "Всі значення мають бути від 0 до 1000!";
            audioSource.PlayOneShot(failSound);
            return;
        }

        if (!(kronus >= lyrion && lyrion >= mystara && mystara >= eclipsia && eclipsia >= fiora))
        {
            errorText.text = "Порушено порядок: Kronus ≥ Lyrion ≥ Mystara ≥ Eclipsia ≥ Fiora";
            audioSource.PlayOneShot(failSound);
            return;
        }

        int total = kronus + lyrion + mystara + eclipsia + fiora;
        if (total != 1000)
        {
            errorText.text = $"Сума повинна дорівнювати 1000 (зараз: {total})";
            audioSource.PlayOneShot(failSound);
            return;
        }

        audioSource.PlayOneShot(successSound);

        StartCoroutine(SendMoveToServer(kronus, lyrion, mystara, eclipsia, fiora));
    }

    IEnumerator SendMoveToServer(int kronus, int lyrion, int mystara, int eclipsia, int fiora)
    {
        int playerId = PlayerPrefs.GetInt("player_id", -1);
        if (playerId == -1)
        {
            Debug.LogError("Player ID не знайдено. Спочатку зареєструйтесь.");
            errorText.color = Color.red;
            errorText.text = "Помилка: гравець не зареєстрований.";
            audioSource.PlayOneShot(failSound);
            yield break;
        }

        var moveData = new MoveData()
        {
            player_id = playerId,
            Kronus = kronus,
            Lyrion = lyrion,
            Mystara = mystara,
            Eclipsia = eclipsia,
            Fiora = fiora
        };

        var jsonData = JsonUtility.ToJson(moveData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest www = new UnityWebRequest(submitMoveUrl, "POST");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Move submitted successfully: " + www.downloadHandler.text);
            errorText.color = Color.green;
            errorText.text = "Успішно! Дані коректні і відправлені на сервер.";
            audioSource.PlayOneShot(successSound);
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("WaitingForAll");
        }
        else
        {
            Debug.LogError("Error submitting move: " + www.error);
            errorText.color = Color.red;
            errorText.text = "Помилка відправки ходу на сервер.";
            audioSource.PlayOneShot(failSound);
        }
    }

    [System.Serializable]
    public class MoveData
    {
        public int player_id;
        public int Kronus;
        public int Lyrion;
        public int Mystara;
        public int Eclipsia;
        public int Fiora;
    }
}
