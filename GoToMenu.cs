using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMenu : MonoBehaviour
{
    public void OnPlayButtonClicked()
    {
        SessionManager.Instance.EndSession();
        SceneManager.LoadScene("Menu");
    }
}
