using UnityEngine;
using System.Collections;

public class CollisionFixer : MonoBehaviour {
    [Tooltip("Run the collider check when the game starts")]
    public bool fixOnStart = true;
    
    [Tooltip("Fix all colliders with this tag")]
    public string targetTag = "IvySegment";
    
    [Tooltip("Set colliders of target objects to be triggers")]
    public bool makeCollidersTriggers = true;
    
    [Tooltip("Print debug info")]
    public bool debugMode = true;
    
    [Tooltip("Check for new objects every X seconds")]
    public float checkInterval = 5f;
    
    void Start() {
        if (fixOnStart) {
            FixColliders();
        }
        
        // Start periodic checking
        StartCoroutine(PeriodicCheck());
    }
    
    IEnumerator PeriodicCheck() {
        while (true) {
            yield return new WaitForSeconds(checkInterval);
            FixColliders();
        }
    }
    
    // Can be called manually from other scripts
    public void FixColliders() {
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(targetTag);
        int fixedCount = 0;
        
        foreach (GameObject obj in targetObjects) {
            Collider2D[] colliders = obj.GetComponents<Collider2D>();
            
            foreach (Collider2D collider in colliders) {
                if (collider.isTrigger != makeCollidersTriggers) {
                    collider.isTrigger = makeCollidersTriggers;
                    fixedCount++;
                }
            }
        }
        
        if (debugMode && fixedCount > 0) {
            Debug.Log($"Fixed {fixedCount} colliders on {targetObjects.Length} objects with tag '{targetTag}'");
        }
    }
    
    // Editor menu item to fix colliders manually (if needed)
    [ContextMenu("Fix All Tagged Colliders")]
    void FixAllColliders() {
        FixColliders();
    }
} 