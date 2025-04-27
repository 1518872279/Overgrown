using UnityEngine;

public enum StatType { CutSpeed, CutRange, ComboWindow }

[CreateAssetMenu(menuName = "Items/CutterItem")]
public class CutterItem : ScriptableObject {
    public string itemName;
    public Sprite icon;
    public StatType boosts;
    public float amount;
} 