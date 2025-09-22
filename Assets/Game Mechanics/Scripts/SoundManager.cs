using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;

    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Load the prefab from Resources folder
                GameObject prefab = Resources.Load<GameObject>("SoundManager");

                if (prefab != null)
                {
                    GameObject obj = Object.Instantiate(prefab);
                    instance = obj.GetComponent<SoundManager>();
                }
                else
                {
                    Debug.LogError("SoundManager prefab not found in Resources folder!");
                }
            }
            return instance;
        }
    }

    // Other fields ----------------------------------------------------------------------------------------------

    public SoundData soundData;

    public AudioSource defaultAudioSource;
    public AudioSource bgAudioSource;
    public AudioSource loopAudioSource;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject); // kill the clone
        }
    }

    public void PlayWinSound()
    {
        for (int i = 0; i < soundData.clips.Count; i++)
        {
            if (soundData.clips[i].clipName == "win")
            {
                defaultAudioSource.clip = soundData.clips[i].clip;
                defaultAudioSource.Play();
            }
        }
    }
    public void PlayFailSound()
    {
        for (int i = 0; i < soundData.clips.Count; i++)
        {
            if (soundData.clips[i].clipName == "fail")
            {
                defaultAudioSource.clip = soundData.clips[i].clip;
                defaultAudioSource.Play();
            }
        }
    }
    public void PlayObjComplete()
    {
        for (int i = 0; i < soundData.clips.Count; i++)
        {
            if (soundData.clips[i].clipName == "objComplete")
            {
                defaultAudioSource.clip = soundData.clips[i].clip;
                defaultAudioSource.Play();
            }
        }
    }
    public void PlaySylphGameplayDialogue(SylphDialogueContext context)
    {
        if (context == SylphDialogueContext.none) return;
        else if (context == SylphDialogueContext.stealth)
        {
            int dicedValue = Mathf.Abs(Random.Range(0, soundData.sylphStealthClips.Count)) - 1;
            for (int i = 0; i < soundData.sylphStealthClips.Count; i++)
            {
                if (soundData.sylphStealthClips.IndexOf(soundData.sylphStealthClips[i]) == dicedValue)
                {
                    defaultAudioSource.clip = soundData.sylphStealthClips[i];
                    defaultAudioSource.Play();
                }
            }
        }
    }



    // Your SoundManager methods go here
}


