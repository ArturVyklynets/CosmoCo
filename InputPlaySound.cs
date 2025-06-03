using UnityEngine;
using TMPro;

public class InputPlaySound : MonoBehaviour
{
    public AudioSource clickSound;

    public void PlayClickSound()
    {
        if (clickSound != null)
            clickSound.Play();
    }
}
