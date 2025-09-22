using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AmmoOrMagIndicator : MonoBehaviour
{
    public enum IndicatorType
    {
        magazine,
        ammo
    }

    [Tooltip("There should be two of these. One should be set to ammo and one should be Mag, or the game will spam null references")]
    public IndicatorType type;
    public Image fill;
    public Image bg;

    public static AmmoOrMagIndicator InstanceMag;
    public static AmmoOrMagIndicator InstanceAmmo;

    private Color defaultColor;

    private void Awake()
    {
        if (type == IndicatorType.ammo)
        {
            InstanceAmmo = this;
            Debug.Log($"Instance Ammo is {InstanceAmmo}");
        }
        if (type == IndicatorType.magazine)
        {
            InstanceMag = this;
            Debug.Log($"Instance Mag is {InstanceMag}");
        }
    }

    private void Start()
    {
        defaultColor = fill.color;
    }

    public void Focus()
    {
        StartCoroutine(InterpolateFloat(100f, 255f, 0.3f, v => { 
            fill.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, v);
            bg.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, v);
        }));
    }

    public void FadeOut()
    {
        StartCoroutine(InterpolateFloat(255f, 100f, 0.3f, v => {
            fill.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, v);
            bg.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, v);
        }));
    }

    IEnumerator InterpolateFloat(float startValue, float endValue, float duration, System.Action<float> onUpdate)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentValue = Mathf.Lerp(startValue, endValue, t);
            onUpdate?.Invoke(currentValue);
            yield return null;
        }
        // Ensure final value is exact
        onUpdate?.Invoke(endValue);
    }
}
