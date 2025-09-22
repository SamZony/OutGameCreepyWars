using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class PanelFade : MonoBehaviour
{

    [Tooltip("It is recommended to name the panel to 'FadingPanel'")]
    public Image PanelImage;

    [Header("Configuration")]
    public float Value;
    public float Target;
    public float speed;
    public float duration;
    private bool doNow;

    public enum FadeThePanel
    {
        WhenStart,
        AfterSeconds,
        AfterTimelineFinish
    }
    public FadeThePanel fadeThePanelOn;
    public PlayableDirector Timeline;

    [Tooltip("This field is useful when 'After Seconds' is selected in 'Fade The Panel' drop down menu")]
    public float WaitForSeconds;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PanelImage == null)
        {
            PanelImage = GameObject.Find("FadingPanel").GetComponent<Image>();
        }

        switch (fadeThePanelOn)
        {
            case FadeThePanel.WhenStart:

                doNow = true;

                break;
            case FadeThePanel.AfterSeconds:
                StartCoroutine(FadeOutImageAfterSeconds());
                break;
            case FadeThePanel.AfterTimelineFinish:
                Timeline.stopped += TimelineFinished;
                break;

            default:
                Debug.Log("Invalid option");
                break;
        }
    }

    IEnumerator FadeOutImageAfterSeconds()
    {
        float elapsedTime = 0f;

        yield return new WaitForSeconds(WaitForSeconds);
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime * speed;
            elapsedTime = Mathf.Clamp(elapsedTime, 0f, duration);
            float alpha = Mathf.Lerp(Value, Target, elapsedTime);
            Color newColor = new(PanelImage.color.r, PanelImage.color.g, PanelImage.color.b, alpha);

            PanelImage.color = newColor;

            Value = alpha;

            StartCoroutine(DisablingImageAfterSeconds(duration));

            yield return null;
        }
    }

    IEnumerator DisablingImageAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        PanelImage.gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (doNow)
        {
            if (Value == 0f)
            {
                float elapsedTime = 0;

                elapsedTime = elapsedTime * Time.deltaTime * speed;
                float alpha = Mathf.Lerp(Value, Target, elapsedTime);
                elapsedTime = Mathf.Clamp01(elapsedTime);
                Value = elapsedTime;
                Color newColor = new(PanelImage.color.r, PanelImage.color.g, PanelImage.color.b, alpha);

                PanelImage.color = newColor;
            }
            else if (Value == 1f)
            {
                float elapsedTime = 0;


                elapsedTime += Time.smoothDeltaTime;
                float alpha = Mathf.Lerp(Value, Target, elapsedTime / duration);
                Color newColor = new(PanelImage.color.r, PanelImage.color.g, PanelImage.color.b, alpha);

                Value = elapsedTime;
                PanelImage.color = newColor;
            }

            StartCoroutine(DisablingImageAfterSeconds(duration));
        }
    }

    void TimelineFinished(PlayableDirector _)
    {
        StartCoroutine(TimelineFinishedCoroutine());
    }

    IEnumerator TimelineFinishedCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime * speed;
            elapsedTime = Mathf.Clamp(elapsedTime, 0f, duration);
            float alpha = Mathf.Lerp(Value, Target, elapsedTime);
            Color newColor = new(PanelImage.color.r, PanelImage.color.g, PanelImage.color.b, alpha);

            PanelImage.color = newColor;

            Value = alpha;

            yield return null;
        }
    }
}
