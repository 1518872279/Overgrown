# Ivy Growth, Cutting & Rogue-Lite System for Overgrown Ivy Jam

This markdown outlines the scripts and setup you’ll need. Paste each code block into Cursor to generate the corresponding C# files in your Unity project.

---

## 1. GrowthManager.cs
**Purpose:** Spawns ivy roots around the room perimeter, aimed inward.
```csharp
using UnityEngine;

public class GrowthManager : MonoBehaviour {
    public static GrowthManager Instance;
    public GameObject ivyRootPrefab; // Prefab with IvyNode
    public float spawnRadius = 10f;
    public int rootCount = 8;
    public Vector2 centerPoint;
    public float centerRadius = 1f;

    void Awake() {
        Instance = this;
        centerPoint = transform.position;
    }

    void Start() {
        for (int i = 0; i < rootCount; i++) {
            float angle = i * (360f / rootCount) + Random.Range(0f, 360f / rootCount);
            Vector3 pos = centerPoint + Quaternion.Euler(0, 0, angle) * Vector3.up * spawnRadius;
            var root = Instantiate(ivyRootPrefab, pos, Quaternion.identity);
            root.transform.up = (centerPoint - new Vector2(pos.x, pos.y)).normalized;
        }
    }
}
```

---

## 2. SpatialPartitionManager.cs
**Purpose:** Grid-based spatial partition for efficient updates and queries.
```csharp
using UnityEngine;
using System.Collections.Generic;

public class SpatialPartitionManager : MonoBehaviour {
    public static SpatialPartitionManager Instance;
    public float cellSize = 2f;
    private Dictionary<Vector2Int, List<IvySegment>> buckets = new();

    void Awake() {
        Instance = this;
    }

    Vector2Int Hash(Vector2 pos) {
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / cellSize),
            Mathf.FloorToInt(pos.y / cellSize)
        );
    }

    public void Insert(IvySegment seg) {
        var key = Hash(seg.transform.position);
        if (!buckets.ContainsKey(key)) buckets[key] = new List<IvySegment>();
        buckets[key].Add(seg);
    }

    public void Remove(IvySegment seg) {
        var key = Hash(seg.transform.position);
        if (buckets.TryGetValue(key, out var list)) list.Remove(seg);
    }

    public IEnumerable<IvySegment> QueryArea(Rect area) {
        int minX = Mathf.FloorToInt(area.xMin / cellSize);
        int maxX = Mathf.FloorToInt(area.xMax / cellSize);
        int minY = Mathf.FloorToInt(area.yMin / cellSize);
        int maxY = Mathf.FloorToInt(area.yMax / cellSize);
        for (int x = minX; x <= maxX; x++) {
            for (int y = minY; y <= maxY; y++) {
                var key = new Vector2Int(x, y);
                if (buckets.TryGetValue(key, out var list)) {
                    foreach (var seg in list) yield return seg;
                }
            }
        }
    }
}
```

---

## 3. IvyNode.cs
**Purpose:** Continuous, branching growth toward center; stops at center; density controls; spatial partitioning.
```csharp
using UnityEngine;
using System.Collections;

public class IvyNode : MonoBehaviour {
    [Header("Prefabs & Growth")]
    public GameObject branchPrefab;
    public GameObject leafPrefab;
    public float baseInterval = 0.5f;
    public float segmentLength = 0.5f;

    [Header("Density Controls")]
    public float branchChance = 0.3f;
    public float branchDecay = 0.8f;
    public int maxBranchSpawns = 1;
    public float leafSpawnChance = 0.5f;
    public int maxLeavesPerBranch = 2;

    [Header("Generation & Health")]
    public int maxDepth = 8;
    public AnimationCurve healthCurve;

    [HideInInspector] public int depth = 0;

    void Start() {
        StartCoroutine(GrowRoutine());
    }

    IEnumerator GrowRoutine() {
        while (depth < maxDepth) {
            // stop if at center
            if (Vector2.Distance(transform.position, GrowthManager.Instance.centerPoint) <= GrowthManager.Instance.centerRadius)
                yield break;

            SpawnBranch(Vector3.up);
            float interval = GrowthController.Instance.GetInterval(baseInterval);
            yield return new WaitForSeconds(interval);
        }
    }

    void SpawnBranch(Vector3 localDir) {
        Vector3 dir = transform.rotation * localDir;
        Vector3 pos = transform.position + dir * segmentLength;
        Quaternion rot = Quaternion.LookRotation(Vector3.forward, dir);

        // Pool or instantiate branch
        var branchObj = PoolManager.Instance.GetBranch();
        branchObj.transform.SetPositionAndRotation(pos, rot);
        branchObj.SetActive(true);

        var seg = branchObj.GetComponent<IvySegment>();
        // init health
        seg.InitHealth(healthCurve.Evaluate(depth + 1));
        // spatial insert
        SpatialPartitionManager.Instance.Insert(seg);

        var node = branchObj.GetComponent<IvyNode>();
        node.depth = depth + 1;
        node.CopySettings(this);

        // Leaves
        if (Random.value < leafSpawnChance) {
            int count = Random.Range(1, maxLeavesPerBranch + 1);
            for (int i = 0; i < count; i++) {
                float angle = Random.Range(-30f, 30f);
                Vector3 ldir = Quaternion.Euler(0, 0, angle) * dir;
                Vector3 lpos = pos + ldir * (segmentLength * 0.5f);
                var leafObj = PoolManager.Instance.GetLeaf();
                leafObj.transform.SetPositionAndRotation(lpos, Quaternion.LookRotation(Vector3.forward, ldir));
                leafObj.SetActive(true);
            }
        }

        // Side branches with decay
        float effChance = branchChance * Mathf.Pow(branchDecay, depth);
        int spawned = 0;
        for (int i = 0; i < maxBranchSpawns && spawned < maxBranchSpawns; i++) {
            if (Random.value < effChance) {
                float a = Random.Range(20f,45f) * (Random.value<0.5f?1:-1);
                node.SpawnBranch(Quaternion.Euler(0,0,a) * Vector3.up);
                spawned++;
            }
        }
    }

    public void CopySettings(IvyNode o) {
        branchPrefab       = o.branchPrefab;
        leafPrefab         = o.leafPrefab;
        baseInterval       = o.baseInterval;
        segmentLength      = o.segmentLength;
        branchChance       = o.branchChance;
        branchDecay        = o.branchDecay;
        maxBranchSpawns    = o.maxBranchSpawns;
        leafSpawnChance    = o.leafSpawnChance;
        maxLeavesPerBranch = o.maxLeavesPerBranch;
        maxDepth           = o.maxDepth;
        healthCurve        = o.healthCurve;
    }
}
```

---

## 4. IvySegment.cs
**Purpose:** Health, XP on destroy, pooling, spatial removal.
```csharp
using UnityEngine;

public class IvySegment : MonoBehaviour {
    public float xpOnDestroy = 10f;
    private float maxHealth, currentHealth;

    public void InitHealth(float h) { maxHealth=h; currentHealth=h; }

    public void TakeDamage(float amt) {
        currentHealth -= amt;
        if (currentHealth <= 0) {
            XPManager.Instance.AddXP(xpOnDestroy);
            // remove from spatial grid & pool
            SpatialPartitionManager.Instance.Remove(this);
            PoolManager.Instance.ReleaseBranch(gameObject);
        }
    }
}
```

---

*(Leaf and other scripts remain largely the same as before.)*

---

## 15. Setup Recap & Tips
1. **Managers**:
   - **GrowthManager**: set `centerRadius` to define protected area.
   - **SpatialPartitionManager**: attach to a manager object.
   - **PoolManager**: configure pool sizes.
   - **GrowthController**: control global speed.
2. **Prefabs:**
   - **Branch Prefab**: `IvyNode`, `IvySegment` + collider, tag `IvySegment`.
   - **Leaf Prefab**: `IvySway` + sprite (pooled).
   - **Root Prefab**: empty with `IvyNode` (assign branch/leaf prefabs).
3. **Player**: `Rigidbody2D`, `CircleCollider2D(Trigger)`, `PlayerMovement`, `PlayerStats`, `IvyCutterCollision`.
4. **Center Area**: trigger collider with `CenterAreaProtection`.
5. **UI**: use `PlayerUI` with TextMeshProUGUI.
6. **Camera**: `CameraFollow` & `CameraShake`.
7. **Performance**: object pooling, density caps, spatial queries, stop growth at center.

Use this markdown in Cursor to generate all scripts and set up your optimized, spatially partitioned ivy growth system that stops at the center. Good luck defending your jam's heart!

