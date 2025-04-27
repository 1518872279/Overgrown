using UnityEngine;

public class IvySway : MonoBehaviour {
    public float swaySpeed = 2f;
    public float swayAngle = 10f;
    private float baseAngle;

    void Start() {
        baseAngle = transform.eulerAngles.z;
    }

    void Update() {
        float phase = transform.GetSiblingIndex() * 0.5f;
        float offset = Mathf.Sin(Time.time * swaySpeed + phase) * swayAngle;
        transform.rotation = Quaternion.Euler(0, 0, baseAngle + offset);
    }
} 