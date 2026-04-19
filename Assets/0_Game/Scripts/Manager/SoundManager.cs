using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : Singleton<SoundManager>
{
    private const string PrefMasterVolume = "sound_master_volume";
    private const string PrefMusicMute    = "sound_music_mute";

    [SerializeField] private Slider      sliderMusic;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource moveSource;
    [SerializeField] private AudioClip   backgroundMenu;
    [SerializeField] private AudioClip   levelBG;
    [SerializeField] private AudioClip   effectButton;
    [SerializeField] private AudioClip   effectShoot;
    [SerializeField] private AudioClip   effectTakeDamage;
    [SerializeField] private AudioClip   effectBooster;
    [SerializeField] private AudioClip   effectBoom;
    [SerializeField] private AudioClip   effectCoin;
    [SerializeField] private AudioClip   effectDeath;
    [SerializeField] private AudioClip   effectWin;
    [SerializeField] private AudioClip   effectLose;

    // Start is called before the first frame update
    private void Start()
    {
        if (sliderMusic != null)
        {
            sliderMusic.onValueChanged.AddListener(val => ChangeMasterVolume(val));
        }
        LoadSoundSettings();
    }

    public void PlayEffect(AudioClip clip)
    {
        musicSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip != clip)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void ChangeMasterVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(PrefMasterVolume, value);
        PlayerPrefs.Save();
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
        PlayerPrefs.SetInt(PrefMusicMute, musicSource.mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSoundSettings()
    {
        float volume = PlayerPrefs.GetFloat(PrefMasterVolume, 1f);
        AudioListener.volume = volume;

        if (sliderMusic != null)
        {
            sliderMusic.SetValueWithoutNotify(volume);
        }

        if (musicSource != null)
        {
            musicSource.mute = PlayerPrefs.GetInt(PrefMusicMute, 0) == 1;
        }
    }

    public void OnClickButton()
    {
        this.PlayEffect(effectButton);
    }

    public void OnShoot()
    {
        this.PlayEffect(this.effectShoot);
    }

    public void OnTakeDamage()
    {
        this.PlayEffect(this.effectTakeDamage);
    }

    public void OnBooster()
    {
        this.PlayEffect(this.effectBooster);
    }

    public void OnBoom()
    {
        this.PlayEffect(this.effectBoom);
    }

    public void OnCoin()
    {
        this.PlayEffect(this.effectCoin);
    }

    public void OnDeath()
    {
        this.PlayEffect(this.effectDeath);
    }

    public void OnMove()
    {
        if (!moveSource.isPlaying)
        {
            moveSource.Play();
        }
    }

    public void OnStopMove()
    {
        if (moveSource.isPlaying)
        {
            moveSource.Stop();
        }
    }

    public void OnChangeToMenu()
    {
        this.PlayMusic(this.backgroundMenu);
    }

    public void OnInGame()
    {
        this.PlayMusic(this.levelBG);
    }

    public void OnWin()
    {
        this.PlayMusic(this.effectWin);
    }

    public void OnLose()
    {
        this.PlayMusic(this.effectLose);
    }
}
