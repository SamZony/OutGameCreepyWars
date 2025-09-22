using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

public class Play_VIdeo : MonoBehaviour
{
    public PlayableDirector Timeline;
    public VideoPlayer VideoPlayer;
    public float AfterSeconds = 50f;

    void Start()
    {
        if (Timeline != null)
        {
            Timeline.played += StartPlayingVideo;

        }
        else
        {
            Debug.LogError("Assign a Timeline");
        }
    }


    public void StartPlayingVideo(PlayableDirector _)
    {
        StartCoroutine(StartAfterSeconds());
    }

    IEnumerator StartAfterSeconds()
    {
        yield return new WaitForSeconds(AfterSeconds);

        VideoPlayer.Play();

    }
}
