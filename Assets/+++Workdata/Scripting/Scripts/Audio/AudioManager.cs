using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class AudioManager : SingletonPersistent<AudioManager>
{
    public SoundCategory[] soundCategories;
    
    protected override void Awake()
    {
        base.Awake();
        
        foreach (var _soundCategory in soundCategories)
        {
            foreach (var _sound in _soundCategory.sounds)
            {
                _sound.audioSource = gameObject.AddComponent<AudioSource>();
                _sound.audioSource.resource = _sound.audioResource;
                _sound.audioSource.volume = _sound.volume;
                _sound.audioSource.loop = _sound.loop;
            
                _sound.audioSource.outputAudioMixerGroup = _sound.audioMixer;
            }
        }
    }

    private Sound GetSound(string soundName)
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
        }
        
        return _s;
    }

    public void ChangeSound(string soundName, AudioResource audioResource)
    {
        if (GetSound(soundName) != null)
        {
            GetSound(soundName).audioSource.resource = audioResource;
        }
        else
        {
            GetSound(soundName).audioSource.resource = null;
        }
    }
    
    public void Play(string soundName)
    {
        if (GetSound(soundName) != null)
        {
            GetSound(soundName).audioSource.Play();
        }
    }
    
    public void Stop(string soundName)
    {
        if (GetSound(soundName) != null)
        {
            GetSound(soundName).audioSource.Stop();
        }
    }
    
    public void Pause(string soundName)
    {
        if (GetSound(soundName) != null)
        {
            GetSound(soundName).audioSource.Pause();
        }
    }
    
    public void FadeOut(string soundEffect)
    {
        StartCoroutine(VolumeFadeOut(soundEffect));
    }
    
    private IEnumerator VolumeFadeOut(string soundName)
    {
        Sound _s = GetSound(soundName);
        
        if (_s != null)
        {
            while (_s.audioSource.volume > 0.01f)
            {
                _s.audioSource.volume = Mathf.Lerp(_s.audioSource.volume, 0, Time.unscaledTime);
                yield return null;
            }

            _s.audioSource.volume = 0;
            Pause(_s.audioSource.name);
        }
    }
    
    public void FadeIn(string soundEffect)
    {
        StartCoroutine(VolumeFadeIn(soundEffect));
    }
    
    private IEnumerator VolumeFadeIn(string soundName)
    {
        Sound _s = GetSound(soundName);
        
        if (_s != null)
        {
            if (!IsPlaying(soundName))
            {
                _s.volume = 0;
                Play(soundName);
            }
        
            while (_s.audioSource.volume < .9f)
            {
                _s.audioSource.volume = Mathf.Lerp(_s.audioSource.volume, 1, Time.unscaledTime);
                yield return null;
            }
            
            _s.audioSource.volume = 1;
        }
    }
    
    public bool IsPlaying(string soundName)
    {
        return GetSound(soundName).audioSource.isPlaying;
    }
}
