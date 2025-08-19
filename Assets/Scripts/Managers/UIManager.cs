using UnityEngine;
using RTLTMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Binders (HUD/Panels)")]
    public AutoRTLNumberBinder scoreTextBinder;
    public AutoRTLNumberBinder livesTextBinder;
    public AutoRTLNumberBinder totalPointsTextBinder;
    public AutoRTLNumberBinder finalScoreTextBinder;

    [Header("Panels")]
    public GameObject MainMenuPanel;
    public GameObject InGamePanel;
    public GameObject GameOverPanel;
    public GameObject StorePanel;
    public GameObject SettingsPanel;

    [Header("Countdown / Toast")]
    public RTLTextMeshPro countdownText;
    public RTLTextMeshPro toastText;

    [Header("Toast Colors")]
    public Color toastSuccess = new Color(0.20f, 0.80f, 0.20f); // سبز
    public Color toastError   = new Color(0.90f, 0.20f, 0.20f); // قرمز
    public Color toastInfo    = new Color(0.95f, 0.75f, 0.10f); // زرد/اطلاعات

    Coroutine toastCo;

    void Awake() => Instance = this;
    void Start() => ShowMainMenu();

    public void ShowMainMenu()
    {
        ShowOnly(MainMenuPanel);
        Time.timeScale = 1f;

        GameManager.Instance.NewRunReset(GameManager.Instance.startingLives);
        GameManager.Instance.OnRunEnded();

        AudioManager.Instance?.EnsureBGPlaying();
    }

    public void ShowGameOverPanel()
    {
        ShowOnly(GameOverPanel);
        Time.timeScale = 1f;
    }

    public void ShowStore()  { ShowOnly(StorePanel);    AudioManager.Instance?.PlayTap(); }
    public void CloseStore() { ShowMainMenu();          AudioManager.Instance?.PlayTap(); }
    public void ShowSettings(){ ShowOnly(SettingsPanel);AudioManager.Instance?.PlayTap(); }
    public void CloseSettings(){ ShowMainMenu();        AudioManager.Instance?.PlayTap(); }

    public void PlayWithCountdown()
    {
        AudioManager.Instance?.EnsureBGPlaying();
        StartCoroutine(CoPlayCountdownThenStart());
        AudioManager.Instance?.PlayTap();
    }

    public void RestartRun()
    {
        AudioManager.Instance?.EnsureBGPlaying();
        StartCoroutine(CoRestartCountdown());
        AudioManager.Instance?.PlayTap();
    }

    IEnumerator CoPlayCountdownThenStart()
    {
        ShowOnly(InGamePanel);
        yield return StartCoroutine(CoCountdown());
        GameManager.Instance.OnRunStarted();
        GameManager.Instance.ballManager.SpawnSelectedBall();
    }

    IEnumerator CoRestartCountdown()
    {
        GameManager.Instance.NewRunReset(GameManager.Instance.startingLives);
        ShowOnly(InGamePanel);
        yield return StartCoroutine(CoCountdown());
        GameManager.Instance.OnRunStarted();
        GameManager.Instance.ballManager.SpawnSelectedBall();
    }

    IEnumerator CoCountdown()
    {
        if (!countdownText) yield break;

        countdownText.gameObject.SetActive(true);
        AudioManager.Instance?.PlayCountdownLoop();

        for (int n = 3; n >= 1; n--)
        {
            RTLUI.Set(countdownText, FaTextUtility.FormatCountdown(n));
            yield return new WaitForSecondsRealtime(1f);
        }

        countdownText.gameObject.SetActive(false);
        AudioManager.Instance?.StopCountdownLoop();
    }

    void ShowOnly(GameObject target)
    {
        SetActiveSafe(MainMenuPanel, target == MainMenuPanel);
        SetActiveSafe(InGamePanel,  target == InGamePanel);
        SetActiveSafe(GameOverPanel, target == GameOverPanel);
        SetActiveSafe(StorePanel,    target == StorePanel);
        SetActiveSafe(SettingsPanel, target == SettingsPanel);
    }
    void SetActiveSafe(GameObject go, bool v) { if (go) go.SetActive(v); }

    // ----------------- Toast APIs -----------------
    // قبلی (سازگار با کدهای قدیمی)
    public void ShowToast(string msg, float duration = 1.5f)
        => ShowToast(msg, duration, toastInfo);

    // اورلود جدید با رنگ
    public void ShowToast(string msg, float duration, Color color)
    {
        if (!toastText) return;
        if (toastCo != null) StopCoroutine(toastCo);
        toastCo = StartCoroutine(CoToast(msg, duration, color));
    }

    // شورت‌کات‌های راحت
    public void ShowToastSuccess(string msg, float duration = 1.5f)
        => ShowToast(msg, duration, toastSuccess);

    public void ShowToastError(string msg, float duration = 1.5f)
        => ShowToast(msg, duration, toastError);

    IEnumerator CoToast(string msg, float duration, Color color)
    {
        toastText.color = color;
        RTLUI.Set(toastText, msg);
        toastText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(duration);
        toastText.gameObject.SetActive(false);
    }
}
