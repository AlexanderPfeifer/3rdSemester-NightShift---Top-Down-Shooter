using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.Serialization;

//Holds everything that I need to play a sound effect
[System.Serializable]
public class Sound
{
    public string name;

    [FormerlySerializedAs("clip")] public AudioResource audioResource;
    [SerializeField] public AudioMixerGroup audioMixer;

    [Range(0f, 1f)] public float volume;
    
    public bool loop;

    [HideInInspector] public float pitch = 1;

    [HideInInspector]
    public AudioSource audioSource;
}
