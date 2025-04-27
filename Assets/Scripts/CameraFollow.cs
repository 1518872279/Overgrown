using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public Transform target; public float smoothSpeed=0.125f; public Vector3 offset;
    void LateUpdate(){ if(target) transform.position=Vector3.Lerp(transform.position,target.position+offset,smoothSpeed); }
} 