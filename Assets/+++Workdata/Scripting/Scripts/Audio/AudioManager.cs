using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : SingletonPersistent<AudioManager>
{
    public SoundCategory[] soundCategories;
    [SerializeField] private float fadeMusicTime;

    [Header("AudioMixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("AudioMaster")]
    private int currentVolumePointMaster;
    private const string MasterVolumePlayerPrefs = "masterVolume";
    private float masterVolume = .5f;
    private int sameDirectionMinusMaster = 1;
    private int sameDirectionPlusMaster;

    [Header("AudioMusic")]
    private int currentVolumePointMusic;
    private const string MusicVolumePlayerPrefs = "musicVolume";
    private float musicVolume = .5f;
    private int sameDirectionMinusMusic = 1;
    private int sameDirectionPlusMusic;

    [Header("AudioSFX")]
    private int currentVolumePointSfx;
    private const string SfxVolumePlayerPrefs = "sfxVolume";
    private float sfxVolume = .5f;
    private int sameDirectionMinusSfx = 1;
    private int sameDirectionPlusSfx;

    [Header("AudioCounterVisuals")]
    [SerializeField] private Sprite counterOff;
    [SerializeField] private Sprite counterOn;

    [HideInInspector] public VolumeType volumeType;

    public enum VolumeType
    {
        Master,
        Music,
        Sfx,
    }

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

    public void SetAudioPlayerPrefs(GameObject[] masterPoints, GameObject[] musicPoints, GameObject[] sfxPoints)
    {
        masterVolume = PlayerPrefs.GetFloat(MasterVolumePlayerPrefs, masterVolume);
        for (int i = 0; i < masterPoints.Length; i++)
        {
            if (Mathf.RoundToInt(masterVolume * 10) - 1 > i)
            {
                masterPoints[i].GetComponent<Image>().sprite = counterOn;
            }
            else
            {
                masterPoints[i].GetComponent<Image>().sprite = counterOff;
            }
        }
        currentVolumePointMaster = 0;
        foreach (GameObject point in masterPoints)
        {
            if(point.GetComponent<Image>().sprite == counterOn)
            {
                currentVolumePointMaster++;
            }
        }

        musicVolume = PlayerPrefs.GetFloat(MusicVolumePlayerPrefs, musicVolume);
        for (int i = 0; i < musicPoints.Length; i++)
        {
            if (Mathf.RoundToInt(musicVolume * 10) - 1 > i)
            {
                musicPoints[i].GetComponent<Image>().sprite = counterOn;
            }
            else
            {
                musicPoints[i].GetComponent<Image>().sprite = counterOff;
            }
        }
        currentVolumePointMusic = 0;
        foreach (GameObject point in musicPoints)
        {
            if (point.GetComponent<Image>().sprite == counterOn)
            {
                currentVolumePointMusic++;
            }
        }

        sfxVolume = PlayerPrefs.GetFloat(SfxVolumePlayerPrefs, sfxVolume);
        for (int i = 0; i < sfxPoints.Length; i++)
        {
            if (Mathf.RoundToInt(sfxVolume * 10) - 1 > i)
            {
                sfxPoints[i].GetComponent<Image>().sprite = counterOn;
            }
            else
            {
                sfxPoints[i].GetComponent<Image>().sprite = counterOff;
            }
        }
        currentVolumePointSfx = 0;
        foreach (GameObject point in sfxPoints)
        {
            if (point.GetComponent<Image>().sprite == counterOn)
            {
                currentVolumePointSfx++;
            }
        }

        audioMixer.SetFloat("SFX", Mathf.Log10(sfxVolume) * 20);
        audioMixer.SetFloat("Music", Mathf.Log10(musicVolume) * 20);
        audioMixer.SetFloat("Master", Mathf.Log10(masterVolume) * 20);
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

    public void ChangeSpecificVolume(VolumeType volumeType, GameObject[] points, bool makeLouder)
    {
        switch (volumeType)
        {
            case VolumeType.Master:
                if(makeLouder)
                {
                    MasterVolumePlus(points);
                }
                else
                {
                    MasterVolumeMinus(points);
                }
                break;

            case VolumeType.Music:
                if (makeLouder)
                {
                    MusicVolumePlus(points);
                }
                else
                {
                    MusicVolumeMinus(points);
                }
                break;

            case VolumeType.Sfx:
                if (makeLouder)
                {
                    SfxVolumePlus(points);
                }
                else
                {
                    SfxVolumeMinus(points);
                }
                break;

            default:
                break;
        }
    }

    private void MusicVolumeMinus(GameObject[] musicPoints)
    {
        if (musicVolume <= 0)
            return;

        sameDirectionMinusMusic++;

        if (sameDirectionPlusMusic > 0)
        {
            currentVolumePointMusic++;
        }

        sameDirectionPlusMusic = 0;

        if (!(currentVolumePointMusic <= 0.2f))
        {
            currentVolumePointMusic--;
            musicPoints[currentVolumePointMusic].GetComponent<Image>().sprite = counterOff;
            ChangeMusicVolume(-.1f);
        }
    }

    private void MusicVolumePlus(GameObject[] musicPoints)
    {
        if (musicVolume >= 1)
            return;

        sameDirectionPlusMusic++;

        if (sameDirectionMinusMusic > 0)
        {
            currentVolumePointMusic--;
        }

        sameDirectionMinusMusic = 0;

        if (!(musicVolume > 0.99f))
        {
            currentVolumePointMusic++;
            musicPoints[currentVolumePointMusic].GetComponent<Image>().sprite = counterOn;
            ChangeMusicVolume(.1f);
        }
    }

    private void MasterVolumeMinus(GameObject[] masterPoints)
    {
        if (masterVolume <= 0)
            return;

        sameDirectionMinusMaster++;

        if (sameDirectionPlusMaster > 0)
        {
            currentVolumePointMaster++;
        }

        sameDirectionPlusMaster = 0;

        if (!(currentVolumePointMaster <= 0.2f))
        {
            currentVolumePointMaster--;
            masterPoints[currentVolumePointMaster].GetComponent<Image>().sprite = counterOff;
            ChangeMasterVolume(-.1f);
            AudioManager.Instance.Play("Shooting");
        }
    }

    private void MasterVolumePlus(GameObject[] masterPoints)
    {
        if (masterVolume >= 1)
            return;

        sameDirectionPlusMaster++;

        if (sameDirectionMinusMaster > 0)
        {
            currentVolumePointMaster--;
        }

        sameDirectionMinusMaster = 0;

        if (!(masterVolume > 0.99f))
        {
            currentVolumePointMaster++;
            masterPoints[currentVolumePointMaster].GetComponent<Image>().sprite = counterOn;
            ChangeMasterVolume(.1f);
            AudioManager.Instance.Play("Shooting");
        }
    }

    private void SfxVolumeMinus(GameObject[] sfxPoints)
    {
        if (sfxVolume <= 0)
            return;

        sameDirectionMinusSfx++;

        if (sameDirectionPlusSfx > 0)
        {
            currentVolumePointSfx++;
        }

        sameDirectionPlusSfx = 0;

        if (!(currentVolumePointSfx <= 0.2f))
        {
            currentVolumePointSfx--;
            sfxPoints[currentVolumePointSfx].GetComponent<Image>().sprite = counterOff;
            ChangeSfxVolume(-.1f);
            AudioManager.Instance.Play("Shooting");
        }
    }

    private void SfxVolumePlus(GameObject[] sfxPoints)
    {
        if (sfxVolume >= 1)
            return;

        sameDirectionPlusSfx++;

        if (sameDirectionMinusSfx > 0)
        {
            currentVolumePointSfx--;
        }

        sameDirectionMinusSfx = 0;

        if (!(sfxVolume > 0.99f))
        {
            currentVolumePointSfx++;
            sfxPoints[currentVolumePointSfx].GetComponent<Image>().sprite = counterOn;
            ChangeSfxVolume(.1f);
            AudioManager.Instance.Play("Shooting");
        }
    }

    private void ChangeMasterVolume(float value)
    {
        masterVolume += value;

        audioMixer.SetFloat("Master", Mathf.Log10(masterVolume) * 20);

        PlayerPrefs.SetFloat(MasterVolumePlayerPrefs, masterVolume);
    }

    private void ChangeMusicVolume(float value)
    {
        musicVolume += value;

        audioMixer.SetFloat("Music", Mathf.Log10(musicVolume) * 20);

        PlayerPrefs.SetFloat(MusicVolumePlayerPrefs, musicVolume);
    }

    private void ChangeSfxVolume(float value)
    {
        sfxVolume += value;

        audioMixer.SetFloat("SFX", Mathf.Log10(sfxVolume) * 20);

        PlayerPrefs.SetFloat(SfxVolumePlayerPrefs, sfxVolume);
    }
}
