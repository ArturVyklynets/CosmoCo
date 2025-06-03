using UnityEngine;

public class ButtonClick : MonoBehaviour
{
    public AudioSource audioSource; 

    public void PlayClickSound()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource не прив’язаний!");
        }
    }
}
