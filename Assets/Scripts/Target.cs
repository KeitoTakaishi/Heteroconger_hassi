using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {

	void Start () {
		
	}
	
	void Update () {
        float t = Time.frameCount % 360;
        t = t * Mathf.PI / 180.0f;
        this.transform.position = new Vector3(Mathf.Cos(t), transform.position.y, Mathf.Sin(t));
	}
}
