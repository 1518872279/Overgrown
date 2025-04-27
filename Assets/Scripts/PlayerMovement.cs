using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour {
    public float maxSpeed=5f, acceleration=20f, deceleration=25f;
    public ParticleSystem moveParticles;
    Rigidbody2D rb; Vector2 input;
    void Awake()=>rb=GetComponent<Rigidbody2D>();
    void Update(){ input=new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")).normalized;
        if(moveParticles != null) {
            if(input.magnitude>.1f&&!moveParticles.isPlaying) moveParticles.Play();
            else if(input.magnitude<=.1f&&moveParticles.isPlaying) moveParticles.Stop();
        }
    }
    void FixedUpdate(){
        Vector2 target=input*maxSpeed;
        rb.velocity=Vector2.MoveTowards(rb.velocity,target,(input.magnitude>.1f?acceleration:deceleration)*Time.fixedDeltaTime);
    }
} 