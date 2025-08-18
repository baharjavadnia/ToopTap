using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance;

    [Header("Hierarchy")] public Transform backgroundParent;

    [Header("Prefabs")]
    public GameObject defaultBackgroundPrefab;
    public List<GameObject> unlockableBackgroundPrefabs;

    [Header("IDs")]
    public string defaultBackgroundId = "BG_Default";

    private GameObject currentBackground;
    public string CurrentBackgroundId { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start() => LoadSelectedBackground();

    public void LoadSelectedBackground()
    {
        if (currentBackground) Destroy(currentBackground);

        int bgIndex = PlayerPrefs.GetInt("SelectedBackgroundIndex", 0);

        GameObject prefabToSpawn =
            (bgIndex == 0)
                ? defaultBackgroundPrefab
                : unlockableBackgroundPrefabs[Mathf.Clamp(bgIndex - 1, 0, unlockableBackgroundPrefabs.Count - 1)];

        if (prefabToSpawn == null)
        {
            prefabToSpawn = defaultBackgroundPrefab;
            bgIndex = 0;
            PlayerPrefs.SetInt("SelectedBackgroundIndex", 0);
        }

        currentBackground = Instantiate(prefabToSpawn, backgroundParent);
        currentBackground.transform.localPosition = new Vector3(0f, 0f, 10f);

        CurrentBackgroundId =
            (bgIndex == 0) ? defaultBackgroundId
                           : (unlockableBackgroundPrefabs[bgIndex - 1] != null
                                ? unlockableBackgroundPrefabs[bgIndex - 1].name
                                : defaultBackgroundId);
    }

    public void SetBackground(int index)
    {
        PlayerPrefs.SetInt("SelectedBackgroundIndex", index);
        LoadSelectedBackground();
    }

    public void SetBackgroundByID(string id)
    {
        if (id == defaultBackgroundId) { SetBackground(0); return; }

        for (int i = 0; i < unlockableBackgroundPrefabs.Count; i++)
        {
            var go = unlockableBackgroundPrefabs[i];
            if (go && go.name == id) { SetBackground(i + 1); return; }
        }
    }

    public bool IsCurrent(string id) => CurrentBackgroundId == id;
}
