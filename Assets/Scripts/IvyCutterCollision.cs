using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public class IvyCutterCollision : MonoBehaviour {
    private CircleCollider2D cutterCollider;
    
    void Awake() {
        cutterCollider = GetComponent<CircleCollider2D>();
    }
    
    void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("IvySegment")) {
            IvySegment seg = other.GetComponent<IvySegment>();
            if (seg != null && PlayerStats.Instance != null) {
                seg.TakeDamage(PlayerStats.Instance.Finesse * Time.deltaTime);
            }
        }
    }
    
    // More efficient spatial query approach
    void FixedUpdate() {
        if (SpatialPartitionManager.Instance != null && PlayerStats.Instance != null) {
            // Create a rect around the cutter
            Vector2 pos = transform.position;
            float radius = cutterCollider.radius;
            Rect queryArea = new Rect(pos.x - radius, pos.y - radius, radius * 2, radius * 2);
            
            // Query segments in this area - now returns a List<IvySegment> instead of IEnumerable
            var segments = SpatialPartitionManager.Instance.QueryArea(queryArea);
            for (int i = 0; i < segments.Count; i++) {
                var seg = segments[i];
                // Additional null check to avoid any NullReferenceException
                if (seg != null && seg.gameObject.activeInHierarchy) {
                    // Check if actually within circle
                    if (Vector2.Distance(pos, seg.transform.position) <= radius) {
                        seg.TakeDamage(PlayerStats.Instance.Finesse * Time.deltaTime);
                    }
                }
            }
        }
    }
} 