using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    //This class is for the array in audio manager which holds everything that I need to play a sound effect
    public string name;

    public AudioClip clip;
    [SerializeField] public AudioMixerGroup audioMixer;

    [Range(0f, 1f)] public float volume;
    
    public bool loop;
    
    [HideInInspector]
    public AudioSource audioSource;
}
