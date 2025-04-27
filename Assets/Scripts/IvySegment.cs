using UnityEngine;
using System;

public class IvySegment : MonoBehaviour {
    [Tooltip("Base XP granted when destroyed")]
    public float xpOnDestroy = 10f;

    private float maxHealth;
    private float currentHealth;
    private bool isDestroyed = false;
    
    // Callback for when segment is destroyed
    public Action<GameObject> onDestroy;

    void Awake() {
        // if InitHealth wasn't called, fallback
        if (maxHealth <= 0f) maxHealth = 50f;
        currentHealth = maxHealth;
        isDestroyed = false;
    }
    
    void OnEnable() {
        isDestroyed = false;
    }

    /// <summary>
    /// Initialize health when spawning
    /// </summary>
    public void InitHealth(float h) {
        maxHealth = h;
        currentHealth = h;
        isDestroyed = false;
    }

    public void TakeDamage(float amount) {
        // Check if already destroyed to prevent multiple calls
        if (isDestroyed) return;
        
        currentHealth -= amount;
        if (currentHealth <= 0f) {
            isDestroyed = true;
            
            if (XPManager.Instance != null) {
                XPManager.Instance.AddXP(xpOnDestroy);
            }
            
            // Remove from spatial partition if available
            if (SpatialPartitionManager.Instance != null) {
                SpatialPartitionManager.Instance.Remove(this);
            }
            
            // Call the callback if assigned
            if (onDestroy != null) {
                onDestroy(gameObject);
            } else {
                Destroy(gameObject);
            }
        }
    }
    
    // Reset segment for reuse from pool
    public void ResetSegment() {
        currentHealth = maxHealth;
        isDestroyed = false;
    }
} 