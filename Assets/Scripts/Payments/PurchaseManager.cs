// Assets/Scripts/Payments/PurchaseManager.cs
using System.Threading.Tasks;
using UnityEngine;

// Poolakey
using Bazaar.Poolakey;
using Bazaar.Poolakey.Data;
using Bazaar.Data;

[DefaultExecutionOrder(-500)]
[DisallowMultipleComponent]
public class PurchaseManager : MonoBehaviour
{
    public static PurchaseManager Instance { get; private set; }

    [Header("Bazaar (Poolakey)")]
    [Tooltip("RSA Public Key برنامه از پنل کافه‌بازار. خالی هم باشد کار می‌کند، ولی توصیه RSA است.")]
    [SerializeField] private string appKey = "";

    private Payment _payment;
    public bool IsReady { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task<bool> InitAsync()
    {
        if (IsReady && _payment != null) return true;

        // اگر RSA نداری، Disable؛ اگر داری Enable
        var security = string.IsNullOrEmpty(appKey) ? SecurityCheck.Disable() : SecurityCheck.Enable(appKey);
        var config   = new PaymentConfiguration(security);

        _payment = new Payment(config);

        var result = await _payment.Connect();
        IsReady    = (result.status == Status.Success);
        if (!IsReady) Debug.LogWarning($"[PurchaseManager] Connect failed: {result.status} - {result.message}");
        return IsReady;
    }

    public async Task<Result<PurchaseInfo>> Purchase(string productId)
    {
        if (!IsReady && !await InitAsync())
            return new Result<PurchaseInfo>(Status.Failure, "Poolakey connect failed");

        return await _payment.Purchase(productId);
    }

    public async Task<Result<bool>> Consume(string purchaseToken)
    {
        if (!IsReady && !await InitAsync())
            return new Result<bool>(Status.Failure, "Poolakey connect failed");

        return await _payment.Consume(purchaseToken);
    }

    public async Task<Result<System.Collections.Generic.List<PurchaseInfo>>> GetPurchases(SKUDetails.Type type = SKUDetails.Type.all)
    {
        if (!IsReady && !await InitAsync())
            return new Result<System.Collections.Generic.List<PurchaseInfo>>(Status.Failure, "Poolakey connect failed");

        return await _payment.GetPurchases(type);
    }

    private void OnApplicationQuit()
    {
        try { _payment?.Disconnect(); } catch { }
    }
}
