
using UnityEngine;

public class SlotAudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource reelLoopSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Gameplay Sound Effects")]
    [SerializeField] private AudioClip spinStartSFX;
    [SerializeField] private AudioClip reelSpinningSFX;
    [SerializeField] private AudioClip reelStopSFX;
    [SerializeField] private AudioClip winSFX;
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip bonusSFX;

    [Header("Volume Settings")]
    [SerializeField, Range(0f, 1f)]
    private float musicVolume = 0.5f;

    [SerializeField, Range(0f, 1f)]
    private float sfxVolume = 1f;

    private void Start()
    {
        ConfigureAudioSources();
        PlayBackgroundMusic();
    }

    // Configure the AudioSources for their intended purposes.
    private void ConfigureAudioSources()
    {
        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.volume = 1f;
        }

        if (reelLoopSource != null)
        {
            reelLoopSource.playOnAwake = false;
            reelLoopSource.loop = true;
            reelLoopSource.volume = sfxVolume;
        }
    }

    // Plays the background music continuously.
    public void PlayBackgroundMusic()
    {
        if (musicSource == null || backgroundMusic == null)
            return;

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    // Stops the background music.
    public void StopBackgroundMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    // Plays when the player starts a spin.
    public void PlaySpinStart()
    {
        PlaySFX(spinStartSFX);
    }

    // Starts the continuous reel-spinning sound.
    public void StartReelLoop()
    {
        if (reelLoopSource == null || reelSpinningSFX == null)
            return;

        if (reelLoopSource.isPlaying)
            return;

        reelLoopSource.clip = reelSpinningSFX;
        reelLoopSource.loop = true;
        reelLoopSource.volume = sfxVolume;

        reelLoopSource.Play();
    }

    // Stops the reel-spinning sound.
    public void StopReelLoop()
    {
        if (reelLoopSource != null)
            reelLoopSource.Stop();
    }

    // Plays when an individual reel stops.
    public void PlayReelStop()
    {
        PlaySFX(reelStopSFX);
    }

    // Plays when the player receives a winning payout.
    public void PlayWin()
    {
        PlaySFX(winSFX);
    }

    // Plays when the player clicks a UI button.
    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSFX);
    }

    // Plays when the player receives EXTRA free spins.
    public void PlayBonus()
    {
        PlaySFX(bonusSFX);
    }

    // Helper method for playing short sound effects.
    private void PlaySFX(AudioClip clip)
    {
        // No audio assigned yet? Simply skip playback.
        if (sfxSource == null || clip == null)
            return;

        // Allows multiple short effects to overlap.
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // Allows us to change music volume at runtime.
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    // Allows us to change SFX volume at runtime.
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        // PlayOneShot uses the AudioSource volume as well.
        // Keep the base source volume at 1 to avoid applying
        // the SFX volume twice.
        if (sfxSource != null)
            sfxSource.volume = 1f;

        if (reelLoopSource != null)
            reelLoopSource.volume = sfxVolume;
    }
}
