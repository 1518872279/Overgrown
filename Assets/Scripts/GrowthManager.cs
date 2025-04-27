using UnityEngine;

public class GrowthManager : MonoBehaviour {
    [Tooltip("Prefab with IvyNode attached")]
    public GameObject ivyRootPrefab;
    public float spawnRadius = 10f;
    public int rootCount = 8;

    void Start() {
        Vector3 center = transform.position;
        for (int i = 0; i < rootCount; i++) {
            float angle = i * (360f / rootCount);
            Vector3 pos = center + Quaternion.Euler(0, 0, angle) * Vector3.up * spawnRadius;
            GameObject root = Instantiate(ivyRootPrefab, pos, Quaternion.identity);
            // Orient to grow toward center
            root.transform.up = (center - pos).normalized;
        }
    }
} 