using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class StartBTN : MonoBehaviour
{
    public List<Button> Buttons;
    public AudioSource AudioSource;
    public float waitBeforeSceneChange = 15f;
    public string sceneName;

    public float speed = 0.5f;
    public float fadeDuration = 1f;
    public float elapsedTime = 0f;





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (Buttons.Count >= 0)
        {
            foreach (Button b in Buttons)
            {
                b.onClick.AddListener(ButtonClicked);
            }
        }
        else
        {
            Debug.LogError("No button has been assigned in the list 'Buttons' of the script 'StartGame.cs'");
        }

        foreach (Button b in Buttons)
        {
            if (!b.GetComponent<CanvasGroup>())
            {
                b.AddComponent<CanvasGroup>();
            }
        }

    }

    void ButtonClicked()
    {

        StartCoroutine(ButtonFadeOut());
    }

    IEnumerator ButtonFadeOut()
    {
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime * speed;
            elapsedTime = Mathf.Clamp(elapsedTime, 0f, fadeDuration);
            float lerpValue = elapsedTime / fadeDuration;

            foreach (Button b in Buttons)
            {
                b.GetComponent<CanvasGroup>().alpha = 1f - lerpValue;
            }

            if (AudioSource != null)
            {
                AudioSource.volume = 1f - lerpValue;
            }
            else
            {
                Debug.Log("Assign an Audio Source in the StartGame script");
            }

            yield return null;
        }

        yield return new WaitForSeconds(waitBeforeSceneChange);

        PlayerPrefs.SetString("ToLoadScene", sceneName);
        SceneManager.LoadScene("LoadingScreen");
        Debug.Log("loaded scene:" + sceneName);
    }
}
