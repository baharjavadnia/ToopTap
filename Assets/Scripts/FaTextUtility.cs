using System.Collections.Generic;
using System.Text;

public static class FaTextUtility
{
    public const string Label_Buy = "خرید";
    public const string Label_Select = "انتخاب";
    public const string Label_Selected = "انتخاب شده";
    public const string Label_Owned = "خریده شده";

    static readonly Dictionary<string, string> _nameMap = new Dictionary<string, string>
    {
        { "Ball_Football",   "توپ فوتبال" },
        { "Ball_Volleyball", "توپ والیبال" },
        { "Ball_Nostalgia",  "توپ نوستالژی" },
        { "Ball_Baseball",   "توپ بیسبال" },
        { "Ball_Beach",      "توپ ساحلی" },
        { "Ball_Golf",       "توپ گلف" },
        { "Ball_Tennis",     "توپ تنیس" },
        { "Ball_Hockey",     "توپ هاکی" },
        { "Ball_Rugby",      "توپ راگبی" },
        { "Ball_Bowling",    "توپ بولینگ" },
        { "Ball_Billiards",  "توپ بیلیارد" },
        { "Ball_Basketball", "توپ بسکتبال" },
        { "Ball_Cricket",    "توپ کریکت" },
        { "Ball_Billiard",   "توپ بیلیارد" },
        { "Ball_HockeyPuck","توپ هاکی" },

        { "BG_Default",       "استادیوم فوتبال" },
        { "BG_SunsetStadium", "غروب آفتاب" },
        { "BG_CartoonStadium","استادیوم جادویی"},
        { "BG_BeachStadium",  "زمین ساحلی" },
        { "BG_IceStadium",    "استادیوم هاکی" },
    };

    public static string GetDisplayName(string itemId)
        => _nameMap.TryGetValue(itemId, out var fa) ? fa : itemId;

    public static string GetDisplayName(string itemId, bool owned, bool selected) => GetDisplayName(itemId);
    public static string GetDisplayName(string itemId, bool isBallSection)        => GetDisplayName(itemId);
    public static string GetDisplayName(string itemId, string section)            => GetDisplayName(itemId);
    public static string GetDisplayName(string itemId, bool isBallSection, string englishName) => GetDisplayName(itemId);
    public static string GetDisplayName(string itemId, string section, bool selected)          => GetDisplayName(itemId);
    public static string GetDisplayName(string itemId, string section, string englishName)     => GetDisplayName(itemId);

    public static string ToPersianDigits(long value)
    {
        string s = value.ToString();
        var sb = new StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c >= '0' && c <= '9') sb.Append((char)('۰' + (c - '0')));
            else sb.Append(c);
        }
        return sb.ToString();
    }

    public static string FormatLives(int lives)             => ToPersianDigits(lives);
    public static string FormatScore(int score)             => ToPersianDigits(score);
    public static string FormatFinalScore(int score)        => ToPersianDigits(score);
    public static string FormatCountdown(int seconds)       => ToPersianDigits(seconds);
    public static string FormatTotalPoints(int totalPoints) => ToPersianDigits(totalPoints);

    public static string FormatPriceTomans(long amount) => $"{ToPersianDigits(amount)} تومان";
    public static string FormatPricePoints(long amount) => $"{ToPersianDigits(amount)} امتیاز";

    public static string ToPersianDigitsInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var sb = new StringBuilder(text.Length);
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c >= '0' && c <= '9') sb.Append((char)('۰' + (c - '0')));
            else sb.Append(c);
        }
        return sb.ToString();
    }

    public static string FormatFinalScoreLabeled(int score)
        => "امتیاز نهایی : " + ToPersianDigits(score);
}
