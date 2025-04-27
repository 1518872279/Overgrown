using UnityEngine;

public class IvySway : MonoBehaviour {
    public float swaySpeed = 2f;
    public float swayAngle = 15f;
    private float baseRot;
    
    // Performance optimizations
    [Tooltip("Only update sway every N frames")]
    public int updateInterval = 2;
    [Tooltip("Disable sway when off-screen")]
    public bool disableOffscreen = true;
    
    private int frameCounter = 0;
    private bool isVisible = true;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private int siblingIndex;

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        siblingIndex = transform.GetSiblingIndex();
    }
    
    void OnEnable() {
        baseRot = transform.eulerAngles.z;
        isVisible = true;
        
        // Optimization: Randomize frame counters to distribute processing
        frameCounter = Random.Range(0, updateInterval);
    }

    void Update() {
        // Skip updates based on interval for performance
        frameCounter++;
        if (frameCounter < updateInterval) {
            return;
        }
        frameCounter = 0;
        
        // Skip if off-screen
        if (disableOffscreen && spriteRenderer != null && mainCamera != null) {
            if (IsVisibleToCamera(mainCamera, spriteRenderer)) {
                if (!isVisible) {
                    isVisible = true;
                }
            } else {
                if (isVisible) {
                    isVisible = false;
                    return; // Skip processing entirely
                }
                return;
            }
        }
        
        // Apply sway using cached sibling index (doesn't change)
        float offset = Mathf.Sin(Time.time * swaySpeed + siblingIndex) * swayAngle;
        transform.rotation = Quaternion.Euler(0, 0, baseRot + offset);
    }
    
    bool IsVisibleToCamera(Camera camera, Renderer renderer) {
        if (renderer == null || camera == null) return false;
        
        // Simple distance check as optimization
        float distanceToCamera = Vector3.Distance(transform.position, camera.transform.position);
        if (distanceToCamera > 30f) return false;
        
        // Quick bounds check
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
} 