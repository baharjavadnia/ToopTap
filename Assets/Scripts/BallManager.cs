using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public static BallManager Instance;

    public const string KEY_SELECTED_BALL_ID = "SelectedBallID";

    [System.Serializable]
    public class BallData
    {
        public string id;
        public GameObject ballPrefab;
        public float gravityScale = 1f;
        public float mass = 1f;
        public PhysicsMaterial2D material;
        public Vector3 spawnPosition = new Vector3(0, 2f, 0);
        public Vector3 scale = Vector3.one * 0.3f;
    }

    [SerializeField] public List<BallData> balls;
    [SerializeField] private string defaultBallId = "Ball_Football";

    private Dictionary<string, int> idToIndex;
    private GameObject currentBall;

    void Awake()
    {
        Instance = this;
        idToIndex = new Dictionary<string, int>(balls.Count);
        for (int i = 0; i < balls.Count; i++)
            if (balls[i] != null && !string.IsNullOrEmpty(balls[i].id))
                idToIndex[balls[i].id] = i;
    }

    void Start()
    {
        var savedId = PlayerPrefs.GetString(KEY_SELECTED_BALL_ID, defaultBallId);
        if (string.IsNullOrEmpty(savedId))
        {
            PlayerPrefs.SetString(KEY_SELECTED_BALL_ID, defaultBallId);
            PlayerPrefs.Save();
        }
    }

    public void SpawnSelectedBall()
    {
        var id = PlayerPrefs.GetString(KEY_SELECTED_BALL_ID, defaultBallId);
        if (string.IsNullOrEmpty(id)) id = defaultBallId;

        if (!idToIndex.TryGetValue(id, out var idx))
        {
            id = defaultBallId;
            PlayerPrefs.SetString(KEY_SELECTED_BALL_ID, id);
            PlayerPrefs.Save();
            if (!idToIndex.TryGetValue(id, out idx)) return;
        }

        SpawnBall(idx);
    }

    public void SetBallByID(string id, bool previewOnly = false)
    {
        if (!idToIndex.TryGetValue(id, out var idx))
        {
            id = defaultBallId;
            idToIndex.TryGetValue(id, out idx);
        }

        PlayerPrefs.SetString(KEY_SELECTED_BALL_ID, id);
        PlayerPrefs.Save();

        if (!previewOnly) SpawnBall(idx);
    }

    private void SpawnBall(int index)
    {
        if (index < 0 || index >= balls.Count || balls[index] == null)
        {
            if (!idToIndex.TryGetValue(defaultBallId, out index)) return;
        }

        if (currentBall) Destroy(currentBall);

        var data = balls[index];
        currentBall = Instantiate(data.ballPrefab, data.spawnPosition, Quaternion.identity);

        var rb = currentBall.GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.gravityScale = data.gravityScale;
            rb.mass = data.mass;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        var col = currentBall.GetComponent<CircleCollider2D>();
        if (col && data.material) col.sharedMaterial = data.material;

        currentBall.transform.localScale = data.scale;
        currentBall.tag = "Ball";

        var sr = currentBall.GetComponent<SpriteRenderer>();
        if (sr) { sr.sortingLayerName = "Default"; sr.sortingOrder = 10; }

        var p = currentBall.transform.position;
        currentBall.transform.position = new Vector3(p.x, p.y, 0f);
    }

    public GameObject GetCurrentBall() => currentBall;
}
