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


    #region unity func
    void Start () {
        joint = this.transform.GetChild(0).GetChild(0).gameObject;
        EachSetTarget();
    }
	
	void Update () {
        if (Input.GetMouseButton(0)){
            var distance = DetectionTouch(); 
            Dive(distance);
        }
        
	}
    #endregion

    #region func

    //targetが共通の場合
    void CommonSetTarget()
    {
        var ccdik = joint.GetComponent<CCDIK>().solver;
        ccdik.target = GameObject.Find("target").transform;
    } 
    //それぞれの個体がtargetを持っている場合
    void EachSetTarget()
    {
        var ccdik = joint.GetComponent<CCDIK>().solver;
        var eachTarget = this.transform.GetChild(0).GetChild(1).gameObject;
        ccdik.target = eachTarget.transform;
    }

    //沈むモーションについての関数
    void Dive(float dis)
    {
        float vel = 0.01f;
        float maxCoef = 5.0f;
        float velCoef = 0.0f;

        var w = Screen.width;
        var h = Screen.height;
        var maxDis = Mathf.Sqrt(w * w + h * h);

        var weight = 1.0f;
        velCoef = (1.0f - dis / maxDis) * weight;

        //Debug.Log(this.gameObject.name+ ":" + velCoef);
        vel *= velCoef; 
  

        if (joint.transform.position.y > -2.0f)
        {
            joint.transform.position -= new Vector3(0.0f, vel, 0.0f);
        }
    }
    //mouseクリック位置からの距離を返す
    //共通のメソッドにする必要あり
    //----global,local変数の使いわけについて
    float DetectionTouch()
    {
        var touchPos = Input.mousePosition;
        touchPos.z = 0.0f;
        var pos = this.transform.position;
        pos.z = 0.0f;
        pos = Camera.main.WorldToScreenPoint(pos);
        var dis = (pos - touchPos).magnitude;

        return dis;
    }


    //指定座標から近い順にオブジェクトをソート
    
    #endregion
}
