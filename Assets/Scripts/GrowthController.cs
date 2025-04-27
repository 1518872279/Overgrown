using UnityEngine;

public class GrowthController : MonoBehaviour {
    public static GrowthController Instance;
    [Tooltip("Multiplier applied to all IvyNode growth intervals")] public float speedMultiplier = 1f;
    void Awake(){ if(Instance==null) Instance=this; else Destroy(gameObject);}    
    public float GetInterval(float baseInterval){ return baseInterval / speedMultiplier; }
} 