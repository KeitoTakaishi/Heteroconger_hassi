/*
 * setup
 * num:個体数
 * Range:生息範囲
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour {

    #region variable
    private List<GameObject> Tinanagos;
    [SerializeField]
    private int _num = 10;
    [SerializeField]
    private float Range = 4.0f;
    public GameObject pref;
    #endregion


    #region property
    public int Num
    {
        get { return _num; }
        set { _num = value; }
    }
    #endregion

    private void OnEnable()
    {
    }

    void Start () {
        Tinanagos = new List<GameObject>();
        for (int i = 0; i <= Num; i++)
        {
            var p = Range * Random.insideUnitCircle + new Vector2(0.0f, 0.5f);
            // Tinanagos.Add(Instantiate(pref, new Vector3(-1.5f + i * 0.3f, 0.0f, p.y), Quaternion.identity) as GameObject);
            Tinanagos.Add(Instantiate(pref, new Vector3(p.x, 0.0f, p.y), Quaternion.identity) as GameObject);
            Tinanagos[i].gameObject.name = "tinanago" + i;
        }

    }
	void Update () {
		
	}
}
