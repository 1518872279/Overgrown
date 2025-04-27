using UnityEngine;
using System.Collections.Generic;

public class GrowthManager : MonoBehaviour {
    [Header("Ivy Generation")]
    [Tooltip("Prefab with IvyNode attached")]
    public GameObject ivyRootPrefab;
    public float spawnRadius = 10f;
    public int rootCount = 8;
    [Tooltip("Minimum distance between spawn positions")]
    public float minSpawnDistance = 1.5f;
    [Tooltip("Maximum spawn attempts per position")]
    public int maxSpawnAttempts = 10;
    [Tooltip("Random jitter applied to spawn positions (0 = precise circle)")]
    [Range(0f, 1f)] public float positionJitter = 0.2f;
    
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
    private List<Vector3> spawnPositions = new List<Vector3>();

    void Start() {
        SpawnIvyRoots();
        lastPerformanceCheck = Time.time;
    }
    
    void SpawnIvyRoots() {
        Vector3 center = transform.position;
        spawnPositions.Clear();
        
        // Try to spawn the requested number of roots
        int successfulSpawns = 0;
        int totalAttempts = 0;
        
        // Pre-calculate potential positions
        List<Vector3> candidatePositions = new List<Vector3>();
        int angleDivisions = rootCount * 3; // Generate more candidate positions than needed
        
        for (int i = 0; i < angleDivisions; i++) {
            float angle = i * (360f / angleDivisions);
            float radiusOffset = Random.Range(-spawnRadius * positionJitter, spawnRadius * positionJitter);
            float actualRadius = spawnRadius + radiusOffset;
            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
            Vector3 position = center + direction * actualRadius;
            candidatePositions.Add(position);
        }
        
        // Shuffle the candidate positions for more randomness
        ShuffleList(candidatePositions);
        
        // Try positions until we get enough successful spawns
        while (successfulSpawns < rootCount && totalAttempts < maxSpawnAttempts * rootCount) {
            totalAttempts++;
            
            // Get the next candidate position, or generate new ones if we ran out
            Vector3 pos;
            if (candidatePositions.Count > 0) {
                pos = candidatePositions[0];
                candidatePositions.RemoveAt(0);
            } else {
                // Generate a completely random position if we've exhausted our candidates
                float randomAngle = Random.Range(0f, 360f);
                float randomRadius = spawnRadius * Random.Range(0.8f, 1.2f);
                pos = center + Quaternion.Euler(0, 0, randomAngle) * Vector3.up * randomRadius;
            }
            
            // Check if this position is too close to existing ones
            bool tooClose = false;
            foreach (Vector3 existingPos in spawnPositions) {
                if (Vector3.Distance(pos, existingPos) < minSpawnDistance) {
                    tooClose = true;
                    break;
                }
            }
            
            // Check for collisions with existing objects
            Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, minSpawnDistance / 2f);
            if (colliders.Length > 0) {
                tooClose = true;
            }
            
            if (!tooClose) {
                spawnPositions.Add(pos);
                CreateIvyRoot(pos, center);
                successfulSpawns++;
            }
        }
        
        // If we couldn't spawn all requested roots, log a warning
        if (successfulSpawns < rootCount) {
            Debug.LogWarning($"Could only spawn {successfulSpawns}/{rootCount} ivy roots due to space constraints.");
        }
    }
    
    void CreateIvyRoot(Vector3 position, Vector3 center) {
        GameObject root = Instantiate(ivyRootPrefab, position, Quaternion.identity);
        
        // Orient to grow toward center
        root.transform.up = (center - position).normalized;
        
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
    
    // Helper method to shuffle a list
    void ShuffleList<T>(List<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
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
    
    // Visual debugging
    void OnDrawGizmosSelected() {
        // Draw spawn radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        // Draw minimum spacing radius if in runtime
        if (Application.isPlaying && spawnPositions.Count > 0) {
            Gizmos.color = Color.yellow;
            foreach (Vector3 pos in spawnPositions) {
                Gizmos.DrawWireSphere(pos, minSpawnDistance / 2f);
            }
        }
    }
} 