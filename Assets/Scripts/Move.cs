using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class Move : MonoBehaviour {

    #region variable
    private GameObject joint;
    private float t;
    private float ThetaCoef = 100.0f;
    [SerializeField]
    private float _amp = 0.01f;
    #endregion

    #region property
    public float Amp
    {
        get { return _amp; }
        set { _amp = value; }
    }
    #endregion

    void Start () {
        joint = this.transform.GetChild(0).GetChild(0).gameObject;
        setTarget();
    }
	
	void Update () {
        if (Input.GetMouseButton(0)){
            Dive();
        }
	}

    void setTarget()
    {
        var ccdik = joint.GetComponent<CCDIK>().solver;
        ccdik.target = GameObject.Find("target").transform;
    } 

    void Dive()
    {
        if (joint.transform.position.y > -2.0f)
        {
            joint.transform.position -= new Vector3(0.0f, 0.05f, 0.0f);
        }
    }
}
