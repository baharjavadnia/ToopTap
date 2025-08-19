using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BallController : MonoBehaviour
{
    [Header("Launch")]
    public float launchForceMultiplier = 8f;
    public float minLaunchVyToScore = 2f;
    public float maxLaunchSpeed = 15f;
    public float maxDragDuration = 0.25f;

    [Header("Mask")]
    public LayerMask ballMask = ~0; // همه لایه‌ها

    Rigidbody2D rb;
    Camera cam;
    bool isDragging, pointerStartedOnBall, scoredThisTouch;
    Vector2 dragStartWorld;
    float dragStartTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        rb.isKinematic = false;
        rb.simulated = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    Vector2 W(Vector2 s) => cam ? (Vector2)cam.ScreenToWorldPoint(s) : s;

    bool PointerOverBall(Vector2 worldPos)
    {
        var hit = Physics2D.OverlapPoint(worldPos, ballMask);
        return hit && hit.transform == transform;
    }

    void Update()
    {
        if (!GameManager.Instance || !GameManager.Instance.IsRunActive) return;

#if UNITY_EDITOR
        bool down = Input.GetMouseButtonDown(0);
        bool up   = Input.GetMouseButtonUp(0);
        Vector2 pos = Input.mousePosition;
#else
        bool down=false, up=false; Vector2 pos=Vector2.zero;
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            pos = t.position;
            down = t.phase == TouchPhase.Began;
            up   = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
        }
#endif
        var w = W(pos);

        if (down)
        {
            pointerStartedOnBall = PointerOverBall(w);
            if (pointerStartedOnBall)
            {
                isDragging = true;
                dragStartWorld = w;
                dragStartTime = Time.time;
                scoredThisTouch = false;
            }
        }

        if (up && isDragging)
        {
            var drag = w - dragStartWorld;
            var dur  = Time.time - dragStartTime;

            if (dur <= maxDragDuration && drag.y > 0f)
            {
                var v = drag * launchForceMultiplier / Time.fixedDeltaTime;
                v = Vector2.ClampMagnitude(v, maxLaunchSpeed);
                rb.velocity = v;

                if (!scoredThisTouch && rb.velocity.y >= minLaunchVyToScore)
                {
                    GameManager.Instance.AddScore(1);
                    scoredThisTouch = true;
                }
            }

            isDragging = false;
            pointerStartedOnBall = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!GameManager.Instance || !GameManager.Instance.IsRunActive) return;

        if (other.gameObject.name == "BottomWall")
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
            GameManager.Instance.LoseLife();
        }
    }
}
