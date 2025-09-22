using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;
using System.Collections.Generic;

public class RandomMultiAimController : MonoBehaviour
{
    [Header("References")]
    public Transform characterTransform; // Character's forward-facing transform
    public MultiAimConstraint multiAimConstraint;

    [Header("Tag Filtering")]
    public List<string> targetTags; // Tags to look for in the scene

    [Header("FOV Settings")]
    public float fieldOfViewAngle = 90f;

    [Header("Timing Settings")]
    public float minDelay = 2f;
    public float maxDelay = 5f;

    private void Start()
    {
        if (multiAimConstraint == null)
            multiAimConstraint = GetComponent<MultiAimConstraint>();

        StartCoroutine(ChangeTargetRoutine());
    }

    private IEnumerator ChangeTargetRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(waitTime);

            List<Transform> targets = GetTargetsWithTagsInFOV();

            if (targets.Count > 0)
            {
                Transform chosen = targets[Random.Range(0, targets.Count)];
                SetConstraintTarget(chosen);
            }
        }
    }

    private List<Transform> GetTargetsWithTagsInFOV()
    {
        List<Transform> visibleTargets = new List<Transform>();

        foreach (string tag in targetTags)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in taggedObjects)
            {
                if (obj == null) continue;

                Vector3 directionToTarget = (obj.transform.position - characterTransform.position).normalized;
                float angle = Vector3.Angle(characterTransform.forward, directionToTarget);

                if (angle <= fieldOfViewAngle / 2f)
                {
                    visibleTargets.Add(obj.transform);
                }
            }
        }

        return visibleTargets;
    }

    private void SetConstraintTarget(Transform newTarget)
    {
        WeightedTransformArray sources = multiAimConstraint.data.sourceObjects;
        sources.Clear();
        sources.Add(new WeightedTransform(newTarget, 1f));
        multiAimConstraint.data.sourceObjects = sources;
    }
}
