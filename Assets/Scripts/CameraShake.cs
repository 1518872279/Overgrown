using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour {
    public static CameraShake Instance;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Shake(float duration, float magnitude) {
        StartCoroutine(DoShake(duration, magnitude));
    }

    IEnumerator DoShake(float dur, float mag) {
        Vector3 orig = transform.localPosition;
        float t = 0f;
        while (t < dur) {
            transform.localPosition = orig + (Vector3)Random.insideUnitCircle * mag;
            t += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = orig;
    }
} 