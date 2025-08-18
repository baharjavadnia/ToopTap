using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    [Header("Camera Padding")]
    public float padding = 0.6f;

    [Header("Prefabs")]
    public GameObject coinPrefab;
    public GameObject potionPrefab;

    [Header("Timing")]
    public float firstDelay = 1.5f;
    public Vector2 nextDelayRange = new Vector2(4f, 7f);

    GameObject currentPickup;
    Coroutine loopCo;
    bool isSpawning = false;

    public void StartSpawning()
    {
        if (isSpawning) return;
        isSpawning = true;
        loopCo = StartCoroutine(CoLoop());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        if (loopCo != null) StopCoroutine(loopCo);
        loopCo = null;
        currentPickup = null;
    }

    IEnumerator CoLoop()
    {
        yield return new WaitForSecondsRealtime(firstDelay);

        while (isSpawning)
        {
            if (currentPickup == null)
            {
                SpawnOne();
                Destroy(currentPickup, 1.5f);
                float d = Random.Range(nextDelayRange.x, nextDelayRange.y);
                yield return new WaitForSecondsRealtime(d);
            }
            else yield return null;
        }
    }

    void SpawnOne()
    {
        Vector2 pos = RandomPointInCamera(padding);
        bool coin = Random.value < 0.5f;
        GameObject pf = coin ? coinPrefab : potionPrefab;
        if (!pf) return;

        currentPickup = Instantiate(pf, pos, Quaternion.identity);
    }

    public static Vector2 RandomPointInCamera(float pad)
    {
        Camera cam = Camera.main;
        float h = cam.orthographicSize;
        float w = h * cam.aspect;
        float x = Random.Range(-w + pad, w - pad);
        float y = Random.Range(-h + pad, h - pad);
        return new Vector2(x, y);
    }
}
