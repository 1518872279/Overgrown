using UnityEngine;

public class XPManager : MonoBehaviour {
    public static XPManager Instance;
    public float xp = 0f;
    public int level = 1;
    public int upgradePoints = 0;
    public AnimationCurve xpCurve; // define xpToNextLevel: time vs xp needed

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddXP(float amount) {
        xp += amount;
        CheckLevelUp();
    }

    void CheckLevelUp() {
        float needed = xpCurve.Evaluate(level);
        while (xp >= needed) {
            xp -= needed;
            level++;
            upgradePoints++;
            // TODO: trigger level-up UI
            needed = xpCurve.Evaluate(level);
        }
    }
} 