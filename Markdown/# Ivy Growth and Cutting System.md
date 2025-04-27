# Ivy Growth, Cutting & Rogue-Lite System for Overgrown Ivy Jam

This markdown outlines the scripts and setup you’ll need. Paste each code block into Cursor to generate the corresponding C# files in your Unity project.

---

## 1. GrowthManager.cs
**Purpose:** Spawns ivy roots around the room perimeter, aimed inward.
```csharp
using UnityEngine;

public class GrowthManager : MonoBehaviour {
    public GameObject ivyRootPrefab; // Prefab with IvyNode
    public float spawnRadius = 10f;
    public int rootCount = 8;

    void Start() {
        Vector3 center = transform.position;
        for (int i = 0; i < rootCount; i++) {
            float angle = i * (360f / rootCount) + Random.Range(0f, 360f / rootCount);
            Vector3 pos = center + Quaternion.Euler(0, 0, angle) * Vector3.up * spawnRadius;
            GameObject root = Instantiate(ivyRootPrefab, pos, Quaternion.identity);
            root.transform.up = (center - pos).normalized;
        }
    }
}
```

---

## 2. IvyNode.cs
**Purpose:** Continuous, branching growth; separates branches and leaves; density controls; health scales with depth.
```csharp
using UnityEngine;

public class IvyNode : MonoBehaviour {
    [Header("Prefabs & Growth")]
    public GameObject branchPrefab;
    public GameObject leafPrefab;
    public float growthInterval = 0.5f;
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
        InvokeRepeating(nameof(Grow), growthInterval, growthInterval);
    }

    void Grow() {
        if (depth < maxDepth) SpawnBranch(Vector3.up);
    }

    void SpawnBranch(Vector3 localDir) {
        Vector3 dir = transform.rotation * localDir;
        Vector3 pos = transform.position + dir * segmentLength;
        Quaternion rot = Quaternion.LookRotation(Vector3.forward, dir);
        GameObject branch = Instantiate(branchPrefab, pos, rot, transform.parent);

        var node = branch.AddComponent<IvyNode>();
        node.CopySettings(this);
        node.depth = depth + 1;

        // Health
        var seg = branch.GetComponent<IvySegment>();
        seg.InitHealth(healthCurve.Evaluate(node.depth));

        // Leaves
        if (Random.value < leafSpawnChance) {
            int count = Random.Range(1, maxLeavesPerBranch + 1);
            for (int i = 0; i < count; i++) {
                float angle = Random.Range(-30f, 30f);
                Vector3 ldir = Quaternion.Euler(0, 0, angle) * dir;
                Vector3 lpos = pos + ldir * (segmentLength * 0.5f);
                Instantiate(leafPrefab, lpos, Quaternion.LookRotation(Vector3.forward, ldir), branch.transform);
            }
        }

        // Side branches
        float effChance = branchChance * Mathf.Pow(branchDecay, depth);
        int spawned = 0;
        for (int i = 0; i < maxBranchSpawns && spawned < maxBranchSpawns; i++) {
            if (Random.value < effChance) {
                float a = Random.Range(20f, 45f) * (Random.value < 0.5f ? 1 : -1);
                SpawnBranch(Quaternion.Euler(0,0,a) * Vector3.up);
                spawned++;
            }
        }
    }

    void CopySettings(IvyNode o) {
        branchPrefab       = o.branchPrefab;
        leafPrefab         = o.leafPrefab;
        growthInterval     = o.growthInterval;
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

## 3. IvySegment.cs
**Purpose:** Health and XP on destroy.
```csharp
using UnityEngine;

public class IvySegment : MonoBehaviour {
    public float xpOnDestroy = 10f;
    private float maxHealth;
    private float currentHealth;

    public void InitHealth(float h) {
        maxHealth = h; currentHealth = h;
    }

    public void TakeDamage(float amount) {
        currentHealth -= amount;
        if (currentHealth <= 0) {
            XPManager.Instance.AddXP(xpOnDestroy);
            Destroy(gameObject);
        }
    }
}
```

---

## 4. IvySway.cs
**Purpose:** Sine-wave sway for leaves.
```csharp
using UnityEngine;

public class IvySway : MonoBehaviour {
    public float swaySpeed = 2f;
    public float swayAngle = 15f;
    private float baseRot;

    void Start() => baseRot = transform.eulerAngles.z;

    void Update() {
        float offset = Mathf.Sin(Time.time * swaySpeed + transform.GetSiblingIndex()) * swayAngle;
        transform.rotation = Quaternion.Euler(0,0,baseRot + offset);
    }
}
```

---

## 5. PlayerStats.cs
**Purpose:** Level-based finesse & collider radius.
```csharp
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PlayerStats : MonoBehaviour {
    public static PlayerStats Instance;
    public float baseFinesse = 10f;
    public float finessePerLevel = 2f;
    public float baseRadius = 0.5f;
    public float radiusPerLevel = 0.1f;
    private CircleCollider2D col;
    public float Finesse { get; private set; }

    void Awake() {
        if (Instance==null) Instance=this; else Destroy(gameObject);
        col = GetComponent<CircleCollider2D>();
    }
    void Update() {
        int lvl = XPManager.Instance.level;
        Finesse = baseFinesse + finessePerLevel*(lvl-1);
        col.radius = baseRadius + radiusPerLevel*(lvl-1);
    }
}
```

---

## 6. IvyCutterCollision.cs
```csharp
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class IvyCutterCollision : MonoBehaviour {
    void OnTriggerStay2D(Collider2D o) {
        if (o.CompareTag("IvySegment")) {
            o.GetComponent<IvySegment>()
             .TakeDamage(PlayerStats.Instance.Finesse * Time.deltaTime);
        }
    }
}
```

---

## 7. CenterAreaProtection.cs
```csharp
using UnityEngine;

public class CenterAreaProtection : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D o) {
        if (o.CompareTag("IvySegment")) {
            Debug.Log("Game Over: Center Covered");
            // end-run logic
        }
    }
}
```

---

## 8. XPManager.cs
```csharp
using UnityEngine;

public class XPManager : MonoBehaviour {
    public static XPManager Instance;
    public float xp;
    public int level = 1;
    public int upgradePoints;
    public AnimationCurve xpCurve;
    void Awake() { if(Instance==null) Instance=this; else Destroy(gameObject); }
    public void AddXP(float amt) { xp+=amt; CheckLevelUp(); }
    void CheckLevelUp() {
        float need = xpCurve.Evaluate(level);
        while(xp>=need) { xp-=need; level++; upgradePoints++; need=xpCurve.Evaluate(level); }
    }
}
```

---

## 9. PlayerMovement.cs
```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour {
    public float maxSpeed=5f, acceleration=20f, deceleration=25f;
    public ParticleSystem moveParticles;
    Rigidbody2D rb; Vector2 input;
    void Awake()=>rb=GetComponent<Rigidbody2D>();
    void Update(){ input=new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")).normalized;
        if(input.magnitude>.1f&&!moveParticles.isPlaying) moveParticles.Play();
        else if(input.magnitude<=.1f&&moveParticles.isPlaying) moveParticles.Stop(); }
    void FixedUpdate(){
        Vector2 target=input*maxSpeed;
        rb.velocity=Vector2.MoveTowards(rb.velocity,target,(input.magnitude>.1f?acceleration:deceleration)*Time.fixedDeltaTime);
    }
}
```

---

## 10. CameraFollow.cs
```csharp
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public Transform target; public float smoothSpeed=0.125f; public Vector3 offset;
    void LateUpdate(){ if(target) transform.position=Vector3.Lerp(transform.position,target.position+offset,smoothSpeed); }
}
```

---

## 11. CameraShake.cs
```csharp
using UnityEngine; using System.Collections;
public class CameraShake : MonoBehaviour {
    public static CameraShake Instance;
    void Awake(){ if(Instance==null) Instance=this; else Destroy(gameObject); }
    public void Shake(float dur,float mag)=>StartCoroutine(DoShake(dur,mag));
    IEnumerator DoShake(float d,float m){ Vector3 orig=transform.localPosition; float t=0;
        while(t<d){ transform.localPosition=orig+(Vector3)Random.insideUnitCircle*m; t+=Time.deltaTime; yield return null; }
        transform.localPosition=orig;
    }
}
```

---

## 12. GrowthController.cs
**Purpose:** Manual & procedural control of ivy growth speed.
```csharp
using UnityEngine;

public class GrowthController : MonoBehaviour {
    public static GrowthController Instance;
    [Tooltip("Multiplier applied to all IvyNode growth intervals")] public float speedMultiplier = 1f;
    void Awake(){ if(Instance==null) Instance=this; else Destroy(gameObject);}    
    public float GetInterval(float baseInterval){ return baseInterval / speedMultiplier; }
}
```

> **Usage:** In `IvyNode`, replace `InvokeRepeating` with a coroutine using `GrowthController.Instance.GetInterval(growthInterval)` to wait.

---

## 13. PlayerUI.cs
**Purpose:** Display player level and finesse on-screen using TextMeshPro.
```csharp
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour {
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI finesseText;

    void Update() {
        levelText.text = "Level: " + XPManager.Instance.level;
        finesseText.text = "Finesse: " + Mathf.RoundToInt(PlayerStats.Instance.Finesse);
    }
}
```

---

## 14. Setup Recap & Tips
1. **Prefabs:**
   - **Branch Prefab:** `branchPrefab` + `IvyNode`, `IvySegment`, tag = `IvySegment`.
   - **Leaf Prefab:** simple sprite + `IvySway`.
   - **Root Prefab:** empty object with `IvyNode` (assign branch/leaf prefabs).
2. **GrowthManager:** attach at center.
3. **GrowthController:** attach to manager object, adjust `speedMultiplier` via UI or scripts.
4. **Player:** add `Rigidbody2D`, `CircleCollider2D (Trigger)`, `PlayerMovement`, `PlayerStats`, `IvyCutterCollision`.
5. **Center Area:** trigger collider + `CenterAreaProtection`.
6. **UI Canvas:** create two Text elements, assign to `PlayerUI`.
7. **Camera:** attach `CameraFollow` & `CameraShake` to main camera.
8. **Manager:** `XPManager` with `xpCurve`; reuse same `AnimationCurve` for `IvyNode.healthCurve`.
9. **Performance:** pool segments/leaves, tune `maxDepth`, limit branch/leaves via density controls.

Use this markdown in Cursor to generate all scripts and wire up dynamic growth speed and UI visualization. Good luck with your jam!

