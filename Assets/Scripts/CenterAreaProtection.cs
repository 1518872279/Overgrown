using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CenterAreaProtection : MonoBehaviour {
    [Tooltip("How many ivy segments need to enter to trigger game over")]
    public int segmentsToTriggerGameOver = 1;
    
    [Header("Tag Settings")]
    [Tooltip("Tags to check for (will detect any of these)")]
    public string[] tagsToCheck = new string[] { "IvySegment", "Ivy", "Untagged" };
    [Tooltip("Alternative: check for IvySegment component instead of tag")]
    public bool checkComponentInsteadOfTag = true;
    
    [Header("Visibility")]
    [Tooltip("Make the protection area visible in game")]
    public bool showProtectionArea = true;
    public Color areaColor = new Color(1f, 0f, 0f, 0.2f);
    public Color borderColor = new Color(1f, 0f, 0f, 0.8f);
    
    [Header("Debug")]
    [Tooltip("Enable debug logging")]
    public bool debugMode = true;
    [Tooltip("Use manual collision checking as backup")]
    public bool useManualDetection = true;
    [Tooltip("Update interval for manual detection")]
    public float manualCheckInterval = 0.5f;
    [Tooltip("Radius for manual detection (0 = use collider radius)")]
    public float manualCheckRadius = 0f;
    
    private int segmentsInside = 0;
    private CircleCollider2D circleCollider;
    private SpriteRenderer spriteRenderer;
    private HashSet<GameObject> trackedObjects = new HashSet<GameObject>();
    private Coroutine manualCheckRoutine;
    
    void Awake() {
        // Get or add necessary components
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null) {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        
        // Always ensure trigger is enabled for this collider
        circleCollider.isTrigger = true;
        
        // Create visual representation if showProtectionArea is true
        if (showProtectionArea && transform.childCount == 0) {
            // Create circle sprite
            GameObject circle = new GameObject("ProtectionVisual");
            circle.transform.SetParent(transform);
            circle.transform.localPosition = Vector3.zero;
            
            spriteRenderer = circle.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateCircleSprite();
            spriteRenderer.color = areaColor;
            
            // Match the size to the collider
            if (circleCollider != null) {
                circle.transform.localScale = new Vector3(
                    circleCollider.radius * 2,
                    circleCollider.radius * 2,
                    1
                );
            }

            // Set sorting layer to ensure visibility
            spriteRenderer.sortingOrder = 10;
        }
        
        if (debugMode) {
            Debug.Log("CenterAreaProtection initialized - Radius: " + circleCollider.radius + ", Trigger: " + circleCollider.isTrigger);
            Debug.Log("Looking for tags: " + string.Join(", ", tagsToCheck));
        }
        
        // Add a rigidbody to ensure collision detection works
        if (GetComponent<Rigidbody2D>() == null) {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;
            rb.gravityScale = 0;
        }
    }
    
    void OnEnable() {
        // Reset counter
        segmentsInside = 0;
        trackedObjects.Clear();
        
        // Start manual detection if enabled
        if (useManualDetection && manualCheckRoutine == null) {
            manualCheckRoutine = StartCoroutine(ManualDetectionRoutine());
        }
    }
    
    void OnDisable() {
        if (manualCheckRoutine != null) {
            StopCoroutine(manualCheckRoutine);
            manualCheckRoutine = null;
        }
    }
    
    // This handles triggers (isTrigger=true colliders)
    void OnTriggerEnter2D(Collider2D other) {
        if (debugMode) {
            Debug.Log("OnTriggerEnter2D called with: " + other.gameObject.name + " (tag: " + other.gameObject.tag + ")");
        }
        HandleCollisionEnter(other.gameObject);
    }
    
    void OnTriggerExit2D(Collider2D other) {
        HandleCollisionExit(other.gameObject);
    }
    
    // This handles solid colliders (isTrigger=false colliders) 
    void OnCollisionEnter2D(Collision2D collision) {
        if (debugMode) {
            Debug.Log("OnCollisionEnter2D called with: " + collision.gameObject.name + " (tag: " + collision.gameObject.tag + ")");
        }
        HandleCollisionEnter(collision.gameObject);
    }
    
    void OnCollisionExit2D(Collision2D collision) {
        HandleCollisionExit(collision.gameObject);
    }
    
    // Manual detection routine as backup
    IEnumerator ManualDetectionRoutine() {
        WaitForSeconds wait = new WaitForSeconds(manualCheckInterval);
        
        while (true) {
            yield return wait;
            CheckForObjectsInRadius();
        }
    }
    
    void CheckForObjectsInRadius() {
        float radius = manualCheckRadius > 0 ? manualCheckRadius : circleCollider.radius;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
        
        HashSet<GameObject> currentlyInside = new HashSet<GameObject>();
        
        foreach (Collider2D collider in colliders) {
            if (IsTargetObject(collider.gameObject)) {
                currentlyInside.Add(collider.gameObject);
                
                // If not already tracked, handle as new entry
                if (!trackedObjects.Contains(collider.gameObject)) {
                    HandleCollisionEnter(collider.gameObject);
                }
            }
        }
        
        // Find objects that left the area
        List<GameObject> objectsToRemove = new List<GameObject>();
        foreach (GameObject obj in trackedObjects) {
            if (!currentlyInside.Contains(obj)) {
                HandleCollisionExit(obj);
                objectsToRemove.Add(obj);
            }
        }
        
        // Remove tracked objects that left
        foreach (GameObject obj in objectsToRemove) {
            trackedObjects.Remove(obj);
        }
    }
    
    // Unified collision handling
    private void HandleCollisionEnter(GameObject other) {
        if (debugMode) {
            Debug.Log("Checking object: " + other.name + " with tag: " + other.tag);
        }
        
        if (IsTargetObject(other)) {
            // Add to tracked objects
            trackedObjects.Add(other);
            
            segmentsInside++;
            
            if (debugMode) {
                Debug.Log("✅ IVY DETECTED! Count: " + segmentsInside + "/" + segmentsToTriggerGameOver);
            }
            
            // Visual feedback (optional)
            if (spriteRenderer != null) {
                spriteRenderer.color = Color.Lerp(areaColor, borderColor, segmentsInside / (float)segmentsToTriggerGameOver);
            }
            
            if (segmentsInside >= segmentsToTriggerGameOver) {
                TriggerGameOver();
            }
        }
    }
    
    private void HandleCollisionExit(GameObject other) {
        if (IsTargetObject(other)) {
            // Remove from tracked objects
            trackedObjects.Remove(other);
            
            segmentsInside = Mathf.Max(0, segmentsInside - 1);
            
            if (debugMode) {
                Debug.Log("IvySegment exited center! Count: " + segmentsInside + "/" + segmentsToTriggerGameOver);
            }
            
            // Visual feedback
            if (spriteRenderer != null) {
                spriteRenderer.color = Color.Lerp(areaColor, borderColor, segmentsInside / (float)segmentsToTriggerGameOver);
            }
        }
    }
    
    // Check if this is an ivy segment using multiple methods
    private bool IsTargetObject(GameObject obj) {
        // Method 1: Check by tag
        foreach (string tag in tagsToCheck) {
            if (obj.CompareTag(tag)) {
                if (debugMode) {
                    Debug.Log("Found matching tag: " + tag + " on object: " + obj.name);
                }
                return true;
            }
        }
        
        // Method 2: Check by component if enabled
        if (checkComponentInsteadOfTag) {
            if (obj.GetComponent<IvySegment>() != null) {
                if (debugMode) {
                    Debug.Log("Found IvySegment component on object: " + obj.name);
                }
                return true;
            }
        }
        
        // Method 3: Check name as last resort
        if (obj.name.Contains("Ivy") || obj.name.Contains("ivy")) {
            if (debugMode) {
                Debug.Log("Name contains 'Ivy': " + obj.name);
            }
            return true;
        }
        
        return false;
    }
    
    void TriggerGameOver() {
        if (debugMode) {
            Debug.Log("GAME OVER triggered by CenterAreaProtection!");
        }
        
        // Use GameManager if available, otherwise just log
        if (GameManager.Instance != null) {
            GameManager.Instance.GameOver("Center Area Overtaken by Ivy");
        } else {
            Debug.LogWarning("Game Over: Center Covered - GameManager not found");
        }
    }
    
    // Helper method to create a circle sprite at runtime
    Sprite CreateCircleSprite() {
        // Create a simple white circle texture
        int resolution = 128;
        Texture2D texture = new Texture2D(resolution, resolution);
        Color[] colors = new Color[resolution * resolution];
        
        // Fill with transparent pixels by default
        for (int i = 0; i < colors.Length; i++) {
            colors[i] = Color.clear;
        }
        
        // Draw circle
        float radius = resolution / 2f;
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        
        for (int y = 0; y < resolution; y++) {
            for (int x = 0; x < resolution; x++) {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius) {
                    colors[y * resolution + x] = Color.white;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), 
            new Vector2(0.5f, 0.5f), resolution);
    }
    
    // Draw the collider in the Scene view for debugging
    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        if (circleCollider != null) {
            Gizmos.DrawWireSphere(transform.position, circleCollider.radius);
        } else {
            // Default radius if collider not yet assigned
            Gizmos.DrawWireSphere(transform.position, 1.5f);
        }
    }
} 