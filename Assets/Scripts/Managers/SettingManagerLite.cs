using UnityEngine;
using UnityEngine.UI;

public class SettingManagerLite : MonoBehaviour
{
    [Header("Buttons & Icons")]
    public Button soundButton;
    public Sprite soundOnIcon;
    public Sprite soundOffIcon;

    public Button musicButton;
    public Sprite musicOnIcon;
    public Sprite musicOffIcon;

    Image soundImg, musicImg;
    bool isSoundOn, isMusicOn;

    void Start()
    {
        soundImg = soundButton ? soundButton.GetComponent<Image>() : null;
        musicImg = musicButton ? musicButton.GetComponent<Image>() : null;

        isSoundOn = PlayerPrefs.GetInt(AudioManager.KEY_SOUND, 1) == 1;
        isMusicOn = PlayerPrefs.GetInt(AudioManager.KEY_MUSIC, 1) == 1;

        ApplyIcons();

        if (soundButton) soundButton.onClick.AddListener(ToggleSound);
        if (musicButton) musicButton.onClick.AddListener(ToggleMusic);
    }

    void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        AudioManager.Instance?.SetSoundOn(isSoundOn);
        ApplyIcons();
        AudioManager.Instance?.PlayTap();
    }

    void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        AudioManager.Instance?.SetMusicOn(isMusicOn);
        ApplyIcons();
        AudioManager.Instance?.PlayTap();
    }

    void ApplyIcons()
    {
        if (soundImg) soundImg.sprite = isSoundOn ? soundOnIcon : soundOffIcon;
        if (musicImg) musicImg.sprite = isMusicOn ? musicOnIcon : musicOffIcon;
    }
}
