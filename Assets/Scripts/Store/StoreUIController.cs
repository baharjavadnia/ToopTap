using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using System;
using System.Collections.Generic;

public class StoreUIController : MonoBehaviour
{
    public static StoreUIController Instance;

    [Header("Managers")]
    public StoreManager storeManager;

    [Header("Purchase Panel")]
    public PurchasePanel purchasePanel;

    [Header("Card UI Prices")]
    public RTLTextMeshPro cardPriceScoreText;
    public RTLTextMeshPro cardPriceMoneyText;

    [Header("Card UI (single card)")]
    public Image          cardIconImage;
    public RTLTextMeshPro cardNameText;
    public GameObject     badgeLock;
    public GameObject     badgeSelected;
    public Button         buyButton;
    public RTLTextMeshPro buyButtonText;

    [Header("Buy Button Visual")]
    public Image buyButtonImage;
    [Range(0f,1f)] public float selectedAlpha = 0.5f;

    [Header("Tabs / Arrows (optional)")]
    public Button ballsTabButton, bgsTabButton, leftArrowButton, rightArrowButton;

    [Header("Data")]
    public List<StoreItemData> balls = new List<StoreItemData>();
    public List<StoreItemData> backgrounds = new List<StoreItemData>();

    bool showingBalls = true;
    int currentIndex = 0;

    private Action<string> purchasedHandler;

    void Awake() => Instance = this;

    void Start()
    {
        if (ballsTabButton)  ballsTabButton.onClick.AddListener(() => SelectTab(true));
        if (bgsTabButton)    bgsTabButton.onClick.AddListener(() => SelectTab(false));
        if (leftArrowButton) leftArrowButton.onClick.AddListener(() => Move(-1));
        if (rightArrowButton)rightArrowButton.onClick.AddListener(() => Move(+1));
        if (buyButton)       buyButton.onClick.AddListener(OnBuyPressed);

        if (storeManager)
        {
            purchasedHandler = _ => RefreshCard();
            storeManager.OnPurchased += purchasedHandler;
        }

        SelectTab(true);
    }

    void OnDestroy()
    {
        if (storeManager != null && purchasedHandler != null)
            storeManager.OnPurchased -= purchasedHandler;
    }

    void SelectTab(bool ballsTab)
    {
        showingBalls = ballsTab;
        currentIndex = FindFirstOwnedIndex(GetList());
        UpdateCard();
    }

    List<StoreItemData> GetList() => showingBalls ? balls : backgrounds;

    int FindFirstOwnedIndex(List<StoreItemData> list)
    {
        for (int i = 0; i < list.Count; i++)
            if (StoreManager.IsPurchased(list[i].itemID)) return i;
        return 0;
    }

    void Move(int dir)
    {
        var list = GetList();
        if (list == null || list.Count == 0) return;
        currentIndex = (currentIndex + dir + list.Count) % list.Count;
        UpdateCard();
    }

    public void RefreshCard() => UpdateCard();

    void UpdateCard()
    {
        var list = GetList();
        if (list == null || list.Count == 0) return;

        currentIndex = Mathf.Clamp(currentIndex, 0, list.Count - 1);
        var data = list[currentIndex];
        if (data == null) return;

        var icon = data.GetIconSprite();
        if (cardIconImage)
        {
            cardIconImage.enabled = (icon != null);
            cardIconImage.sprite = icon;
            cardIconImage.preserveAspect = true;
            cardIconImage.color = Color.white;
        }

        if (cardNameText)
            RTLUI.Set(cardNameText, FaTextUtility.GetDisplayName(data.itemID, data.displayName, showingBalls));

        bool owned    = StoreManager.IsPurchased(data.itemID);
        bool selected = IsSelected(data.itemID);

        if (cardPriceScoreText)
            RTLUI.Set(cardPriceScoreText, owned ? FaTextUtility.Label_Owned
                                                : FaTextUtility.FormatPricePoints(data.pricePoints));
        if (cardPriceMoneyText)
            RTLUI.Set(cardPriceMoneyText, owned ? FaTextUtility.Label_Owned
                                                : FaTextUtility.FormatPriceTomans(data.priceToman));

        if (badgeLock)
        {
            badgeLock.SetActive(!owned);
            var imgLock = badgeLock.GetComponent<Image>();
            if (imgLock) imgLock.color = Color.white;
        }
        if (badgeSelected) badgeSelected.SetActive(false);

        if (buyButtonText)
        {
            string btxt = !owned ? FaTextUtility.Label_Buy
                                 : (selected ? FaTextUtility.Label_Selected
                                             : FaTextUtility.Label_Select);
            RTLUI.Set(buyButtonText, btxt);
        }

        ApplyBuyButtonOpacity(selected);
    }

    void ApplyBuyButtonOpacity(bool isSelected)
    {
        if (!buyButtonImage) return;
        var c = buyButtonImage.color;
        c.a = isSelected ? selectedAlpha : 1f;
        buyButtonImage.color = c;
    }

    bool IsSelected(string itemID)
    {
        if (itemID.StartsWith("Ball_"))
        {
            string sel = PlayerPrefs.GetString(StoreManager.selectedBallIdPref, StoreManager.defaultBallId);
            return sel == itemID;
        }
        else
        {
            return BackgroundManager.Instance && BackgroundManager.Instance.IsCurrent(itemID);
        }
    }

    void OnBuyPressed()
    {
        var list = GetList();
        if (list == null || list.Count == 0) return;

        var data  = list[currentIndex];
        bool owned = StoreManager.IsPurchased(data.itemID);

        if (!owned)
        {
            purchasePanel?.OpenForItem(data, storeManager);
        }
        else
        {
            storeManager.SelectItem(data.itemID);
            UpdateCard();
        }
    }
}
