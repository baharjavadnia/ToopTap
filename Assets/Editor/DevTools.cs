#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class DevTools
{
    [MenuItem("Tools/Game/Clear PlayerPrefs (Factory Reset)")]
    public static void ClearAllPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[DevTools] PlayerPrefs cleared. Run the scene once so defaults are re-created.");
    }
}
#endif
