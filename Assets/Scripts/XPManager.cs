using UnityEngine;

public class XPManager : MonoBehaviour {
    public static XPManager Instance;
    public float xp;
    public int level = 1;
    public int upgradePoints;
    public AnimationCurve xpCurve;

    void Awake() {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    public void AddXP(float amt) {
        xp += amt;
        CheckLevelUp();
    }

    void CheckLevelUp() {
        float need = xpCurve.Evaluate(level);
        while (xp >= need) {
            xp -= need;
            level++;
            upgradePoints++;
            need = xpCurve.Evaluate(level);
        }
    }
} 