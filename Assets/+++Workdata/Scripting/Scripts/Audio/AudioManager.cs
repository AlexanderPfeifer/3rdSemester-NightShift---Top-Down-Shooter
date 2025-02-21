using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //A variable for every sound that is for the game
    public Sound[] sounds;

    public static AudioManager Instance;

    //I make this a singleton class and delete it if already existing, then I apply every option for the sound that is 
    //adjustable in the inspector
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

    //Here I Search for a sound in the sound array that has the according string as a name with lambda method, the I play the sound
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
    
    //Here the same as play but with stop
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
