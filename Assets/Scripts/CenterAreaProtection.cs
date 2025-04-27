using UnityEngine;

public class CenterAreaProtection : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("IvySegment")) {
            Debug.Log("Game Over: Center Covered");
            // TODO: Trigger end-of-run logic
        }
    }
} 