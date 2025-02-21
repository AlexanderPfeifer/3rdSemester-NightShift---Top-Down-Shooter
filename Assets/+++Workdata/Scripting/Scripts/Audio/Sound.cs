using UnityEngine.Audio;
using UnityEngine;

//Holds everything that I need to play a sound effect
[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;
    [SerializeField] public AudioMixerGroup audioMixer;

    [Range(0f, 1f)] public float volume;
    
    public bool loop;
    
    [HideInInspector]
    public AudioSource audioSource;
}
