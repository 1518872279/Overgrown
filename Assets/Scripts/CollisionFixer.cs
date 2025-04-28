using UnityEngine;
using System.Collections;

public class CollisionFixer : MonoBehaviour {
    [Tooltip("Run the collider check when the game starts")]
    public bool checkOnStart = true;
    
    [Tooltip("Fix all colliders with this tag")]
    public string targetTag = "IvySegment";
    
    [Tooltip("Set colliders of target objects to be triggers")]
    public bool makeCollidersTriggers = true;
    
    [Tooltip("Only report issues, don't modify colliders")]
    public bool reportOnlyMode = true;
    
    [Tooltip("Print debug info")]
    public bool debugMode = true;
    
    [Tooltip("Check for new objects every X seconds")]
    public float checkInterval = 5f;
    
    void Start() {
        if (checkOnStart) {
            CheckColliders();
        }
        
        // Start periodic checking
        StartCoroutine(PeriodicCheck());
    }
    
    IEnumerator PeriodicCheck() {
        while (true) {
            yield return new WaitForSeconds(checkInterval);
            CheckColliders();
        }
    }
    
    // Can be called manually from other scripts
    public void CheckColliders() {
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(targetTag);
        int fixedCount = 0;
        int issueCount = 0;
        
        foreach (GameObject obj in targetObjects) {
            Collider2D[] colliders = obj.GetComponents<Collider2D>();
            
            if (colliders.Length == 0 && debugMode) {
                Debug.LogWarning($"No colliders found on object '{obj.name}' with tag '{targetTag}'");
                continue;
            }
            
            foreach (Collider2D collider in colliders) {
                if (collider.isTrigger != makeCollidersTriggers) {
                    issueCount++;
                    
                    if (!reportOnlyMode) {
                        collider.isTrigger = makeCollidersTriggers;
                        fixedCount++;
                    }
                }
            }
        }
        
        if (debugMode) {
            if (reportOnlyMode) {
                if (issueCount > 0) {
                    Debug.Log($"Found {issueCount} colliders that could be fixed on {targetObjects.Length} objects with tag '{targetTag}' (Report-Only Mode)");
                }
            } else if (fixedCount > 0) {
                Debug.Log($"Fixed {fixedCount} colliders on {targetObjects.Length} objects with tag '{targetTag}'");
            }
        }
    }
    
    // Editor menu item to check colliders manually (if needed)
    [ContextMenu("Check Tagged Colliders")]
    void CheckAllColliders() {
        CheckColliders();
    }
} 