using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum ObjectiveType
{
    Main,
    Enemy,
    Friendly,
    Misc
}

public class ObjectiveUISystem : MonoBehaviour
{
    [Header("UI Text")]
    public TMP_Text objectiveTitle;
    public TMP_Text objectiveDescription;

    [Header("Main UI Indicator (Header)")]
    public DOTweenAnimation objectiveIndicator;
    public Image objectiveIndicatorImage;

    [Header("Floating Indicator")]
    public Camera mainCamera;
    public RectTransform canvasRect;
    public RectTransform objectiveIndicatorUI;
    public Image floatingIndicatorImage;

    [Header("Icons by Type")]
    public Sprite mainSprite;
    public Sprite enemySprite;
    public Sprite friendlySprite;
    public Sprite miscSprite;

    private Dictionary<ObjectiveType, Sprite> iconMap;

    [Header("Indicator Settings")]
    public Vector2 screenOffset = new Vector2(0, 50f);
    public float borderPadding = 40f;

    private Coroutine typingRoutine;
    private bool showIndicator;
    private Transform objectiveWorldPosition;

    void Awake()
    {
        iconMap = new Dictionary<ObjectiveType, Sprite>
        {
            { ObjectiveType.Main, mainSprite },
            { ObjectiveType.Enemy, enemySprite },
            { ObjectiveType.Friendly, friendlySprite },
            { ObjectiveType.Misc, miscSprite }
        };
    }

    void Update()
    {
        if (!showIndicator || mainCamera == null || objectiveIndicatorUI == null)
            return;

        // Get the world position of the objective in screen coordinates
        Vector3 screenPos = mainCamera.WorldToScreenPoint(objectiveWorldPosition.position);
        bool isBehind = screenPos.z < 0;
        if (isBehind) screenPos *= -1; // Flip if behind camera

        // Clamp the position to screen bounds
        if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
        {
            // On-screen: keep position
        }
        else
        {
            // Off-screen: clamp to borders
            screenPos.x = Mathf.Clamp(screenPos.x, borderPadding, Screen.width - borderPadding);
            screenPos.y = Mathf.Clamp(screenPos.y, borderPadding, Screen.height - borderPadding);
        }

        // Convert back to world position for World Space canvas
        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screenPos, mainCamera, out worldPos);

        // Apply world position + any offset
        objectiveIndicatorUI.position = worldPos + (Vector3)screenOffset;
    }


    public void ChangeObjective(string title, string description, float duration, Transform worldPosition, ObjectiveType type)
    {
        objectiveWorldPosition = worldPosition;
        showIndicator = true;

        // Change both sprites based on objective type
        if (iconMap.TryGetValue(type, out Sprite newSprite))
        {
            if (objectiveIndicatorImage != null)
                objectiveIndicatorImage.sprite = newSprite;

            if (floatingIndicatorImage != null)
                floatingIndicatorImage.sprite = newSprite;
        }

        objectiveIndicator.DORestartAllById("objectiveUpdate");
        objectiveTitle.GetComponent<DOTweenAnimation>().DORestartAllById("objectiveUpdate");
        if (objectiveIndicatorUI != null)
            objectiveIndicatorUI.GetComponent<DOTweenAnimation>().DORestartAllById("updateObjective");

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(TypeObjective(title, description, duration));
    }

    private IEnumerator TypeObjective(string title, string description, float duration)
    {
        objectiveTitle.text = "";
        objectiveDescription.text = "";

        float totalChars = title.Length + description.Length;
        float secondsPerChar = duration / Mathf.Max(totalChars, 1);

        for (int i = 0; i <= title.Length; i++)
        {
            objectiveTitle.text = title.Substring(0, i);
            yield return new WaitForSeconds(secondsPerChar);
        }

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i <= description.Length; i++)
        {
            objectiveDescription.text = description.Substring(0, i);
            yield return new WaitForSeconds(secondsPerChar);
        }

        typingRoutine = null;
    }
}
