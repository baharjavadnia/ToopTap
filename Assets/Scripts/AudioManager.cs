using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public const string KEY_SOUND = "SoundOn";
    public const string KEY_MUSIC = "MusicOn";

    [Header("Clips")]
    public AudioClip bgMusic;
    public AudioClip tap;
    public AudioClip oh;
    public AudioClip lose;
    public AudioClip applause;
    public AudioClip magic;
    public AudioClip countdown;
    public AudioClip coin;          // ⬅️ NEW: صدای خرید موفق

    [Header("Volumes")]
    [Range(0f,1f)] public float musicVolume = 0.6f;
    [Range(0f,1f)] public float sfxVolume   = 1.0f;

    private AudioSource musicSource, sfxSource, countdownSource;
    private bool isSoundOn = true, isMusicOn = true;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>(); musicSource.loop = true;
        sfxSource = gameObject.AddComponent<AudioSource>();
        countdownSource = gameObject.AddComponent<AudioSource>(); countdownSource.loop = true;

        musicSource.playOnAwake = sfxSource.playOnAwake = countdownSource.playOnAwake = false;
        musicSource.spatialBlend = sfxSource.spatialBlend = countdownSource.spatialBlend = 0f;

        isSoundOn = PlayerPrefs.GetInt(KEY_SOUND, 1) == 1;
        isMusicOn = PlayerPrefs.GetInt(KEY_MUSIC, 1) == 1;
        ApplySettingsInternal();
        EnsureBGPlaying();
    }

    public void SetSoundOn(bool on){ isSoundOn = on; PlayerPrefs.SetInt(KEY_SOUND, on?1:0); ApplySettingsInternal(); if(!on) StopCountdownLoop(); }
    public void SetMusicOn(bool on){ isMusicOn = on; PlayerPrefs.SetInt(KEY_MUSIC, on?1:0); ApplySettingsInternal(); }
    public void ApplySettingsFromPrefs(){
        isSoundOn = PlayerPrefs.GetInt(KEY_SOUND, 1) == 1;
        isMusicOn = PlayerPrefs.GetInt(KEY_MUSIC, 1) == 1;
        ApplySettingsInternal();
    }
    void ApplySettingsInternal(){
        musicSource.volume = isMusicOn ? musicVolume : 0f;
        sfxSource.volume = isSoundOn ? sfxVolume : 0f;
        countdownSource.volume = isSoundOn ? sfxVolume : 0f;
        if (!isMusicOn) musicSource.Pause(); else EnsureBGPlaying();
    }

    public void EnsureBGPlaying(){ if(!isMusicOn || !bgMusic) return; if(musicSource.clip!=bgMusic) musicSource.clip=bgMusic; if(!musicSource.isPlaying) musicSource.Play(); }
    public void PauseBG(){ if (musicSource.isPlaying) musicSource.Pause(); }
    public void ResumeBG(){ if (isMusicOn) EnsureBGPlaying(); }
    public void StopBG(){ if (musicSource.isPlaying) musicSource.Stop(); }

    void PlaySFX(AudioClip c, float v=1f){ if(!isSoundOn || !c) return; sfxSource.PlayOneShot(c, sfxVolume*v); }
    public void PlayTap()      => PlaySFX(tap);
    public void PlayOh()       => PlaySFX(oh);
    public void PlayLose()     => PlaySFX(lose);
    public void PlayApplause() => PlaySFX(applause, 0.85f);
    public void PlayMagic()    => PlaySFX(magic);
    public void PlayCoin()     => PlaySFX(coin);

    public void PlayCountdownLoop(){ if(!countdown || !PlayerPrefs.GetInt(KEY_SOUND,1).Equals(1)) return;
        if (countdownSource.clip!=countdown) countdownSource.clip=countdown;
        if (!countdownSource.isPlaying) countdownSource.Play(); }
    public void StopCountdownLoop(){ if (countdownSource.isPlaying) countdownSource.Stop(); }
}
