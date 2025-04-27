using UnityEngine;
using System.Collections;

public class IvyNode : MonoBehaviour {
    [Header("Prefabs & Growth")]
    public GameObject branchPrefab;      // Branch segment prefab
    public GameObject leafPrefab;        // Leaf prefab with IvySway
    public float growthInterval = 0.5f;
    public float segmentLength = 0.5f;

    [Header("Density Controls")]
    [Tooltip("Base chance to spawn side branches (reduced by depth)")]
    public float branchChance = 0.3f;
    [Tooltip("Decay factor for branchChance per generation")]
    public float branchDecay = 0.8f;
    [Tooltip("Maximum side branches per growth")]
    public int maxBranchSpawns = 1;

    [Header("Leaf Controls")]
    [Tooltip("Chance per branch to spawn leaves")]
    public float leafSpawnChance = 0.5f;
    [Tooltip("Maximum leaves per branch")]
    public int maxLeavesPerBranch = 2;

    [Header("Generation & Health")]
    public int maxDepth = 8;
    public AnimationCurve healthCurve;
    [Tooltip("Maximum active segments to allow")]
    public int maxActiveSegments = 200;

    [HideInInspector] public int depth = 0;
    private Coroutine growthCoroutine;
    private static int activeSegmentCount = 0;

    void Start() {
        growthCoroutine = StartCoroutine(GrowRoutine());
    }

    IEnumerator GrowRoutine() {
        while(depth < maxDepth) {
            // Stop if at center
            if (GrowthManager.Instance != null && 
                Vector2.Distance(transform.position, GrowthManager.Instance.centerPoint) <= GrowthManager.Instance.centerRadius)
                yield break;
                
            SpawnBranch(Vector3.up);
            // Use GrowthController if available, otherwise use base interval
            float interval = GrowthController.Instance != null ? 
                GrowthController.Instance.GetInterval(growthInterval) : 
                growthInterval;
            yield return new WaitForSeconds(interval);
        }
    }

    void SpawnBranch(Vector3 localDir) {
        // Check if we've hit the maximum number of active segments
        if (activeSegmentCount >= maxActiveSegments) return;

        // Calculate direction
        Vector3 direction = transform.rotation * localDir;
        Vector3 pos = transform.position + direction * segmentLength;
        Quaternion rot = Quaternion.LookRotation(Vector3.forward, direction);

        // Get branch from pool if available
        GameObject branch;
        if (PoolManager.Instance != null) {
            branch = PoolManager.Instance.GetBranch();
            branch.transform.position = pos;
            branch.transform.rotation = rot;
            branch.transform.parent = transform.parent;
        } else {
            branch = Instantiate(branchPrefab, pos, rot, transform.parent);
        }

        // Track active segments
        activeSegmentCount++;

        // Configure node component
        var node = branch.GetComponent<IvyNode>();
        if (node == null) {
            node = branch.AddComponent<IvyNode>();
            node.CopySettings(this);
        }
        node.depth = depth + 1;

        // Initialize branch health
        var seg = branch.GetComponent<IvySegment>();
        if (seg != null) {
            seg.InitHealth(healthCurve.Evaluate(node.depth));
            seg.onDestroy = OnSegmentDestroyed;
            
            // Add to spatial partition
            if (SpatialPartitionManager.Instance != null) {
                SpatialPartitionManager.Instance.Insert(seg);
            }
        }

        // Spawn leaves conditionally
        if (Random.value < leafSpawnChance) {
            int leafCount = Random.Range(1, maxLeavesPerBranch + 1);
            for (int i = 0; i < leafCount; i++) {
                float angleOff = Random.Range(-30f, 30f);
                Vector3 leafDir = Quaternion.Euler(0, 0, angleOff) * direction;
                Vector3 leafPos = pos + leafDir * (segmentLength * 0.5f);
                Quaternion leafRot = Quaternion.LookRotation(Vector3.forward, leafDir);
                
                // Get leaf from pool if available
                if (PoolManager.Instance != null) {
                    GameObject leaf = PoolManager.Instance.GetLeaf();
                    leaf.transform.position = leafPos;
                    leaf.transform.rotation = leafRot;
                    leaf.transform.parent = branch.transform;
                } else {
                    Instantiate(leafPrefab, leafPos, leafRot, branch.transform);
                }
            }
        }

        // Side branches with density limits
        float effectiveChance = branchChance * Mathf.Pow(branchDecay, depth);
        int spawned = 0;
        for (int i = 0; i < maxBranchSpawns && spawned < maxBranchSpawns; i++) {
            if (Random.value < effectiveChance) {
                float angle = Random.Range(20f, 45f) * (Random.value < 0.5f ? 1 : -1);
                SpawnBranch(Quaternion.Euler(0, 0, angle) * Vector3.up);
                spawned++;
            }
        }
    }

    void OnSegmentDestroyed(GameObject obj) {
        activeSegmentCount--;
        
        // Remove from spatial partition
        var seg = obj.GetComponent<IvySegment>();
        if (seg != null && SpatialPartitionManager.Instance != null) {
            SpatialPartitionManager.Instance.Remove(seg);
        }
        
        // Return to pool if available
        if (PoolManager.Instance != null) {
            // Process child leaves first
            foreach (Transform child in obj.transform) {
                if (child.gameObject.GetComponent<IvySway>() != null) {
                    PoolManager.Instance.ReleaseLeaf(child.gameObject);
                }
            }
            PoolManager.Instance.ReleaseBranch(obj);
        }
    }

    void CopySettings(IvyNode other) {
        branchPrefab       = other.branchPrefab;
        leafPrefab         = other.leafPrefab;
        growthInterval     = other.growthInterval;
        segmentLength      = other.segmentLength;
        branchChance       = other.branchChance;
        branchDecay        = other.branchDecay;
        maxBranchSpawns    = other.maxBranchSpawns;
        leafSpawnChance    = other.leafSpawnChance;
        maxLeavesPerBranch = other.maxLeavesPerBranch;
        maxDepth           = other.maxDepth;
        healthCurve        = other.healthCurve;
        maxActiveSegments  = other.maxActiveSegments;
    }

    void OnDestroy() {
        if (growthCoroutine != null) {
            StopCoroutine(growthCoroutine);
        }
    }
} 