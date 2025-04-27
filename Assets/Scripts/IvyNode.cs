using UnityEngine;

public class IvyNode : MonoBehaviour {
    [Tooltip("Segment prefab with IvySegment & IvySway")]
    public GameObject segmentPrefab;
    public float growthInterval = 0.3f;
    public float segmentLength = 0.5f;
    public float branchChance = 0.25f;
    public int maxDepth = 5;

    [Tooltip("Curve defining health per generation (match your XP curve)")]
    public AnimationCurve healthCurve;

    [HideInInspector] public int depth = 0;

    void Start() {
        Invoke(nameof(Grow), growthInterval);
    }

    void Grow() {
        if (depth >= maxDepth) return;

        // Spawn forward segment
        SpawnSegment(transform.up);
        // Random branching
        if (Random.value < branchChance) {
            SpawnSegment(Quaternion.Euler(0, 0, Random.Range(20f,45f)) * transform.up);
            SpawnSegment(Quaternion.Euler(0, 0, -Random.Range(20f,45f)) * transform.up);
        }
    }

    void SpawnSegment(Vector3 direction) {
        Vector3 pos = transform.position + direction * segmentLength;
        Quaternion rot = Quaternion.LookRotation(Vector3.forward, direction);
        GameObject segObj = Instantiate(segmentPrefab, pos, rot, transform.parent);

        // Set generation and settings
        IvyNode child = segObj.AddComponent<IvyNode>();
        child.depth = depth + 1;
        child.segmentPrefab = segmentPrefab;
        child.growthInterval = growthInterval;
        child.segmentLength = segmentLength;
        child.branchChance = branchChance;
        child.maxDepth = maxDepth;
        child.healthCurve = healthCurve;

        // Scale health based on curve
        IvySegment seg = segObj.GetComponent<IvySegment>();
        float health = healthCurve.Evaluate(child.depth);
        seg.InitHealth(health);
    }
} 