using TMPro;
using UnityEngine;
using System.Collections;

public class ObjectiveTriggers : MonoBehaviour
{
    public AudioSource audioSource;
    public TextMeshProUGUI textMeshPro;
    public float textDisplayTime = 3f; // Adjust the display time as needed

    private bool hasPlayedAudio = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasPlayedAudio)
        {
            hasPlayedAudio = true;
            audioSource.Play();
            textMeshPro.gameObject.SetActive(true);

            // Start a coroutine to hide the text after the specified time
            StartCoroutine(HideTextAfterTime(textDisplayTime));
        }
    }

    private IEnumerator HideTextAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        textMeshPro.gameObject.SetActive(false);
    }
}
