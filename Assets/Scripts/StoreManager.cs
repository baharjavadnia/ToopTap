using System;
using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance;

    [Header("Catalog")]
    public List<StoreItemData> items = new List<StoreItemData>();

    // Pref keys
    public const string ownedSetPref        = "OwnedItemsSet_v1";
    public const string selectedBallIdPref  = "SelectedBallID";
    public const string selectedBgIdPref    = "SelectedBgID";

    [Header("Defaults")]
    public static string defaultBallId = "Ball_Football";
    public static string defaultBgId   = "BG_Default";

    public event Action<string> OnPurchased;   // itemID

    private HashSet<string> owned = new HashSet<string>();

    void Awake()
    {
        Instance = this;
        LoadOwned();
        EnsureDefaultsOwned();
    }

    void LoadOwned()
    {
        owned.Clear();
        string csv = PlayerPrefs.GetString(ownedSetPref, "");
        if (!string.IsNullOrEmpty(csv))
        {
            foreach (var part in csv.Split(','))
                if (!string.IsNullOrWhiteSpace(part)) owned.Add(part.Trim());
        }
    }

    void SaveOwned()
    {
        string csv = string.Join(",", owned);
        PlayerPrefs.SetString(ownedSetPref, csv);
        PlayerPrefs.Save();
    }

    string GetDefaultBgId()
    {
        return BackgroundManager.Instance ? BackgroundManager.Instance.defaultBackgroundId : defaultBgId;
    }

    void EnsureDefaultsOwned()
    {
        string defBall = defaultBallId;
        string defBg   = GetDefaultBgId();

        if (!string.IsNullOrEmpty(defBall)) owned.Add(defBall);
        if (!string.IsNullOrEmpty(defBg))   owned.Add(defBg);

        SaveOwned();
    }

    public static bool IsPurchased(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return false;

        string defBg = Instance ? Instance.GetDefaultBgId() : defaultBgId;
        if (itemId == defaultBallId || itemId == defBg) return true;

        if (Instance == null)
        {
            string csv = PlayerPrefs.GetString(ownedSetPref, "");
            return ("," + csv + ",").Contains("," + itemId + ",");
        }
        return Instance.owned.Contains(itemId);
    }

    public bool IsPurchasedInstance(string itemId) => IsPurchased(itemId);

    public void SelectItem(string itemId)
    {
        if (!IsPurchased(itemId)) { Debug.LogWarning("[StoreManager] Not owned: " + itemId); return; }

        if (itemId.StartsWith("Ball_"))
        {
            PlayerPrefs.SetString(selectedBallIdPref, itemId);
            PlayerPrefs.Save();
            if (BallManager.Instance) BallManager.Instance.SetBallByID(itemId, previewOnly: true);
        }
        else
        {
            PlayerPrefs.SetString(selectedBgIdPref, itemId);
            PlayerPrefs.Save();
            if (BackgroundManager.Instance) BackgroundManager.Instance.SetBackgroundByID(itemId);
        }
    }

    public bool BuyWithPoints(string itemId, int pricePoints)
    {
        if (IsPurchased(itemId)) return true;

        if (!GameManager.Instance.TrySpendPoints(pricePoints)) return false;

        owned.Add(itemId);
        SaveOwned();
        OnPurchased?.Invoke(itemId);
        return true;
    }

    public void MarkPurchased(string itemId)
    {
        if (IsPurchased(itemId)) return;
        owned.Add(itemId);
        SaveOwned();
        OnPurchased?.Invoke(itemId);
    }

    public StoreItemData GetItem(string itemId) => items.Find(x => x.itemID == itemId);
}
