using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class ManageAfterTimeline : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public List<GameObject> gameObjectsToActivate;
    public List<GameObject> gameObjectsToDeactivate;
    public List<GameObject> gameObjectsToActivateAfterTime;
    public List<GameObject> gameObjectsToDeactivateAfterTime;

    private void Awake()
    {
        if (gameObjectsToActivate.Count > 0)
        {
            foreach (GameObject gameObject in gameObjectsToActivate)
            {
                gameObject.SetActive(true);
            }
        }
        if (gameObjectsToDeactivate.Count > 0)
        {
            foreach (GameObject gameObject in gameObjectsToDeactivate)
            {
                gameObject.SetActive(false);
            }
        }


        playableDirector.stopped += TimelineFinished;
    }

    void TimelineFinished(PlayableDirector _)
    {
        playableDirector.enabled = false;


        if (gameObjectsToActivateAfterTime.Count > 0)
        {
            foreach (GameObject gameObject in gameObjectsToActivateAfterTime)
            {
                gameObject.SetActive(true);
            }
        }
        if (gameObjectsToDeactivateAfterTime.Count > 0)
        {
            foreach (GameObject gameObject in gameObjectsToDeactivateAfterTime)
            {
                gameObject.SetActive(false);
            }
        }

    }
}
