using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour {
    [Header("Movement")]
    public float maxSpeed = 5f;
    public float acceleration = 20f; 
    public float deceleration = 25f;
    
    [Header("Visual Effects")]
    public ParticleSystem moveParticles;
    [Tooltip("Should the sprite rotate to face movement direction")]
    public bool rotateSprite = true;
    [Tooltip("Use sprite flipping instead of rotation (for side-view sprites)")]
    public bool useFlipInstead = false;
    [Tooltip("Sprite to flip (if different from this object)")]
    public SpriteRenderer spriteToFlip;
    
    [Header("Rotation Settings")]
    [Tooltip("Offset angle in degrees (to correct sprite orientation)")]
    public float rotationOffset = -90f;
    [Tooltip("Invert the rotation direction")]
    public bool invertRotation = false;
    [Tooltip("Smooth rotation speed (0 for instant)")]
    public float rotationSpeed = 10f;
    
    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 lastNonZeroDirection = Vector2.right; // Default facing right
    private SpriteRenderer spriteRenderer;
    private float targetAngle;
    
    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        // Find sprite renderer if not specified
        if (spriteToFlip == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            spriteToFlip = spriteRenderer;
        } else {
            spriteRenderer = spriteToFlip;
        }
    }
    
    void Update() {
        // Get movement input
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        
        // Handle particle effects
        if (moveParticles != null) {
            if (input.magnitude > .1f && !moveParticles.isPlaying)
                moveParticles.Play();
            else if (input.magnitude <= .1f && moveParticles.isPlaying)
                moveParticles.Stop();
        }
        
        // Update facing direction if moving
        if (rotateSprite && input.magnitude > 0.1f) {
            // Save last direction for when player stops moving
            lastNonZeroDirection = input.normalized;
            
            if (useFlipInstead) {
                // Only use horizontal flip
                if (spriteRenderer != null) {
                    // Invert if needed
                    bool shouldFlip = invertRotation ? (input.x > 0) : (input.x < 0);
                    spriteRenderer.flipX = shouldFlip;
                }
            } else {
                // Calculate the angle based on movement direction
                Vector2 direction = invertRotation ? -input.normalized : input.normalized;
                targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                
                // Apply the offset
                targetAngle += rotationOffset;
                
                // Apply rotation (smoothed if rotation speed > 0)
                if (rotationSpeed > 0) {
                    // Smooth rotation
                    float currentAngle = transform.rotation.eulerAngles.z;
                    // Ensure we rotate the shortest way
                    float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
                    float newAngle = currentAngle + (angleDifference * rotationSpeed * Time.deltaTime);
                    transform.rotation = Quaternion.Euler(0, 0, newAngle);
                } else {
                    // Instant rotation
                    transform.rotation = Quaternion.Euler(0, 0, targetAngle);
                }
            }
        }
    }
    
    void FixedUpdate() {
        // Apply movement
        Vector2 target = input * maxSpeed;
        rb.velocity = Vector2.MoveTowards(
            rb.velocity,
            target,
            (input.magnitude > .1f ? acceleration : deceleration) * Time.fixedDeltaTime
        );
    }
    
    // For debugging - draws the direction the player is facing
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Vector3 direction = Quaternion.Euler(0, 0, targetAngle) * Vector3.right;
        Gizmos.DrawRay(transform.position, direction);
    }
} 