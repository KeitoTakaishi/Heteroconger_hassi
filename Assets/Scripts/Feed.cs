using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feed : MonoBehaviour {

    private ParticleSystem ps;
    private bool isFeeding = false;
    private int control = 0;


    private void Awake()
    {
        ps = this.GetComponent<ParticleSystem>();
    }
    void Start () {
	}
	
	void Update () {
        isFeeding = (Input.GetMouseButton(1) == true) ? true:false;

        if (isFeeding)
        {
            if (control == 0)
            {
                ps.Play();
                control++;
            }
            var emitterPos = Input.mousePosition;
            emitterPos.z = 2.5f;
            emitterPos = Camera.main.ScreenToWorldPoint(emitterPos);
            this.transform.position = emitterPos;
        }else
        {
            control = 0;
            ps.Stop();
        }     
    }
}
