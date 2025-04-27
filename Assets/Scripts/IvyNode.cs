using UnityEngine;

public class IvyNode : MonoBehaviour {
    public GameObject segmentPrefab;
    public float growthInterval = 0.3f;
    public float segmentLength = 0.5f;
    public float branchChance = 0.25f;
    public int maxDepth = 5;

    [HideInInspector] public int depth = 0;

    void Start() {
        Invoke(nameof(Grow), growthInterval);
    }

    void Grow() {
        if (depth >= maxDepth) return;

        // Spawn forward segment
        Vector3 forward = transform.up * segmentLength;
        var seg = Instantiate(segmentPrefab, transform.position + forward, transform.rotation, transform.parent);
        var node = seg.AddComponent<IvyNode>();
        node.depth = depth + 1;
        node.CopySettings(this);

        // Random branching
        if (Random.value < branchChance) {
            SpawnBranch( Random.Range(20f, 45f) );
            SpawnBranch( -Random.Range(20f, 45f) );
        }
    }

    void SpawnBranch(float angle) {
        Quaternion rot = transform.rotation * Quaternion.Euler(0, 0, angle);
        Vector3 dir = rot * Vector3.up * segmentLength;
        var seg = Instantiate(segmentPrefab, transform.position + dir, rot, transform.parent);
        var node = seg.AddComponent<IvyNode>();
        node.depth = depth + 1;
        node.CopySettings(this);
    }

    public void CopySettings(IvyNode other) {
        segmentPrefab   = other.segmentPrefab;
        growthInterval  = other.growthInterval;
        segmentLength   = other.segmentLength;
        branchChance    = other.branchChance;
        maxDepth        = other.maxDepth;
    }
} 