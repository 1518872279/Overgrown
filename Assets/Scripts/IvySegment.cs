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
    public void InitHealth(float h) {
        maxHealth = h;
        currentHealth = h;
    }

    public void TakeDamage(float amount) {
        currentHealth -= amount;
        if (currentHealth <= 0f) {
            XPManager.Instance.AddXP(xpOnDestroy);
            Destroy(gameObject);
        }
    }
} 