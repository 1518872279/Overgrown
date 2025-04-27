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
        if (Instance == null) Instance = this; else Destroy(gameObject);
        circle = GetComponent<CircleCollider2D>();
    }

    void Update() {
        int lvl = XPManager.Instance.level;
        Finesse = baseFinesse + finessePerLevel * (lvl - 1);
        circle.radius = baseRadius + radiusPerLevel * (lvl - 1);
    }
} 