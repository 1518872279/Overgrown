using UnityEngine;

public class GameInitializer : MonoBehaviour {
    // References to singleton managers to ensure they are initialized first
    public XPManager xpManager;
    public PlayerStats playerStats;

    void Awake() {
        // This will ensure XPManager initializes before PlayerStats
        if (xpManager == null) {
            Debug.LogWarning("XPManager reference not set in GameInitializer");
        }
        
        if (playerStats == null) {
            Debug.LogWarning("PlayerStats reference not set in GameInitializer");
        }
    }
} 