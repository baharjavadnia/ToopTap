using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// Poolakey (Bazaar)
using Bazaar.Poolakey;
using Bazaar.Poolakey.Data;   // Result<T>, PurchaseInfo, SKUDetails
using Bazaar.Data;            // Status

/// <summary>
/// Singleton bridge for Poolakey. Async-only (Connect / GetPurchases / Purchase)
/// Non-consumables only (no Consume).
/// </summary>
[DefaultExecutionOrder(-500)]
[DisallowMultipleComponent]
public class PurchaseManager : MonoBehaviour
{
    public static PurchaseManager Instance { get; private set; }

    // --- RSA (inside code)
    private const string RSA_PUBLIC_KEY =
        "MIHNMA0GCSqGSIb3DQEBAQUAA4G7ADCBtwKBrwDb8GK+svR4p+rMLAWpTpePNAXoRcOnObsppsglG96yNPNOvPRwBraIHPRcX/KfWPKMOQDA1G0HcOVjEoi7FAN8i7YnOoIqMZeNHqeAyZrwwnuEW0IbHxYzUxX8lErhOlQ1j2DO+Vza9BqZaGPcaKgzTAai3BS0obDOo0o3VRCRBMzIMTrrW79jwkOs3M0YZ+CsEYOFYawmvsrfOmV5O2gGSQmuy+JHPd6j8TU1pYMCAwEAAQ==";

#if UNITY_ANDROID
    private Payment _payment;
    public bool IsConnected { get; private set; }
#endif

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

#if UNITY_ANDROID
    /// <summary>
    /// Create Payment and connect (async).
    /// </summary>
    public async Task<bool> InitAsync()
    {
        if (IsConnected && _payment != null) return true;

        var security = string.IsNullOrEmpty(RSA_PUBLIC_KEY)
            ? SecurityCheck.Disable()
            : SecurityCheck.Enable(RSA_PUBLIC_KEY);

        var config = new PaymentConfiguration(security);
        _payment = new Payment(config);

        var result = await _payment.Connect();
        Log("Connect", result.status, result.message);

        IsConnected = (result.status == Status.Success);
        return IsConnected;
    }

    private async Task<bool> EnsureConnected()
    {
        if (IsConnected && _payment != null) return true;
        return await InitAsync();
    }

    public async Task<Result<List<SKUDetails>>> GetSkuDetails(string productId)
    {
        if (!await EnsureConnected()) return Fail<List<SKUDetails>>("Poolakey not connected");
        var res = await _payment.GetSkuDetails(productId);
        Log("GetSkuDetails", res.status, res.message);
        return res;
    }

    public async Task<Result<List<PurchaseInfo>>> GetPurchases(SKUDetails.Type type = SKUDetails.Type.all)
    {
        if (!await EnsureConnected()) return Fail<List<PurchaseInfo>>("Poolakey not connected");
        var res = await _payment.GetPurchases(type);
        Log("GetPurchases", res.status, res.message);
        return res;
    }

    /// <summary>
    /// Non-consumable purchase (no Consume).
    /// </summary>
    public async Task<Result<PurchaseInfo>> Purchase(string productId)
    {
        if (!await EnsureConnected()) return Fail<PurchaseInfo>("Poolakey not connected");
        var res = await _payment.Purchase(productId);
        Log("Purchase", res.status, res.message);
        return res;
    }

    void OnApplicationQuit()
    {
        try { _payment?.Disconnect(); } catch { /* ignore */ }
    }
#else
    // Safe stubs for non-Android/Editor
    public Task<bool> InitAsync() => Task.FromResult(false);
    public Task<Result<List<SKUDetails>>> GetSkuDetails(string productId)
        => Task.FromResult(Fail<List<SKUDetails>>("Platform not supported"));
    public Task<Result<List<PurchaseInfo>>> GetPurchases(SKUDetails.Type type = SKUDetails.Type.all)
        => Task.FromResult(Fail<List<PurchaseInfo>>("Platform not supported"));
    public Task<Result<PurchaseInfo>> Purchase(string productId)
        => Task.FromResult(Fail<PurchaseInfo>("Platform not supported"));
#endif

    // ---- Helpers
    static Result<T> Fail<T>(string msg) => new Result<T>(Status.Failure, msg);

    static void Log(string api, Status s, string m)
    {
        var line = $"[PurchaseManager] {api} => {s} | {m}";
        if (s == Status.Success) Debug.Log(line); else Debug.LogWarning(line);
    }
}
