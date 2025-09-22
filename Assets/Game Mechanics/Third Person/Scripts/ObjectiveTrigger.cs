using UnityEngine;
using UnityEngine.Playables;

public class ObjectiveTrigger : MonoBehaviour
{
    public AudioSource audioSource;
    [Tooltip("Optional")]
    public AudioClip audioClip;

    [Space]
    [Header("Objective Info")]
    public string objectiveTitle;
    public string objectiveDescription;
    public ObjectiveType objectiveType;
    public Transform objectiveLocation;

    [Space]
    public bool playsCutscene;
    [Tooltip("Required if Plays Cutscene is ticked")]
    public PlayableDirector cutsceneTimeline;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (audioClip)
                audioSource.PlayOneShot(audioClip);
            GetComponent<SphereCollider>().enabled = false;
            OutGameManager.Instance.UpdateObjective(objectiveTitle, objectiveDescription, 0.5f, objectiveLocation, objectiveType);
            cutsceneTimeline?.gameObject.SetActive(playsCutscene);
        }

    }
}
