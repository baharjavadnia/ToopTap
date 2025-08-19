using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using Bazaar.Poolakey.Data; // Result<T>, PurchaseInfo
using Bazaar.Data;          // Status

public class PurchasePanel : MonoBehaviour
{
    [Header("Root")] public GameObject root;

    [Header("UI")]
    public RTLTextMeshPro purchaseTitleText;
    public Button         purchaseScoreButton;
    public RTLTextMeshPro purchaseScoreButtonText;
    public Button         purchaseMoneyButton;
    public RTLTextMeshPro purchaseMoneyButtonText;
    public Button         cancelButton;
    public RTLTextMeshPro toastText; // optional

    private StoreItemData currentItem;
    private StoreManager  storeManager;
    private bool          subscribedToStore = false;

    void Awake() { if (!root) root = gameObject; }

    void Start()
    {
        if (cancelButton)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(OnCancelClicked);
        }
        if (purchaseScoreButton)
        {
            purchaseScoreButton.onClick.RemoveAllListeners();
            purchaseScoreButton.onClick.AddListener(OnBuyScoreClicked);
        }
        if (purchaseMoneyButton)
        {
            purchaseMoneyButton.onClick.RemoveAllListeners();
            purchaseMoneyButton.onClick.AddListener(OnBuyMoneyClicked);
        }
        Close();
    }

    public void OpenForItem(StoreItemData item, StoreManager mgr)
    {
        UnsubscribeFromStore();
        currentItem  = item;
        storeManager = mgr;

        if (storeManager != null)
        {
            storeManager.OnPurchased += HandlePurchasedAndClose;
            subscribedToStore = true;
        }

        if (purchaseTitleText)       RTLUI.Set(purchaseTitleText,       FaTextUtility.GetDisplayName(item.itemID));
        if (purchaseScoreButtonText) RTLUI.Set(purchaseScoreButtonText, FaTextUtility.FormatPricePoints(item.pricePoints));
        if (purchaseMoneyButtonText) RTLUI.Set(purchaseMoneyButtonText, FaTextUtility.FormatPriceTomans(item.priceToman));

        SetActive(true);
        SetToast("");
    }

    public void OnCancelClicked() { AudioManager.Instance?.PlayTap(); Close(); }

    public void Close()
    {
        SetActive(false);
        UnsubscribeFromStore();
    }

    void SetActive(bool v)
    {
        if (root) root.SetActive(v);
        var cg = root ? root.GetComponent<CanvasGroup>() : null;
        if (cg)
        {
            cg.alpha = v ? 1f : 0f;
            cg.blocksRaycasts = v;
            cg.interactable   = v;
        }
    }

    void SetToast(string msg, float autoHideSec = 0f, bool success = false)
    {
        if (string.IsNullOrEmpty(msg))
        {
            if (toastText) toastText.gameObject.SetActive(false);
            return;
        }

        if (toastText)
        {
            toastText.color = success ? UIManager.Instance.toastSuccess
                                      : UIManager.Instance.toastError;
            RTLUI.Set(toastText, msg);
            toastText.gameObject.SetActive(true);
        }
        else
        {
            if (success) UIManager.Instance?.ShowToastSuccess(msg, Mathf.Max(1.2f, autoHideSec));
            else         UIManager.Instance?.ShowToastError(msg,   Mathf.Max(1.2f, autoHideSec));
        }
    }

    // خرید با امتیاز
    void OnBuyScoreClicked()
    {
        if (currentItem == null || storeManager == null) return;

        if (storeManager.BuyWithPoints(currentItem.itemID, currentItem.pricePoints))
        {
            AudioManager.Instance?.PlayCoin();
            storeManager.MarkPurchased(currentItem.itemID);
            StoreUIController.Instance?.RefreshCard();

            SetToast(currentItem.itemID.StartsWith("Ball_") ? "توپ جدید مبارک!" : "به زمین بازی جدید خوش اومدی!", 1.6f, true);
            Close();
        }
        else
        {
            AudioManager.Instance?.PlayTap();
            SetToast("به اندازه کافی امتیاز نداری!", 1.8f, false);
        }
    }

    // خرید ریالی — فقط اندروید دیوایس (نه Editor)
    async void OnBuyMoneyClicked()
    {
        if (currentItem == null || storeManager == null)
        {
            AudioManager.Instance?.PlayTap();
            return;
        }

        if (string.IsNullOrEmpty(currentItem.productId))
        {
            AudioManager.Instance?.PlayTap();
            SetToast("Product Id برای این آیتم تنظیم نشده.", 1.8f, false);
            return;
        }

#if !UNITY_ANDROID || UNITY_EDITOR
        AudioManager.Instance?.PlayTap();
        SetToast("پرداخت بازار فقط روی دستگاه اندرویدی (نسخهٔ Release) قابل انجام است.", 2.2f, false);
        return;
#else
        try
        {
            UIManager.Instance?.ShowToast("در حال اتصال به بازار...", 1.0f);
            var ok = await PurchaseManager.Instance.InitAsync();
            if (!ok)
            {
                AudioManager.Instance?.PlayTap();
                SetToast("اتصال به بازار ناموفق بود.", 1.8f, false);
                return;
            }

            UIManager.Instance?.ShowToast("در حال شروع خرید...", 0.9f);
            var result = await PurchaseManager.Instance.Purchase(currentItem.productId);

            if (result.status == Status.Success)
            {
                // آیتم‌ها غیرمصرفی هستند → Consume نمی‌کنیم
                storeManager.MarkPurchased(currentItem.itemID);
                StoreUIController.Instance?.RefreshCard();
                AudioManager.Instance?.PlayCoin();

                SetToast(currentItem.itemID.StartsWith("Ball_") ? "توپ جدید مبارک!" : "به زمین بازی جدید خوش اومدی!", 1.6f, true);
                Close();
            }
            else
            {
                AudioManager.Instance?.PlayTap();
                SetToast("خرید ناموفق بود.", 1.6f, false);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[PurchasePanel] Exception: " + ex);
            AudioManager.Instance?.PlayTap();
            SetToast("خطا هنگام خرید. دوباره تلاش کن.", 1.8f, false);
        }
#endif
    }

    void HandlePurchasedAndClose(string itemId)
    {
        if (currentItem != null && itemId == currentItem.itemID)
        {
            Close();
            StoreUIController.Instance?.RefreshCard();
        }
    }

    void OnDestroy() => UnsubscribeFromStore();
    void UnsubscribeFromStore()
    {
        if (subscribedToStore && storeManager != null)
            storeManager.OnPurchased -= HandlePurchasedAndClose;
        subscribedToStore = false;
    }
}
