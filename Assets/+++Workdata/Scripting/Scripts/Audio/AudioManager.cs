using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : SingletonPersistent<AudioManager>
{
    public SoundCategory[] soundCategories;
    [SerializeField] private float fadeMusicTime;
    
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
    
    public void FadeOut(string soundEffect, string soundEffectFadeInAfter)
    {
        StartCoroutine(VolumeFadeOut(soundEffect, soundEffectFadeInAfter));
    }
    
    private IEnumerator VolumeFadeOut(string soundName, string soundNameFadeIn)
    {
        Sound _s = GetSound(soundName);
        
        if (_s != null)
        {
            float _startVolume = _s.audioSource.volume;
            float _elapsed = 0f;

            while (_elapsed < fadeMusicTime)
            {
                _elapsed += Time.unscaledDeltaTime;
                _s.audioSource.volume = Mathf.Lerp(_startVolume, 0f, _elapsed / fadeMusicTime);
                yield return null;
            }

            _s.audioSource.volume = 0f;
            Pause(_s.audioSource.resource.name);

            if (soundName == "MainMenuMusic")
            {
                SceneManager.Instance.SwitchScene(GameSaveStateManager.Instance.saveGameDataManager.loadedSceneName);
            }

            if (soundNameFadeIn != "")
            {
                FadeIn(soundNameFadeIn);
            }
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
                Play(soundName);
                _s.audioSource.volume = 0;
            }
            
            float _startVolume = _s.audioSource.volume;
            float _elapsed = 0f;
        
            while (_elapsed < fadeMusicTime)
            {
                _elapsed += Time.unscaledDeltaTime;
                _s.audioSource.volume = Mathf.Lerp(_startVolume, _s.volume, _elapsed / fadeMusicTime);
                yield return null;
            }

            _s.audioSource.volume = _s.volume;
        }
    }
    
    public bool IsPlaying(string soundName)
    {
        return GetSound(soundName).audioSource.isPlaying;
    }
}
