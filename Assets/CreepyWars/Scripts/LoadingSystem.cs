using System.Collections;
using UnityEngine;

public class LoadingSystem : MonoBehaviour
{
    public bool loadSceneOnStart;
    public string sceneName;
    public float delay;
    private void Start()
    {
        if (loadSceneOnStart)
        {
            StartCoroutine(ChangeSceneDelay(sceneName, delay));
        }
    }

    IEnumerator ChangeSceneDelay(string sceneName, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        MainUICanvas.Instance.ChangeSceneAsync(sceneName);
    }
}
