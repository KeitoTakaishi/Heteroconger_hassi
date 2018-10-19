
/*
 * setup
 * targetとなるgameobjectをsetupする
 */
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
    [SerializeField]
    GameObject target;
    private Vector3 posBias;//targetの座標に対するバイアス
    private float phaseBias;//位相バイアス
    private float theta;
    private Vector3 bufPos;
    public GameObject buf;
    bool isCoRoutine = false;
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
        Initialize();
    }
	
	void Update () {
        target.transform.position = TargetMove(target.transform.position);
        
       // if (Input.GetMouseButton(0)){
            //    var distance = DetectionTouch(); 
            //    Dive(distance);

            bool trigger=false;
            Debug.Log(Time.frameCount % 500);
            if (Time.frameCount % 500 == 250)
            {
                trigger = Trigger();
            }

        if (!isCoRoutine)
        {
            if (trigger)
            {
                StartCoroutine("SuddenlyMove");
                this.transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color = Color.yellow;
            }
            else
            {
                this.transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color = Color.white;
            }
        }
         //}
        
	}
    #endregion

    #region func

    private void Initialize()
    {
        posBias = Random.insideUnitSphere * 0.2f;
        phaseBias = Random.RandomRange(0, 360.0f);
        joint = this.transform.GetChild(0).GetChild(0).gameObject;
        EachSetTarget();
        bufPos = target.transform.position;
        buf = Instantiate(buf, target.transform.position, target.transform.rotation) as GameObject;
    }


    //targetが共通の場合
    void CommonSetTarget()
    {
        var ccdik = joint.GetComponent<CCDIK>().solver;
        ccdik.target = target.transform;
    } 

    //それぞれの個体がtargetを持っている場合のtargetのセットアップ
    void EachSetTarget()
    {
        var ccdik = joint.GetComponent<CCDIK>().solver;
        target.transform.position += posBias;
        ccdik.target = target.transform;
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



    //targetの移動
    private Vector3 TargetMove(Vector3 target)
    {

        theta = ((phaseBias+Time.frameCount) % 360.0f) * Mathf.Deg2Rad;
        var temp = Amp * Mathf.Sin(theta);
        if (!isCoRoutine){
            target.y += temp;
        }
        else{

        }
        bufPos.y += temp;
        buf.transform.position = bufPos;
        return target;
    }

    private bool Trigger()
    {
        bool trigger = false;
        var threshould = 0.5f;
        trigger = (Random.RandomRange(0.0f, 1.0f) > threshould) ? true : false;
        return trigger;
    }

    //void SuddenlyMove()
    IEnumerator SuddenlyMove()
    {
        isCoRoutine = true;
        var t = 0.0f;
        var fromPos = target.transform.position;
        var toPos = target.transform.position + Vector3.one * Random.RandomRange(-1.5f, 1.5f);
        for(int i = 0; i < 100; i++)
        {
            target.transform.position = Vector3.Lerp(fromPos, toPos, t);
            t += 1.0f/100.0f;
            yield return null;
        }
        target.GetComponent<MeshRenderer>().material.color = Color.blue;
        t = 0.0f;
        fromPos = target.transform.position;
        toPos = bufPos;
        target.GetComponent<MeshRenderer>().material.color = Color.yellow;
        for (int i = 0; i < 100; i++)
        {
            toPos = bufPos;
            target.transform.position = Vector3.Lerp(fromPos, toPos, t);
            t += 1.0f / 100.0f;
            yield return null;
        }
        target.GetComponent<MeshRenderer>().material.color = Color.red;
        isCoRoutine = false;
    }

    #endregion
}
