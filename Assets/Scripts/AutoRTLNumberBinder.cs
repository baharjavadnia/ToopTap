using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;

[DisallowMultipleComponent]
public class AutoRTLNumberBinder : MonoBehaviour
{
    public enum PersianTextKind
    {
        Raw,
        Lives,
        Score,
        FinalScore,
        Countdown,
        PriceTomans,
        PricePoints,
        DisplayName
    }

    [Header("Target")]
    public RTLTextMeshPro target;

    [Header("Mode")]
    public PersianTextKind kind = PersianTextKind.Raw;

    public void SetValue(int value)  => SetValue((long)value);

    public void SetValue(long value)
    {
        if (!target) return;

        string txt;
        switch (kind)
        {
            case PersianTextKind.Lives:       txt = FaTextUtility.FormatLives((int)value);        break;
            case PersianTextKind.Score:       txt = FaTextUtility.FormatScore((int)value);        break;
            case PersianTextKind.FinalScore:  txt = FaTextUtility.FormatFinalScore((int)value);   break;
            case PersianTextKind.Countdown:   txt = FaTextUtility.FormatCountdown((int)value);    break;
            case PersianTextKind.PriceTomans: txt = FaTextUtility.FormatPriceTomans(value);       break;
            case PersianTextKind.PricePoints: txt = FaTextUtility.FormatPricePoints(value);       break;
            default:                          txt = FaTextUtility.ToPersianDigits(value);         break;
        }

        RTLUI.Set(target, txt);
    }

    public void SetName(string itemKey)
    {
        if (!target) return;
        RTLUI.Set(target, FaTextUtility.GetDisplayName(itemKey));
    }

    public void SetName(string itemKey, string _) => SetName(itemKey);

    public void SetRaw(string anyText)
    {
        if (!target) return;
        RTLUI.Set(target, FaTextUtility.ToPersianDigitsInText(anyText));
    }

    void Reset()       { if (!target) target = GetComponent<RTLTextMeshPro>(); }
    void OnValidate()  { if (!target) target = GetComponent<RTLTextMeshPro>(); }
}
