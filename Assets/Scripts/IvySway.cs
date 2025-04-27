using UnityEngine;

public class IvySway : MonoBehaviour {
    public float swaySpeed = 2f;
    public float swayAngle = 15f;
    private float baseRot;

    void Start() {
        baseRot = transform.eulerAngles.z;
    }

    void Update() {
        float offset = Mathf.Sin(Time.time * swaySpeed + transform.GetSiblingIndex()) * swayAngle;
        transform.rotation = Quaternion.Euler(0, 0, baseRot + offset);
    }
} 