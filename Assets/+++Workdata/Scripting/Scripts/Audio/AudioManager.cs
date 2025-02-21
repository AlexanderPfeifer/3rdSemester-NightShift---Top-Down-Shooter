using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        foreach (var sound in sounds)
        {
            sound.audioSource = gameObject.AddComponent<AudioSource>();
            sound.audioSource.clip = sound.clip;

            sound.audioSource.volume = sound.volume;

            sound.audioSource.loop = sound.loop;
            
            sound.audioSource.outputAudioMixerGroup = sound.audioMixer;
        }
    }

    public void Play(string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound:" + soundName + "not found!");
            return;
        }
        s.audioSource.Play();
    }
    
    public void Stop(string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound:" + soundName + "not found!");
            return;
        }
        s.audioSource.Stop();
    }
}
