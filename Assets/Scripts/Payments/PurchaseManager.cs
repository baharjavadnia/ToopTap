using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// Poolakey (Bazaar)
using Bazaar.Poolakey;
using Bazaar.Poolakey.Data;   // Result<T>, PurchaseInfo, SKUDetails
using Bazaar.Data;            // Status

[DefaultExecutionOrder(-500)]
[DisallowMultipleComponent]
public class PurchaseManager : MonoBehaviour
{
    public static PurchaseManager Instance { get; private set; }

    // --- RSA: SecurityCheck.Enable(KEY)
    // کلید شما: مستقیماً در کد (طبق خواستهٔ شما)
    private const string RSA_PUBLIC_KEY =
        "MIHNMA0GCSqGSIb3DQEBAQUAA4G7ADCBtwKBrwDb8GK+svR4p+rMLAWpTpePNAXoRcOnObsppsglG96yNPNOvPRwBraIHPRcX/KfWPKMOQDA1G0HcOVjEoi7FAN8i7YnOoIqMZeNHqeAyZrwwnuEW0IbHxYzUxX8lErhOlQ1j2DO+Vza9BqZaGPcaKgzTAai3BS0obDOo0o3VRCRBMzIMTrrW79jwkOs3M0YZ+CsEYOFYawmvsrfOmV5O2gGSQmuy+JHPd6j8TU1pYMCAwEAAQ==";

    private Payment _payment;
    public bool IsConnected { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

#if UNITY_ANDROID
    // ساخت Payment و اتصال (فقط Async)
    public async Task<bool> InitAsync()
    {
        if (IsConnected && _payment != null) return true;

        var security = string.IsNullOrEmpty(RSA_PUBLIC_KEY)
            ? SecurityCheck.Disable()
            : SecurityCheck.Enable(RSA_PUBLIC_KEY);

        var config = new PaymentConfiguration(security);
        _payment   = new Payment(config);

        var result = await _payment.Connect();
        LogResult("Connect", result.status, result.message);
        IsConnected = (result.status == Status.Success);
        return IsConnected;
    }

    async Task<bool> EnsureConnected()
    {
        if (IsConnected && _payment != null) return true;
        return await InitAsync();
    }

    public async Task<Result<List<SKUDetails>>> GetSkuDetails(string productId)
    {
        if (!await EnsureConnected()) return Fail<List<SKUDetails>>("Poolakey not connected");
        var res = await _payment.GetSkuDetails(productId);
        LogResult("GetSkuDetails", res.status, res.message);
        return res;
    }

    public async Task<Result<List<PurchaseInfo>>> GetPurchases(SKUDetails.Type type = SKUDetails.Type.all)
    {
        if (!await EnsureConnected()) return Fail<List<PurchaseInfo>>("Poolakey not connected");
        var res = await _payment.GetPurchases(type);
        LogResult("GetPurchases", res.status, res.message);
        return res;
    }

    // فقط خرید (non-consumable) — Consume استفاده نمی‌کنیم
    public async Task<Result<PurchaseInfo>> Purchase(string productId)
    {
        if (!await EnsureConnected()) return Fail<PurchaseInfo>("Poolakey not connected");
        var res = await _payment.Purchase(productId);
        LogResult("Purchase", res.status, res.message);
        return res;
    }
#else
    public Task<bool> InitAsync() => Task.FromResult(false);
    public Task<Result<List<SKUDetails>>> GetSkuDetails(string productId) => Task.FromResult(Fail<List<SKUDetails>>("Platform not supported"));
    public Task<Result<List<PurchaseInfo>>> GetPurchases(SKUDetails.Type type = SKUDetails.Type.all) => Task.FromResult(Fail<List<PurchaseInfo>>("Platform not supported"));
    public Task<Result<PurchaseInfo>> Purchase(string productId) => Task.FromResult(Fail<PurchaseInfo>("Platform not supported"));
#endif

    void OnApplicationQuit()
    {
#if UNITY_ANDROID
        try { _payment?.Disconnect(); } catch { }
#endif
    }

    static Result<T> Fail<T>(string msg) => new Result<T>(Status.Failure, msg);
    void LogResult(string api, Status s, string m) =>
        (s == Status.Success ? Debug.Log : (System.Action<string>)Debug.LogWarning)($"[PurchaseManager] {api} => {s} | {m}");
}
