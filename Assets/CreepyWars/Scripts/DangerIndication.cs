using UnityEngine;
using UnityEngine.UI;

public class DangerIndication : MonoBehaviour
{
    public Image directionIndicator;

    [HideInInspector]
    public Transform target;

    private void OnEnable()
    {
        if (target == null || directionIndicator == null) return;

        // Run this every frame so the arrow always updates
        StartCoroutine(UpdateDirection());
    }

    private System.Collections.IEnumerator UpdateDirection()
    {
        while (gameObject.activeInHierarchy && target != null)
        {
            // Get screen positions
            Vector3 screenTargetPos = Camera.main.WorldToScreenPoint(target.position);
            Vector3 screenSelfPos = Camera.main.WorldToScreenPoint(transform.position);

            // Calculate direction from self to target
            Vector2 dir = (screenTargetPos - screenSelfPos).normalized;

            // Angle in degrees
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Rotate so the RIGHT side of the arrow points at the target
            directionIndicator.rectTransform.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }
    }

    private void OnDisable()
    {
        StopCoroutine(UpdateDirection());
    }
}
