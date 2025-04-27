# Ivy Growth, Cutting & Rogue-Lite System for Overgrown Ivy Jam

This markdown outlines the scripts and setup you’ll need. Paste each code block into Cursor to generate the corresponding C# file in your Unity project.

---

## 1. IvyNode.cs
**Purpose:** Procedural, transform-based ivy growth with branching.

```csharp
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
        // Forward segment
        Vector3 forward = transform.up * segmentLength;
        var seg = Instantiate(segmentPrefab, transform.position + forward, transform.rotation, transform.parent);
        var node = seg.AddComponent<IvyNode>(); node.depth = depth + 1; node.CopySettings(this);
        // Random branches
        if (Random.value < branchChance) {
            SpawnBranch(Random.Range(20f, 45f));
            SpawnBranch(-Random.Range(20f, 45f));
        }
    }

    void SpawnBranch(float angle) {
        Quaternion rot = transform.rotation * Quaternion.Euler(0, 0, angle);
        Vector3 dir = rot * Vector3.up * segmentLength;
        var seg = Instantiate(segmentPrefab, transform.position + dir, rot, transform.parent);
        var node = seg.AddComponent<IvyNode>(); node.depth = depth + 1; node.CopySettings(this);
    }

    public void CopySettings(IvyNode other) {
        segmentPrefab   = other.segmentPrefab;
        growthInterval  = other.growthInterval;
        segmentLength   = other.segmentLength;
        branchChance    = other.branchChance;
        maxDepth        = other.maxDepth;
    }
}
```

---

## 2. IvySway.cs
**Purpose:** Adds a subtle sine-wave rotation to each segment for organic movement.

```csharp
using UnityEngine;

public class IvySway : MonoBehaviour {
    public float swaySpeed = 2f;
    public float swayAngle = 10f;
    private float baseAngle;

    void Start() {
        baseAngle = transform.eulerAngles.z;
    }

    void Update() {
        float phase = transform.GetSiblingIndex() * 0.5f;
        float offset = Mathf.Sin(Time.time * swaySpeed + phase) * swayAngle;
        transform.rotation = Quaternion.Euler(0, 0, baseAngle + offset);
    }
}
```

---

## 3. IvySegment.cs
**Purpose:** Adds health to ivy segments and handles damage from the player.

```csharp
using UnityEngine;

public class IvySegment : MonoBehaviour {
    public float maxHealth = 100f;
    public float xpOnDestroy = 10f;
    private float currentHealth;

    void Awake() {
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
**Purpose:** Tracks player level-based stats: finesse (damage rate) and collider radius.

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
**Purpose:** Applies continuous damage to ivy segments when in contact, based on player finesse.

```csharp
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class IvyCutterCollision : MonoBehaviour {
    void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("IvySegment")) {
            var seg = other.GetComponent<IvySegment>();
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

1. **Segment Prefab**
   - Sprite + `BoxCollider2D` (Is Trigger) + Tag = **IvySegment**.
   - Attach `IvyNode`, `IvySway`, and `IvySegment`.

2. **Root Spawner**
   - Empty GameObject + `IvyNode`, assign segment prefab.

3. **Player**
   - Add `Rigidbody2D` + `CircleCollider2D` (Is Trigger).
   - Attach `PlayerMovement`, `PlayerStats`, and `IvyCutterCollision`.

4. **Managers**
   - Empty GameObjects: `XPManager` (configure `xpCurve`).

5. **Camera**
   - Main Camera: attach `CameraFollow` (set target & offset) and `CameraShake`.

6. **Performance**
   - Pool ivy segments, limit `maxDepth`, and reuse particle systems.

Use this markdown in Cursor to generate all scripts and set up your top-down rogue-lite ivy cutter game with collision-based cutting and level-based finesse and radius. Good luck in your jam!

