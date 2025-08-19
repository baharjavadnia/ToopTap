using UnityEngine.UI;
using RTLTMPro;

public static class RTLUI
{
    public static void Set(RTLTextMeshPro t, string value)
    {
        if (!t) return;
        t.text = value ?? string.Empty;
        t.ForceFix = true;                           // اعمال RTL fix
        t.Rebuild(CanvasUpdate.PreRender);           // رفرش حتمی
    }
}
