// Assets/Editor/RTLTextConverter.cs
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using TMPro;

public static class RTLTextConverter
{
    static Type FindRTLType()
    {
        // تلاش‌های رایج
        var t =
            Type.GetType("RTLTMPro.RTLTextMeshProUGUI") ??
            Type.GetType("RTLTMPro.RTLTextMeshProUGUI, RTLTMPro") ??
            Type.GetType("RTLTMPro.RTLTextMeshPro") ??
            Type.GetType("RTLTMPro.RTLTextMeshPro, RTLTMPro");

        if (t != null) return t;

        // جست‌وجوی سراسری در همه اسمبلی‌ها (ایمن اما کند؛ فقط هنگام کانورت اجرا می‌شود)
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                t = asm.GetTypes().FirstOrDefault(x =>
                    x.Name == "RTLTextMeshProUGUI" ||
                    x.FullName == "RTLTMPro.RTLTextMeshProUGUI" ||
                    x.Name == "RTLTextMeshPro" ||
                    x.FullName == "RTLTMPro.RTLTextMeshPro");
                if (t != null) return t;
            }
            catch { /* بعضی اسمبلی‌ها ReflectionOnly هستند */ }
        }

        return null;
    }

    [MenuItem("Tools/RTL TMP/Convert TextMeshProUGUI in Scene")]
    public static void ConvertAllInScene()
    {
        var rtlType = FindRTLType();
        if (rtlType == null)
        {
            EditorUtility.DisplayDialog(
                "RTLTMP پیدا نشد",
                "نوع کامپوننت RTLTMP شناسایی نشد. مطمئن شو پکیج RTLTMPro_Extended بدون ارور ایمپورت شده.",
                "باشه");
            return;
        }

        var tmps = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>(true);
        int converted = 0;

        foreach (var tmp in tmps)
        {
            // اگر قبلاً RTLTMP است یا شیء نابود شده، رد شو
            if (!tmp || tmp.GetType() == rtlType || tmp.GetComponent(rtlType)) continue;

            Undo.RecordObject(tmp.gameObject, "Convert TMP to RTLTMP");

            // مقادیر مهم برای حفظ
            var go               = tmp.gameObject;
            var text             = tmp.text;
            var font             = tmp.font;
            var fontMat          = tmp.fontSharedMaterial;
            var color            = tmp.color;
            var fontSize         = tmp.fontSize;
            var autoSize         = tmp.enableAutoSizing;
            var alignment        = tmp.alignment;
            var overflow         = tmp.overflowMode;
            var wordWrap         = tmp.enableWordWrapping;
            var raycastTarget    = tmp.raycastTarget;
            var richText         = tmp.richText;
            var lineSpacing      = tmp.lineSpacing;
            var charSpacing      = tmp.characterSpacing;
            var paraSpacing      = tmp.paragraphSpacing;
            var margin           = tmp.margin;

            // حذف TMP قدیمی و افزودن RTLTMP با Reflection
            UnityEngine.Object.DestroyImmediate(tmp, true);
            var newComp = Undo.AddComponent(go, rtlType);

            // از طریق پایه TMP_Text پراپرتی‌های مشترک را ست کنیم
            var tmpBase = newComp as TMP_Text;
            if (tmpBase != null)
            {
                tmpBase.text               = text;
                tmpBase.font               = font;
                tmpBase.fontSharedMaterial = fontMat;
                tmpBase.color              = color;
                tmpBase.fontSize           = fontSize;
                tmpBase.enableAutoSizing   = autoSize;
                tmpBase.alignment          = alignment;
                tmpBase.overflowMode       = overflow;
                tmpBase.enableWordWrapping = wordWrap;
                tmpBase.raycastTarget      = raycastTarget;
                tmpBase.richText           = richText;
                tmpBase.lineSpacing        = lineSpacing;
                tmpBase.characterSpacing   = charSpacing;
                tmpBase.paragraphSpacing   = paraSpacing;
                tmpBase.margin             = margin;
            }

            // تلاش برای روشن کردن گزینه‌های مخصوص RTLTMP (اگر وجود دارند)
            TrySetBool(newComp, "isRightToLeftText", true);
            TrySetBool(newComp, "farsi", true);
            TrySetBool(newComp, "fixTags", true);
            // اگر دوست داری اعداد فارسی نشوند:
            // TrySetBool(newComp, "preserveNumbers", true);

            converted++;
        }

        Debug.Log($"[RTLTextConverter] Converted {converted} TextMeshProUGUI component(s) to RTLTMP in this scene.");
        EditorUtility.DisplayDialog("انجام شد", $"تعداد {converted} مورد تبدیل شد.", "اوکی");
    }

    static void TrySetBool(Component c, string propName, bool value)
    {
        if (c == null) return;
        var p = c.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
        if (p != null && p.CanWrite && p.PropertyType == typeof(bool))
            p.SetValue(c, value);
        else
        {
            var f = c.GetType().GetField(propName, BindingFlags.Public | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(bool))
                f.SetValue(c, value);
        }
    }
}
