# Ivy Growth, Cutting & Rogue-Lite System for Overgrown Ivy Jam

This markdown outlines the scripts and setup you’ll need. Paste each code block into Cursor to generate the corresponding C# file in your Unity project.

---

## 1. GrowthManager.cs
**Purpose:** Spawns multiple ivy roots randomly around the room perimeter, all growing inward toward the center.

```csharp
using UnityEngine;

public class GrowthManager : MonoBehaviour {
    [Tooltip("Prefab with IvyNode attached")]
    public GameObject ivyRootPrefab;
    public float spawnRadius = 10f;
    public int rootCount = 8;

    void Start() {
        Vector3 center = transform.position;
        for (int i = 0; i < rootCount; i++) {
            float angle = i * (360f / rootCount) + Random.Range(0f, 360f / rootCount);
            Vector3 pos = center + Quaternion.Euler(0, 0, angle) * Vector3.up * spawnRadius;
            GameObject root = Instantiate(ivyRootPrefab, pos, Quaternion.identity);
            // Orient to grow toward center
            root.transform.up = (center - pos).normalized;
        }
    }
}
```

---

## 2. IvyNode.cs
**Purpose:** Procedural, transform-based ivy growth with branching and health scaling based on player XP curve.

```csharp
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
```

---

## 3. IvySegment.cs
**Purpose:** Adds scalable health and XP drop on death.

```csharp
using UnityEngine;

public class IvySegment : MonoBehaviour {
    [Tooltip("Base XP granted when destroyed")]
    public float xpOnDestroy = 10f;

    private float maxHealth;
    private float currentHealth;

    void Awake() {
        // if InitHealth wasn't called, fallback
        if (maxHealth <= 0f) maxHealth = 50f;
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Initialize health when spawning
    /// </summary>
    public void InitHealth(float health) {
        maxHealth = health;
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount) {
        currentHealth -= amount;
        if (currentHealth <= 0f) {
            XPManager.Instance.AddXP(xpOnDestroy);
            Destroy(gameObject);
        }
    }
}
```

---

## 4. PlayerStats.cs
**Purpose:** Tracks player level-based stats: finesse (damage rate) and cutter collider radius.

```csharp
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PlayerStats : MonoBehaviour {
    public static PlayerStats Instance;

    public float baseFinesse = 10f;
    public float finessePerLevel = 2f;
    public float baseRadius = 0.5f;
    public float radiusPerLevel = 0.1f;

    private CircleCollider2D circle;
    public float Finesse { get; private set; }

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        circle = GetComponent<CircleCollider2D>();
    }

    void Update() {
        int lvl = XPManager.Instance.level;
        Finesse = baseFinesse + finessePerLevel * (lvl - 1);
        circle.radius = baseRadius + radiusPerLevel * (lvl - 1);
    }
}
```

---

## 5. IvyCutterCollision.cs
**Purpose:** Applies continuous damage to ivy segments on contact, based on player finesse.

```csharp
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class IvyCutterCollision : MonoBehaviour {
    void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("IvySegment")) {
            IvySegment seg = other.GetComponent<IvySegment>();
            seg.TakeDamage(PlayerStats.Instance.Finesse * Time.deltaTime);
        }
    }
}
```

---

## 6. XPManager.cs
**Purpose:** Track XP, leveling, and upgrade points.

```csharp
using UnityEngine;

public class XPManager : MonoBehaviour {
    public static XPManager Instance;
    public float xp = 0f;
    public int level = 1;
    public int upgradePoints = 0;
    [Tooltip("Curve: level → XP needed for next level")]
    public AnimationCurve xpCurve;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddXP(float amount) {
        xp += amount;
        CheckLevelUp();
    }

    void CheckLevelUp() {
        float needed = xpCurve.Evaluate(level);
        while (xp >= needed) {
            xp -= needed;
            level++;
            upgradePoints++;
            // TODO: trigger level-up UI
            needed = xpCurve.Evaluate(level);
        }
    }
}
```

---

## 7. CutterItem.cs (Optional)
**Purpose:** Define run-only stat-boosting items.

```csharp
using UnityEngine;

public enum StatType { CutSpeed, CutRange, ComboWindow }

[CreateAssetMenu(menuName = "Items/CutterItem")]
public class CutterItem : ScriptableObject {
    public string itemName;
    public Sprite icon;
    public StatType boosts;
    public float amount;
}
```

---

## 8. PlayerMovement.cs
**Purpose:** Smooth top-down movement with acceleration/deceleration and feedback particles.

```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour {
    public float maxSpeed = 5f;
    public float acceleration = 20f;
    public float deceleration = 25f;
    public ParticleSystem moveParticles;

    private Rigidbody2D rb;
    private Vector2 input;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update() {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (input.magnitude > 0.1f && !moveParticles.isPlaying) moveParticles.Play();
        else if (input.magnitude <= 0.1f && moveParticles.isPlaying) moveParticles.Stop();
    }

    void FixedUpdate() {
        Vector2 targetVel = input * maxSpeed;
        rb.velocity = Vector2.MoveTowards(rb.velocity, targetVel,
            (input.magnitude > 0.1f ? acceleration : deceleration) * Time.fixedDeltaTime);
    }
}
```

---

## 9. CameraFollow.cs
**Purpose:** Smoothly follow the player.

```csharp
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    void LateUpdate() {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed);
    }
}
```

---

## 10. CameraShake.cs
**Purpose:** Screen shake for impactful events.

```csharp
using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour {
    public static CameraShake Instance;
    void Awake() {
        if (Instance == null) Instance = this; else Destroy(gameObject);
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
```

---

## 11. Setup Recap & Tips

1. **GrowthManager**:
   - Attach to an empty GameObject at center, assign `ivyRootPrefab`.
2. **Segment Prefab**:
   - Sprite + `BoxCollider2D` (Is Trigger), Tag = **IvySegment**.
   - Attach `IvyNode`, `IvySway`, and `IvySegment`.
3. **Root Prefab**:
   - Prefab containing only an empty GameObject with `IvyNode` + `IvySway` + `IvySegment`.
4. **Player**:
   - Add `Rigidbody2D` + `CircleCollider2D` (Is Trigger).
   - Attach `PlayerMovement`, `PlayerStats`, and `IvyCutterCollision`.
5. **Managers**:
   - `XPManager`: empty GameObject, set `xpCurve` for level progression.
6. **Camera**:
   - Main Camera: attach `CameraFollow` (set target & offset) and `CameraShake`.
7. **Curve Setup**:
   - Use the same `AnimationCurve` asset for `healthCurve` in `IvyNode` and `xpCurve` in `XPManager` so node health scales with XP progression.
8. **Performance**:
   - Pool ivy segments, limit `maxDepth`, reuse particle systems.

Use this markdown in Cursor to generate all scripts and set up your top-down rogue-lite ivy cutter game with random peripheral growth and health-scaling nodes matching your XP curve. Good luck in your jam!

