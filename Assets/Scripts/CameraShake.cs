using UnityEngine; using System.Collections;
public class CameraShake : MonoBehaviour {
    public static CameraShake Instance;
    void Awake(){ if(Instance==null) Instance=this; else Destroy(gameObject); }
    public void Shake(float dur,float mag)=>StartCoroutine(DoShake(dur,mag));
    IEnumerator DoShake(float d,float m){ Vector3 orig=transform.localPosition; float t=0;
        while(t<d){ transform.localPosition=orig+(Vector3)Random.insideUnitCircle*m; t+=Time.deltaTime; yield return null; }
        transform.localPosition=orig;
    }
} 