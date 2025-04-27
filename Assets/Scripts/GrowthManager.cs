using UnityEngine;

public class GrowthManager : MonoBehaviour {
    [Header("Ivy Generation")]
    [Tooltip("Prefab with IvyNode attached")]
    public GameObject ivyRootPrefab;
    public float spawnRadius = 10f;
    public int rootCount = 8;
    
    [Header("Growth Rate")]
    [Tooltip("Minimum random growth rate (0.5 = half speed, 2.0 = double speed)")]
    [Range(0.1f, 2.0f)] public float minGrowthRate = 0.6f;
    [Tooltip("Maximum random growth rate (0.5 = half speed, 2.0 = double speed)")]
    [Range(0.1f, 3.0f)] public float maxGrowthRate = 1.4f;
    [Tooltip("Show debug text with growth rates")]
    public bool showGrowthRateDebug = false;
    
    [Header("Performance Settings")]
    [Tooltip("Pause growth when FPS drops below this threshold")]
    public float minFpsThreshold = 30f;
    [Tooltip("Resume growth when FPS rises above this threshold")]
    public float resumeFpsThreshold = 40f;
    [Tooltip("Check performance every N seconds")]
    public float performanceCheckInterval = 2f;
    [Tooltip("Automatically adjust growth parameters for performance")]
    public bool autoAdjustForPerformance = true;
    
    private float lastPerformanceCheck;
    private float currentFps;
    private bool growthPaused = false;

    void Start() {
        Vector3 center = transform.position;
        for (int i = 0; i < rootCount; i++) {
            float angle = i * (360f / rootCount) + Random.Range(0f, 360f / rootCount);
            Vector3 pos = center + Quaternion.Euler(0, 0, angle) * Vector3.up * spawnRadius;
            GameObject root = Instantiate(ivyRootPrefab, pos, Quaternion.identity);
            
            // Orient to grow toward center
            root.transform.up = (center - pos).normalized;
            
            // Configure random growth rate
            IvyNode ivyNode = root.GetComponent<IvyNode>();
            if (ivyNode != null) {
                ivyNode.minGrowthRate = minGrowthRate;
                ivyNode.maxGrowthRate = maxGrowthRate;
                
                // Explicitly set random growth rate (will be used instead of automatic assignment)
                float randomRate = Random.Range(minGrowthRate, maxGrowthRate);
                ivyNode.growthRateMultiplier = randomRate;
                
                // Add debug text object if enabled
                if (showGrowthRateDebug) {
                    CreateDebugText(root, randomRate);
                }
            }
        }
        
        lastPerformanceCheck = Time.time;
    }
    
    void Update() {
        if (autoAdjustForPerformance) {
            // Calculate FPS
            currentFps = 1f / Time.unscaledDeltaTime;
            
            // Only check periodically to avoid overhead
            if (Time.time - lastPerformanceCheck > performanceCheckInterval) {
                CheckPerformance();
                lastPerformanceCheck = Time.time;
            }
        }
    }
    
    // Create a simple text object showing the growth rate
    void CreateDebugText(GameObject root, float rate) {
        GameObject textObj = new GameObject("GrowthRateText");
        textObj.transform.position = root.transform.position;
        textObj.transform.parent = root.transform;
        
        // Add TextMesh component if available in the project
        // If not using TextMesh, this can be removed
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = rate.ToString("F2") + "x";
        textMesh.fontSize = 14;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = Color.white;
        
        // Position slightly offset from the root
        textObj.transform.localPosition = new Vector3(0.3f, 0, 0);
        textObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }
    
    private void CheckPerformance() {
        // If FPS is too low, pause growth
        if (!growthPaused && currentFps < minFpsThreshold) {
            IvyNode.PauseAllGrowth();
            growthPaused = true;
            Debug.Log("Growth paused: FPS dropped below " + minFpsThreshold);
        }
        // If FPS is good again, resume growth
        else if (growthPaused && currentFps > resumeFpsThreshold) {
            IvyNode.ResumeAllGrowth();
            growthPaused = false;
            Debug.Log("Growth resumed: FPS recovered to " + currentFps);
        }
    }
    
    // Can be called from UI to toggle growth manually
    public void ToggleGrowth() {
        growthPaused = !growthPaused;
        if (growthPaused) {
            IvyNode.PauseAllGrowth();
        } else {
            IvyNode.ResumeAllGrowth();
        }
    }
} 