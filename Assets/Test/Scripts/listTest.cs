using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class listTest : MonoBehaviour {

    [Serializable]
    public struct Lives
    {
        [Range(0f, 60f)]
        public float minLife;
        [Range(0f, 60f)]
        public float maxLife;
    }
    [SerializeField]
    List<Lives> lives = new List<Lives>();
   
    void Start () {
        

        Debug.Log("max" + lives[0].maxLife);
        Debug.Log("max"+lives[0].maxLife);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
