using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Score / Lives")]
    public int scoreThisRun = 0;
    public int totalPoints  = 0;
    public int lives        = 3;
    public int startingLives = 3;

    [Header("UI (Binders)")]
    public AutoRTLNumberBinder scoreTextBinder;
    public AutoRTLNumberBinder livesTextBinder;
    public AutoRTLNumberBinder totalPointsMenuBinder;
    public AutoRTLNumberBinder totalPointsInGameBinder;
    public AutoRTLNumberBinder finalScoreTextBinder;

    [Header("Refs")]
    public BallManager  ballManager;
    public SpawnManager spawnManager;

    [HideInInspector] public bool cheatUnlimited = false;

    public bool IsRunActive => isRunActive;
    bool isRunActive = false;

    Coroutine applauseCo;

    void Awake()
    {
        Instance    = this;
        totalPoints = PlayerPrefs.GetInt("TotalPoints", 0);
        Time.timeScale = 1f;
        UpdateUI();
    }

    public void NewRunReset(int startLives)
    {
        foreach (var p in FindObjectsOfType<Pickup>()) Destroy(p.gameObject);

        scoreThisRun   = 0;
        lives          = startLives;
        isRunActive    = false;
        cheatUnlimited = false;

        UpdateUI();
    }

    public void OnRunStarted()
    {
        if (isRunActive) return;
        isRunActive = true;

        spawnManager?.StartSpawning();

        if (applauseCo != null) StopCoroutine(applauseCo);
        applauseCo = StartCoroutine(CoApplauseLoop());
    }

    public void OnRunEnded()
    {
        if (!isRunActive) return;
        isRunActive    = false;
        cheatUnlimited = false;

        spawnManager?.StopSpawning();

        if (applauseCo != null)
        {
            StopCoroutine(applauseCo);
            applauseCo = null;
        }
    }

    IEnumerator CoApplauseLoop()
    {
        while (isRunActive)
        {
            yield return new WaitForSeconds(Random.Range(9f, 13f));
            if (isRunActive) AudioManager.Instance?.PlayApplause();
        }
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        scoreThisRun += amount;
        UpdateUI();
    }

    public void AddToTotal(int amount)
    {
        totalPoints += amount;
        if (totalPoints < 0) totalPoints = 0;
        PlayerPrefs.SetInt("TotalPoints", totalPoints);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public bool TrySpendPoints(int price)
    {
        if (cheatUnlimited) return true;
        if (totalPoints < price) return false;

        totalPoints -= price;
        PlayerPrefs.SetInt("TotalPoints", totalPoints);
        PlayerPrefs.Save();
        UpdateUI();
        return true;
    }

    public void LoseLife(int amount = 1)
    {
        lives -= amount;
        AudioManager.Instance?.PlayOh();
        UpdateUI();

        if (lives <= 0)
        {
            OnRunEnded();
            AddToTotal(scoreThisRun);

            AudioManager.Instance?.PauseBG();
            AudioManager.Instance?.PlayLose();

            if (finalScoreTextBinder)
                finalScoreTextBinder.SetRaw(FaTextUtility.FormatFinalScoreLabeled(scoreThisRun));

            UIManager.Instance.ShowGameOverPanel();
        }
        else
        {
            StartCoroutine(CoRespawnAfterDelay(1.0f));
        }
    }

    IEnumerator CoRespawnAfterDelay(float sec)
    {
        yield return new WaitForSecondsRealtime(sec);
        ballManager.SpawnSelectedBall();
    }

    public void StartSlowMotion(float duration)
    {
        StopCoroutine(nameof(CoSlowMotion));
        StartCoroutine(CoSlowMotion(duration));
    }

    IEnumerator CoSlowMotion(float d)
    {
        float prev = Time.timeScale;
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(d);
        Time.timeScale = prev;
    }

    public void AddLife(int amount = 1)
    {
        lives += amount;
        UpdateUI();
    }

    public void OnPickupCollected()
    {
        AudioManager.Instance?.PlayMagic();
    }

    public void UpdateUI()
    {
        if (scoreTextBinder)  scoreTextBinder.SetValue(scoreThisRun);
        if (livesTextBinder)  livesTextBinder.SetValue(lives);
        if (totalPointsMenuBinder)   totalPointsMenuBinder.SetValue(totalPoints);
        if (totalPointsInGameBinder) totalPointsInGameBinder.SetValue(totalPoints);
    }
}
