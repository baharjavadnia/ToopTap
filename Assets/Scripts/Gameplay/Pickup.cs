using UnityEngine;

public enum PickupType { Coin, RandomPotion }

public class Pickup : MonoBehaviour
{
    [Header("Type")] public PickupType type = PickupType.Coin;

    [Header("Coin")] public int coinScore = 10;

    [Header("Potion Durations")] public float slowMotionDuration = 3f;

    void Start() { Destroy(gameObject, 1.2f); }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ball")) return;

        if (type == PickupType.Coin)
        {
            GameManager.Instance.AddScore(coinScore);
        }
        else
        {
            int effect = Random.Range(0, 3);
            switch (effect)
            {
                case 0: GameManager.Instance.StartSlowMotion(slowMotionDuration); break;
                case 1: GameManager.Instance.AddLife(1);                          break;
                case 2: GameManager.Instance.AddScore(20);                        break;
            }
        }

        GameManager.Instance.OnPickupCollected();
        Destroy(gameObject);
    }
}
