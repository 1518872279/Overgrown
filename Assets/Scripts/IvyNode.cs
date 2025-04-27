using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IvyNode : MonoBehaviour {
    [Header("Prefabs & Growth")]
    public GameObject branchPrefab;      // Branch segment prefab
    public GameObject leafPrefab;        // Leaf prefab with IvySway
    public float growthInterval = 0.5f;
    public float segmentLength = 0.5f;
    [Tooltip("Min random growth rate multiplier (1.0 = normal)")]
    public float minGrowthRate = 0.6f;
    [Tooltip("Max random growth rate multiplier (1.0 = normal)")]
    public float maxGrowthRate = 1.4f;
    [HideInInspector] 
    public float growthRateMultiplier = 1.0f;

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
    
    [Header("Performance Settings")]
    [Tooltip("Stop growth when total branches exceeds this number")]
    public int maxTotalBranches = 300;
    [Tooltip("Distance at which to pause growth (from camera or player)")]
    public float growthPauseDistance = 20f;
    [Tooltip("Use object pooling for branches and leaves")]
    public bool useObjectPooling = true;

    [HideInInspector] public int depth = 0;
    private Coroutine growthCoroutine;
    private bool isGrowing = true;
    
    // Static tracking of total instances to limit growth
    private static int totalBranchCount = 0;
    private static List<IvyNode> allNodes = new List<IvyNode>();
    
    // Object pooling
    private static Dictionary<int, Queue<GameObject>> branchPool = new Dictionary<int, Queue<GameObject>>();
    private static Dictionary<int, Queue<GameObject>> leafPool = new Dictionary<int, Queue<GameObject>>();
    
    void Awake() {
        allNodes.Add(this);
        totalBranchCount++;
        
        // Set random growth rate for root nodes
        if (depth == 0) {
            growthRateMultiplier = Random.Range(minGrowthRate, maxGrowthRate);
        }
    }

    void Start() {
        // Initialize object pools if needed
        if (useObjectPooling && depth == 0) {
            InitObjectPools();
        }
        
        growthCoroutine = StartCoroutine(GrowRoutine());
    }
    
    void OnEnable() {
        // Reset static counters on first ivy root
        if (depth == 0) {
            totalBranchCount = 0;
        }
    }
    
    void OnDestroy() {
        if (growthCoroutine != null) {
            StopCoroutine(growthCoroutine);
        }
        
        allNodes.Remove(this);
        totalBranchCount--;
    }
    
    private void InitObjectPools() {
        if (!branchPool.ContainsKey(branchPrefab.GetInstanceID())) {
            branchPool[branchPrefab.GetInstanceID()] = new Queue<GameObject>();
        }
        
        if (!leafPool.ContainsKey(leafPrefab.GetInstanceID())) {
            leafPool[leafPrefab.GetInstanceID()] = new Queue<GameObject>();
        }
    }

    IEnumerator GrowRoutine() {
        while (isGrowing) {
            // Calculate growth interval using both controller and individual rate
            float baseInterval = GrowthController.Instance != null ? 
                GrowthController.Instance.GetInterval(growthInterval) : 
                growthInterval;
                
            // Apply individual growth rate multiplier
            float actualInterval = baseInterval / growthRateMultiplier;
                
            yield return new WaitForSeconds(actualInterval);
            
            // Check if we should grow
            if (ShouldGrow()) {
                SpawnBranch(Vector3.up);
            }
            
            // Add dynamic pause - stop checking every frame when too many branches exist
            if (totalBranchCount > maxTotalBranches * 0.8f) {
                yield return new WaitForSeconds(actualInterval * 3f); // Slow down growth checks
            }
        }
    }
    
    private bool ShouldGrow() {
        // Basic depth check
        if (depth >= maxDepth) return false;
        
        // Check total branch limit
        if (totalBranchCount >= maxTotalBranches) return false;
        
        // Distance-based growth pause (from camera or player)
        if (Camera.main != null && Vector3.Distance(transform.position, Camera.main.transform.position) > growthPauseDistance) {
            return false;
        }
        
        return true;
    }
    
    // Pause/resume growth system-wide
    public static void PauseAllGrowth() {
        foreach (var node in allNodes) {
            node.isGrowing = false;
        }
    }
    
    public static void ResumeAllGrowth() {
        foreach (var node in allNodes) {
            node.isGrowing = true;
        }
    }

    void SpawnBranch(Vector3 localDir) {
        // Calculate direction
        Vector3 direction = transform.rotation * localDir;
        Vector3 pos = transform.position + direction * segmentLength;
        Quaternion rot = Quaternion.LookRotation(Vector3.forward, direction);

        // Instantiate branch (using pool if enabled)
        GameObject branch;
        if (useObjectPooling) {
            branch = GetFromPool(branchPrefab, pos, rot, transform.parent);
        } else {
            branch = Instantiate(branchPrefab, pos, rot, transform.parent);
        }
        
        var node = branch.GetComponent<IvyNode>();
        if (node == null) {
            node = branch.AddComponent<IvyNode>();
        }
        
        node.CopySettings(this);
        node.depth = depth + 1;

        // Initialize branch health
        var seg = branch.GetComponent<IvySegment>();
        if (seg != null) {
            seg.InitHealth(healthCurve.Evaluate(node.depth / (float)maxDepth));
        }

        // Spawn leaves conditionally
        if (Random.value < leafSpawnChance) {
            int leafCount = Mathf.Min(Random.Range(1, maxLeavesPerBranch + 1), 
                maxLeavesPerBranch);
            
            for (int i = 0; i < leafCount; i++) {
                float angleOff = Random.Range(-30f, 30f);
                Vector3 leafDir = Quaternion.Euler(0, 0, angleOff) * direction;
                Vector3 leafPos = pos + leafDir * (segmentLength * 0.5f);
                
                GameObject leaf;
                if (useObjectPooling) {
                    leaf = GetFromPool(leafPrefab, leafPos, 
                        Quaternion.LookRotation(Vector3.forward, leafDir), branch.transform);
                } else {
                    leaf = Instantiate(leafPrefab, leafPos, 
                        Quaternion.LookRotation(Vector3.forward, leafDir), branch.transform);
                }
                
                // Get or add IvySway component
                var sway = leaf.GetComponent<IvySway>();
                if (sway == null) {
                    leaf.AddComponent<IvySway>();
                }
            }
        }

        // Side branches with density and performance limits
        float effectiveChance = branchChance * Mathf.Pow(branchDecay, depth);
        
        // Reduce side branching as we approach the total branch limit
        if (totalBranchCount > maxTotalBranches * 0.7f) {
            effectiveChance *= 0.5f;
        }
        
        int spawned = 0;
        for (int i = 0; i < maxBranchSpawns && spawned < maxBranchSpawns; i++) {
            if (Random.value < effectiveChance) {
                float angle = Random.Range(20f, 45f) * (Random.value < 0.5f ? 1 : -1);
                SpawnBranch(Quaternion.Euler(0, 0, angle) * Vector3.up);
                spawned++;
            }
        }
    }
    
    private GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent) {
        Queue<GameObject> pool = prefab == branchPrefab ? 
            branchPool[prefab.GetInstanceID()] : leafPool[prefab.GetInstanceID()];
            
        GameObject obj;
        if (pool.Count > 0) {
            obj = pool.Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.parent = parent;
            obj.SetActive(true);
        } else {
            obj = Instantiate(prefab, position, rotation, parent);
        }
        
        return obj;
    }
    
    public static void ReturnToPool(GameObject obj, bool isBranch) {
        obj.SetActive(false);
        obj.transform.parent = null;
        
        if (isBranch && branchPool.Count > 0) {
            var firstKey = new List<int>(branchPool.Keys)[0]; // Simplification - assumes one prefab type
            branchPool[firstKey].Enqueue(obj);
        } else if (!isBranch && leafPool.Count > 0) {
            var firstKey = new List<int>(leafPool.Keys)[0]; // Simplification - assumes one prefab type
            leafPool[firstKey].Enqueue(obj);
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
        maxTotalBranches   = other.maxTotalBranches;
        growthPauseDistance = other.growthPauseDistance;
        useObjectPooling   = other.useObjectPooling;
        minGrowthRate      = other.minGrowthRate;
        maxGrowthRate      = other.maxGrowthRate;
        
        // When copying to child branches, randomly vary the growth rate slightly
        // but stay within parent's range and keep somewhat consistent
        if (other.growthRateMultiplier != 1.0f) {
            // Child branches inherit parent's growth rate with slight variation
            float variation = Random.Range(0.9f, 1.1f);
            growthRateMultiplier = other.growthRateMultiplier * variation;
            
            // Clamp to configured min/max
            growthRateMultiplier = Mathf.Clamp(growthRateMultiplier, minGrowthRate, maxGrowthRate);
        }
    }

    public static bool IsPoolingEnabled() {
        // If we have any nodes in the list, check the first one's settings
        if (allNodes != null && allNodes.Count > 0) {
            return allNodes[0].useObjectPooling;
        }
        return false;
    }
} 