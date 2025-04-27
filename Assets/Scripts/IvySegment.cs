using UnityEngine;

public class IvySegment : MonoBehaviour {
    [Tooltip("Base XP granted when destroyed")]
    public float xpOnDestroy = 10f;

    private float maxHealth;
    private float currentHealth;
    private bool isDestroyed = false;

    void Awake() {
        // if InitHealth wasn't called, fallback
        if (maxHealth <= 0f) maxHealth = 50f;
        currentHealth = maxHealth;
        isDestroyed = false;
    }

    void OnEnable() {
        // Reset destroyed flag when reused from pool
        isDestroyed = false;
        // Reset health if needed
        if (currentHealth <= 0) {
            currentHealth = maxHealth;
        }
    }

    /// <summary>
    /// Initialize health when spawning
    /// </summary>
    public void InitHealth(float h) {
        maxHealth = h;
        currentHealth = h;
    }

    public void TakeDamage(float amount) {
        // Prevent processing already "destroyed" objects
        if (isDestroyed) return;
        
        currentHealth -= amount;
        if (currentHealth <= 0f) {
            isDestroyed = true;
            
            // Add XP if the manager exists
            if (XPManager.Instance != null) {
                XPManager.Instance.AddXP(xpOnDestroy);
            }
            
            // First, handle potential children
            HandleChildren();
            
            // Return to pool instead of destroying
            if (IvyNode.IsPoolingEnabled()) {
                IvyNode.ReturnToPool(gameObject, true);
                
                // Also attempt to return parent node to pool if it exists
                IvyNode parentNode = GetComponent<IvyNode>();
                if (parentNode != null) {
                    parentNode.enabled = false;
                }
            } else {
                Destroy(gameObject);
            }
        }
    }
    
    private void HandleChildren() {
        // If we have child objects (like leaves), return them to pool too
        foreach (Transform child in transform) {
            if (child.gameObject.activeSelf) {
                if (IvyNode.IsPoolingEnabled()) {
                    // Assuming leaves for simplicity
                    IvyNode.ReturnToPool(child.gameObject, false);
                } else {
                    Destroy(child.gameObject);
                }
            }
        }
    }
} 