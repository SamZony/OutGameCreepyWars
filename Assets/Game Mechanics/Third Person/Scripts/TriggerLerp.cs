using UnityEngine;
using System.Collections;

public class TriggerLerp : MonoBehaviour
{
    public Animator playerAnimator;
    public Transform targetLocation;
    public float lerpSpeed = 10f;
    private Collider other;
    public string pressAlarmTrigger;

    public Objective nextObjective;

    void OnTriggerEnter(Collider otherr)
    {
        if (otherr.CompareTag("Player"))
        {
            OutGameManager.Instance.currentP.TryGetComponent<Animator>(out Animator animator);
            playerAnimator = animator;
            other = otherr;
            StartCoroutine(LerpToTarget());
        }
    }

    IEnumerator LerpToTarget()
    {
        playerAnimator.SetLayerWeight(playerAnimator.GetLayerIndex("Activities"), 1);
        playerAnimator.SetTrigger(pressAlarmTrigger);

        float timeElapsed = 0f;
        Vector3 startingPosition = other.transform.position;

        while (timeElapsed < 0.5f)
        {
            float t = timeElapsed / 0.5f;
            other.GetComponent<Rigidbody>().position = Vector3.Lerp(startingPosition, targetLocation.position, t * lerpSpeed);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the playerActionMap is exactly at the target location
        other.GetComponent<Rigidbody>().position = targetLocation.position;
        Invoke(nameof(DisableActivity), 1);

        SoundManager.Instance.PlayObjComplete();
        OutGameManager.Instance.UpdateObjective(nextObjective.title, nextObjective.description, 1f, nextObjective.location, nextObjective.type);
        OutGameManager.Instance.currentP.GetComponent<AudioSource>().clip = nextObjective.briefAudio;
        OutGameManager.Instance.currentP.GetComponent<AudioSource>().Play();
    }

    public void DisableActivity()
    {
        playerAnimator.SetLayerWeight(playerAnimator.GetLayerIndex("Activities"), 0);
        gameObject.SetActive(false);
    } 
}