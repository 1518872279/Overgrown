using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class IvyCutterCollision : MonoBehaviour {
    void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("IvySegment")) {
            var seg = other.GetComponent<IvySegment>();
            if (seg != null && PlayerStats.Instance != null) {
                seg.TakeDamage(PlayerStats.Instance.Finesse * Time.deltaTime);
            }
        }
    }
} 