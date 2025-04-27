using UnityEngine;
using System.Collections.Generic;

public class SpatialPartitionManager : MonoBehaviour {
    public static SpatialPartitionManager Instance;
    public float cellSize = 2f;
    private Dictionary<Vector2Int, List<IvySegment>> buckets = new();
    private readonly object lockObject = new object();

    void Awake() {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    Vector2Int Hash(Vector2 pos) {
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / cellSize),
            Mathf.FloorToInt(pos.y / cellSize)
        );
    }

    public void Insert(IvySegment seg) {
        var key = Hash(seg.transform.position);
        lock (lockObject) {
            if (!buckets.ContainsKey(key)) buckets[key] = new List<IvySegment>();
            buckets[key].Add(seg);
        }
    }

    public void Remove(IvySegment seg) {
        var key = Hash(seg.transform.position);
        lock (lockObject) {
            if (buckets.TryGetValue(key, out var list)) list.Remove(seg);
        }
    }

    public List<IvySegment> QueryArea(Rect area) {
        List<IvySegment> results = new List<IvySegment>();
        
        int minX = Mathf.FloorToInt(area.xMin / cellSize);
        int maxX = Mathf.FloorToInt(area.xMax / cellSize);
        int minY = Mathf.FloorToInt(area.yMin / cellSize);
        int maxY = Mathf.FloorToInt(area.yMax / cellSize);
        
        lock (lockObject) {
            for (int x = minX; x <= maxX; x++) {
                for (int y = minY; y <= maxY; y++) {
                    var key = new Vector2Int(x, y);
                    if (buckets.TryGetValue(key, out var list)) {
                        results.AddRange(new List<IvySegment>(list));
                    }
                }
            }
        }
        
        return results;
    }
} 