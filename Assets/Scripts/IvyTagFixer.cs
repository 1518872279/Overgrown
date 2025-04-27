using UnityEngine;

[DefaultExecutionOrder(-100)] // Run before other scripts
public class IvyTagFixer : MonoBehaviour {
    [Tooltip("Tag to apply to all ivy objects")]
    public string ivyTag = "IvySegment";
    
    [Tooltip("Run in Update instead of just on Start (more thorough but less performant)")]
    public bool runEveryFrame = false;
    
    [Tooltip("Print debug info")]
    public bool debugMode = true;
    
    void Start() {
        if (debugMode) {
            Debug.Log("IvyTagFixer: Running initial tag fixing pass");
        }
        FixIvyTags();
    }
    
    void Update() {
        if (runEveryFrame) {
            FixIvyTags();
        }
    }
    
    void FixIvyTags() {
        // Method 1: Find by IvySegment component
        IvySegment[] segments = FindObjectsOfType<IvySegment>();
        int fixedCount = 0;
        
        foreach (IvySegment segment in segments) {
            if (segment.gameObject.tag != ivyTag) {
                segment.gameObject.tag = ivyTag;
                fixedCount++;
            }
        }
        
        // Method 2: Find by IvyNode component
        IvyNode[] nodes = FindObjectsOfType<IvyNode>();
        foreach (IvyNode node in nodes) {
            if (node.gameObject.tag != ivyTag) {
                node.gameObject.tag = ivyTag;
                fixedCount++;
            }
        }
        
        // Print results only if we fixed something and debugMode is on
        if (debugMode && fixedCount > 0) {
            Debug.Log($"IvyTagFixer: Fixed {fixedCount} objects to have tag '{ivyTag}'");
        }
    }
    
    // Allows calling from UI buttons or other scripts
    public void FixTagsNow() {
        FixIvyTags();
    }
} 