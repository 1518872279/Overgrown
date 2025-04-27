using UnityEngine;

public class CenterAreaProtection : MonoBehaviour {
    public GameOverHandler gameOverHandler;
    
    void Awake() {
        // Set radius to match the GrowthManager if it exists
        if (GrowthManager.Instance != null) {
            var circle = GetComponent<CircleCollider2D>();
            if (circle != null) {
                circle.radius = GrowthManager.Instance.centerRadius;
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("IvySegment")) {
            Debug.Log("Game Over: Center Covered");
            // Trigger end-of-run logic
            if (gameOverHandler != null) {
                gameOverHandler.TriggerGameOver("Center area has been compromised!");
            }
        }
    }
} 