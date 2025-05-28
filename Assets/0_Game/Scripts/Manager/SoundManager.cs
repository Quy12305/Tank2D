using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private Slider      sliderMusic;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource moveSource;
    [SerializeField] private AudioClip   backgroundMenu;
    [SerializeField] private AudioClip   levelBG;
    [SerializeField] private AudioClip   effectButton;
    [SerializeField] private AudioClip   effectShoot;
    [SerializeField] private AudioClip   effectTakeDamage;
    [SerializeField] private AudioClip   effectBooster;
    [SerializeField] private AudioClip   effectWin;
    [SerializeField] private AudioClip   effectLose;

    // Start is called before the first frame update
    private void Start()
    {
        sliderMusic.onValueChanged.AddListener(val => ChangeMasterVolume(val));
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
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
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