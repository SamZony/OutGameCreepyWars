using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    public List<AudioClip> woodFootstepSounds;
    public List<AudioClip> rockFootstepSounds;
    public List<AudioClip> sandFootstepSounds;
    public List<AudioClip> grassFootstepSounds;
    // Add more arrays for other surface types

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        LoadAudioClips("SFX/Footsteps/Grass", grassFootstepSounds);
        LoadAudioClips("SFX/Footsteps/Rock", rockFootstepSounds);
        LoadAudioClips("SFX/Footsteps/Sand", sandFootstepSounds);
        LoadAudioClips("SFX/Footsteps/Wood", woodFootstepSounds);
    }

    void LoadAudioClips(string folderName, List<AudioClip> audioClipList)
    {
        string targetFolderPath = Path.Combine(Application.dataPath, "Resources", folderName);

        DirectoryInfo directoryInfo = new DirectoryInfo(targetFolderPath);
        FileInfo[] fileInfos = directoryInfo.GetFiles("*.wav"); // Adjust the wildcard for your audio file format

        foreach (FileInfo fileInfo in fileInfos)
        {
            AudioClip audioClip = Resources.Load<AudioClip>(fileInfo.Name);
            if (audioClip != null)
            {
                audioClipList.Add(audioClip);
            }
            else
            {
                Debug.LogWarning("Failed to load audio clip: " + fileInfo.FullName);
            }
        }
    }

    public void PlayFootstepSound(string surfaceType)
    {
        List<AudioClip> footstepSounds;

        switch (surfaceType)
        {
            case "Wood":
                footstepSounds = woodFootstepSounds;
                break;
            case "Rock":
                footstepSounds = rockFootstepSounds;
                break;
            case "Sand":
                footstepSounds = sandFootstepSounds;
                break;
            case "Grass":
                footstepSounds = grassFootstepSounds;
                break;
            // Add cases for other surface types

            default:
                footstepSounds = null; // Handle unknown surfaces
                break;
        }

        if (footstepSounds != null && footstepSounds.Count > 0)
        {
            AudioClip selectedSound = footstepSounds[Random.Range(0, footstepSounds.Count)];
            audioSource.PlayOneShot(selectedSound);
        }
    }
}

