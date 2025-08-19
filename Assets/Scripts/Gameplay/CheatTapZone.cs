using UnityEngine;

public class CheatTapZone : MonoBehaviour
{
    [Header("Zone (px from bottom-center)")]
    public int zoneWidthPx = 220;
    public int zoneHeightPx = 180;
    public int zoneBottomOffsetPx = 0;

    [Header("Trigger")]
    public float tapMaxInterval = 0.45f;
    public int infinitePointsValue = 9999;

    [Header("Where to work")]
    public bool enableInMainMenu = true;
    public bool enableInGame     = true;

    int tapCount = 0;
    float lastTapTime = -999f;

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            TryHandleTap((Vector2)Input.mousePosition);
#else
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
            TryHandleTap(Input.touches[0].position);
#endif
    }

    void TryHandleTap(Vector2 screenPos)
    {
        if (!IsInsideZone(screenPos)) return;
        if (!PassesPanelFilter()) return;

        float now = Time.realtimeSinceStartup;
        tapCount = (now - lastTapTime <= tapMaxInterval) ? tapCount + 1 : 1;
        lastTapTime = now;

        if (tapCount >= 3)
        {
            tapCount = 0;
            ToggleInfinitePoints();
        }
    }

    bool PassesPanelFilter()
    {
        if (!UIManager.Instance) return true;
        bool inMain = UIManager.Instance.MainMenuPanel && UIManager.Instance.MainMenuPanel.activeInHierarchy;
        bool inGame = UIManager.Instance.InGamePanel   && UIManager.Instance.InGamePanel.activeInHierarchy;
        if (inMain && enableInMainMenu) return true;
        if (inGame && enableInGame)     return true;
        return false;
    }

    void ToggleInfinitePoints()
    {
        var gm = GameManager.Instance;
        if (!gm) return;

        bool on = gm.totalPoints >= infinitePointsValue;
        int target = on ? 0 : infinitePointsValue;

        gm.totalPoints = Mathf.Max(0, target);
        PlayerPrefs.SetInt("TotalPoints", gm.totalPoints);
        PlayerPrefs.Save();
        gm.UpdateUI();

        var msg = on ? "حالت نامحدود غیرفعال شد" : "حالت نامحدود فعال شد";
        if (on) UIManager.Instance?.ShowToastError(msg, 1.2f);
        else    UIManager.Instance?.ShowToastSuccess(msg, 1.2f);
    }

    bool IsInsideZone(Vector2 screenPos)
    {
        float cx = Screen.width * 0.5f;
        float xMin = cx - zoneWidthPx * 0.5f;
        float xMax = cx + zoneWidthPx * 0.5f;
        float yMin = zoneBottomOffsetPx;
        float yMax = zoneBottomOffsetPx + zoneHeightPx;
        return (screenPos.x >= xMin && screenPos.x <= xMax &&
                screenPos.y >= yMin && screenPos.y <= yMax);
    }
}
