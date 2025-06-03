using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayScript : MonoBehaviour
{

    public void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("Register");
    }
}
