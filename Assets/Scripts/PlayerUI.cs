using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour {
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI finesseText;

    void Update() {
        levelText.text = "Level: " + XPManager.Instance.level;
        finesseText.text = "Finesse: " + Mathf.RoundToInt(PlayerStats.Instance.Finesse);
    }
} 