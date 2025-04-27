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