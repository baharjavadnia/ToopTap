using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using Bazaar.Data;
// اگر از Result<PurchaseInfo> استفاده می‌کنی: using Bazaar.Poolakey.Data;

public class PurchasePanel : MonoBehaviour
{
    [Header("Root")]
    public GameObject root;

    [Header("UI")]
    public RTLTextMeshPro purchaseTitleText;
    public Button         purchaseScoreButton;
    public RTLTextMeshPro purchaseScoreButtonText;
    public Button         purchaseMoneyButton;
    public RTLTextMeshPro purchaseMoneyButtonText;
    public Button         cancelButton;

    // متن کوتاه داخل خود پنل (اختیاری). اگر ست نکنی، به UIManager.ShowToast می‌افتیم.
    public RTLTextMeshPro toastText;

    private StoreItemData currentItem;
    private StoreManager  storeManager;
    private bool          subscribedToStore = false;

    void Awake()
    {
        if (!root) root = gameObject;
    }

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

        if (purchaseTitleText)
            RTLUI.Set(purchaseTitleText, FaTextUtility.GetDisplayName(item.itemID));

        if (purchaseScoreButtonText)
            RTLUI.Set(purchaseScoreButtonText, FaTextUtility.FormatPricePoints(item.pricePoints));

        if (purchaseMoneyButtonText)
            RTLUI.Set(purchaseMoneyButtonText, FaTextUtility.FormatPriceTomans(item.priceToman));

        SetActive(true);
        SetToast(""); // پاک
    }

    public void OnCancelClicked()
    {
        AudioManager.Instance?.PlayTap();
        Close();
    }

    public void Close()
    {
        SetActive(false);
        UnsubscribeFromStore();
    }

    private void SetActive(bool v)
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

    // success=true → سبز ، false → قرمز
    private void SetToast(string msg, float autoHideSec = 0f, bool success = false)
    {
        if (string.IsNullOrEmpty(msg))
        {
            if (toastText) toastText.gameObject.SetActive(false);
            return;
        }

        if (toastText)
        {
            toastText.color = success ? new Color(0.15f, 0.7f, 0.2f)
                                      : new Color(0.9f, 0.25f, 0.25f);
            RTLUI.Set(toastText, msg);
            toastText.gameObject.SetActive(true);
            // اگر خواستی بعد از مدتی محو شود، می‌توانی کوروتین ساده اضافه کنی.
        }
        else
        {
            UIManager.Instance?.ShowToast(msg, Mathf.Max(1.2f, autoHideSec));
        }
    }

    // خرید با امتیاز
    private void OnBuyScoreClicked()
    {
        if (currentItem == null || storeManager == null) return;

        bool ok = storeManager.BuyWithPoints(currentItem.itemID, currentItem.pricePoints);
        if (ok)
        {
            AudioManager.Instance?.PlayCoin();
            storeManager.MarkPurchased(currentItem.itemID);
            StoreUIController.Instance?.RefreshCard();

            var msg = currentItem.itemID.StartsWith("Ball_")
                ? "توپ جدید مبارک!"
                : "به زمین بازی جدید خوش اومدی!";
            SetToast(msg, 1.6f, success: true);

            Close();
        }
        else
        {
            AudioManager.Instance?.PlayTap();
            SetToast("به اندازه کافی امتیاز نداری!", 1.8f, success: false);
        }
    }

    // خرید ریالی (بازار)
    private async void OnBuyMoneyClicked()
    {
        if (currentItem == null || storeManager == null)
        {
            AudioManager.Instance?.PlayTap();
            return;
        }
        if (string.IsNullOrEmpty(currentItem.productId))
        {
            AudioManager.Instance?.PlayTap();
            SetToast("Product Id برای این آیتم تنظیم نشده.", 1.8f, success: false);
            return;
        }

        // فقط روی اندروید مجاز است
#if !UNITY_ANDROID
        AudioManager.Instance?.PlayTap();
        SetToast("خرید ریالی فقط روی نسخهٔ اندروید امکان‌پذیر است.", 2.0f, success: false);
        return;
#endif

        UIManager.Instance?.ShowToast("در حال اتصال به بازار...", 1.0f);
        var ok = await PurchaseManager.Instance.InitAsync();
        if (!ok)
        {
            AudioManager.Instance?.PlayTap();
            SetToast("اتصال به بازار ناموفق بود.", 1.8f, success: false);
            return;
        }

        UIManager.Instance?.ShowToast("در حال شروع خرید...", 0.9f);
        var result = await PurchaseManager.Instance.Purchase(currentItem.productId);

        if (result.status == Status.Success)
        {
            // اگر لازم بود برای Consumable:
            // await PurchaseManager.Instance.Consume(result.data.purchaseToken);

            storeManager.MarkPurchased(currentItem.itemID);
            StoreUIController.Instance?.RefreshCard();
            AudioManager.Instance?.PlayCoin();

            var msg = currentItem.itemID.StartsWith("Ball_")
                ? "توپ جدید مبارک!"
                : "به زمین بازی جدید خوش اومدی!";
            SetToast(msg, 1.6f, success: true);

            Close();
        }
        else
        {
            AudioManager.Instance?.PlayTap();
            SetToast("خرید ناموفق بود.", 1.6f, success: false);
        }
    }

    private void HandlePurchasedAndClose(string itemId)
    {
        if (currentItem != null && itemId == currentItem.itemID)
        {
            Close();
            StoreUIController.Instance?.RefreshCard();
        }
    }

    private void OnDestroy() => UnsubscribeFromStore();
    private void UnsubscribeFromStore()
    {
        if (subscribedToStore && storeManager != null)
            storeManager.OnPurchased -= HandlePurchasedAndClose;

        subscribedToStore = false;
    }
}
