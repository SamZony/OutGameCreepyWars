using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundData", menuName = "DirectiveEcho/Sound Data")]
public class SoundData : ScriptableObject
{
    public List<Clips> clips;

    public List<AudioClip> sylphStealthClips;

    [Serializable]
    public struct Clips
    {
        public string clipName;
        public AudioClip clip;
        public float volume;
        public bool loop;
    }

}

public enum SylphDialogueContext
{
    none, stealth, loudFight, hurt, idle, environmental
}