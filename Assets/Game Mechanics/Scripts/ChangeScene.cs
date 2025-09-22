using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [Tooltip("Essential if you checked 'Change On Timeline Finish' field")]
    public PlayableDirector PlayableDirector;
    public bool ChangeOnTimelineFinish;

    [Description("OR")]
    [Tooltip("Loads the scene after these seconds if you didn't check 'Change On Timeline Finish' field")]
    public float ChangeAfter = 10f;

    public string SceneName;


    void ChangeSceneNow(PlayableDirector _)
    {
        SceneManager.LoadScene(SceneName);
    }
    void Start()
    {

        if (ChangeOnTimelineFinish == true)
        {
            PlayableDirector.stopped += ChangeSceneNow;
        }
        else if (ChangeOnTimelineFinish == false)
        {
            StartCoroutine(WaitnLoadScene());
        }

        if (PlayableDirector == null)
        {

        }
    }

    IEnumerator WaitnLoadScene()
    {
        yield return new WaitForSeconds(ChangeAfter);
        SceneManager.LoadScene(SceneName);

    }

}
