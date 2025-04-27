using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour {
    public static PoolManager Instance;
    public GameObject branchPrefab;
    public GameObject leafPrefab;
    public int branchPoolSize = 100;
    public int leafPoolSize = 200;

    private Queue<GameObject> branchPool = new Queue<GameObject>();
    private Queue<GameObject> leafPool = new Queue<GameObject>();

    void Awake() {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        // Prewarm pools
        for (int i = 0; i < branchPoolSize; i++) {
            var obj = Instantiate(branchPrefab);
            obj.SetActive(false);
            branchPool.Enqueue(obj);
        }
        for (int i = 0; i < leafPoolSize; i++) {
            var obj = Instantiate(leafPrefab);
            obj.SetActive(false);
            leafPool.Enqueue(obj);
        }
    }

    public GameObject GetBranch() {
        if (branchPool.Count > 0) {
            var obj = branchPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        // optionally expand pool
        return Instantiate(branchPrefab);
    }
    
    public void ReleaseBranch(GameObject obj) {
        obj.SetActive(false);
        branchPool.Enqueue(obj);
    }
    
    public GameObject GetLeaf() {
        if (leafPool.Count > 0) {
            var obj = leafPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(leafPrefab);
    }
    
    public void ReleaseLeaf(GameObject obj) {
        obj.SetActive(false);
        leafPool.Enqueue(obj);
    }
} 