using UnityEngine;
using System.Collections;

public class OutCameraController : MonoBehaviour
{
    [Tooltip("TPSCam should have Rotation Composer component")]
    public float aimScreenPos;
    public void ShakeCamera(float intensity)
    {
        // Basic shake logic — you can replace this with Cinemachine shake if using it
        StartCoroutine(DoShake(intensity));
    }

    private IEnumerator DoShake(float intensity)
    {
        Vector3 originalPos = transform.localPosition;

        float duration = 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * intensity * 0.02f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

}
