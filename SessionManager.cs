using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class SessionManager : MonoBehaviour
{
    private static SessionManager _instance;

    [Header("API URL")]
    public string endSessionUrl = "https://ba92-213-109-232-49.ngrok-free.app/game-server/end_session.php";

    public static SessionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("SessionManager");
                _instance = obj.AddComponent<SessionManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    public void EndSession()
    {
        Instance.StartCoroutine(Instance.SendEndSessionRequest());
    }

    private IEnumerator SendEndSessionRequest()
    {
        UnityWebRequest www = UnityWebRequest.Get(endSessionUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Помилка при завершенні сесії: " + www.error);
        }
        else
        {
            Debug.Log("Сесію завершено: " + www.downloadHandler.text);
        }
    }
}
