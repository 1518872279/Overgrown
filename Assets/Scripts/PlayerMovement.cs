using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour {
    public float maxSpeed = 5f;
    public float acceleration = 20f;
    public float deceleration = 25f;
    public ParticleSystem moveParticles;

    private Rigidbody2D rb;
    private Vector2 input;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update() {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        
        // Only use particles if assigned
        if (moveParticles != null) {
            if (input.magnitude > 0.1f && !moveParticles.isPlaying) 
                moveParticles.Play();
            else if (input.magnitude <= 0.1f && moveParticles.isPlaying) 
                moveParticles.Stop();
        }
    }

    void FixedUpdate() {
        Vector2 targetVel = input * maxSpeed;
        rb.velocity = Vector2.MoveTowards(rb.velocity, targetVel,
            (input.magnitude > 0.1f ? acceleration : deceleration) * Time.fixedDeltaTime);
    }
} 