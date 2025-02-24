using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public SoundCategory[] soundCategories;

    public static AudioManager Instance;
    
    private string lastRandomPlayedSound = "";

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
        
        foreach (var _soundCategory in soundCategories)
        {
            foreach (var _sound in _soundCategory.sounds)
            {
                _sound.audioSource = gameObject.AddComponent<AudioSource>();
                _sound.audioSource.clip = _sound.clip;

                _sound.audioSource.volume = _sound.volume;

                _sound.audioSource.loop = _sound.loop;
            
                _sound.audioSource.outputAudioMixerGroup = _sound.audioMixer;
            }
        }
    }

    public void Play(string soundName)
    {
        Sound _s = null;
        
        foreach (var _soundCategory in soundCategories)
        {
            if(_s != null)
                break;
            
            _s = Array.Find(_soundCategory.sounds, sound => sound.name == soundName);
        }
        
        if (_s == null)
        {
            Debug.LogError("Sound: " + soundName + " not found!");
            return;
        }
        _s.audioSource.Play();
    }
    
    public void Stop(string soundName)
    {
        Sound _s = null;

        foreach (var _soundCategory in soundCategories)
        {
            if(_s != null)
                break;
            
            _s = Array.Find(_soundCategory.sounds, sound => sound.name == soundName);
        }
        
        if (_s == null)
        {
            Debug.LogError("Sound: " + soundName + " not found!");
            return;
        }
        _s.audioSource.Stop();
    }

    public void PlayRandomSoundFromListArray(string[] soundNameFromArray)
    {
        if (soundNameFromArray.Length == 0)
        {
            Debug.LogError("Sound array is empty!");
            return;
        }

        Sound _s = null;
        string _soundName;

        //I am keep asking for a new sound because it is easier to implement and performance is not a limiting factor here
        do
        {
            _soundName = soundNameFromArray[UnityEngine.Random.Range(0, soundNameFromArray.Length)];
        } 
        while (_soundName == lastRandomPlayedSound && soundNameFromArray.Length > 1); 

        lastRandomPlayedSound = _soundName; 

        foreach (var _soundCategory in soundCategories)
        {
            if(_s != null)
                break;
            
            _s = Array.Find(_soundCategory.sounds, sound => sound.name == _soundName);
        }
        
        if (_s == null)
        {
            Debug.LogError("Sound: " + _soundName + " not found!");
            return;
        }
        
        _s.audioSource.Play();
    }
}
