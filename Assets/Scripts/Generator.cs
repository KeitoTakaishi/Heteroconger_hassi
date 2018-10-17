using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour {

    #region variable
    private List<GameObject> Tinanagos;
    [SerializeField]
    private int _num = 10;
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
            Tinanagos.Add(Instantiate(Resources.Load("Tinanago"), new Vector3(-1.5f + i * 0.3f, 0.0f, 0.0f), Quaternion.identity) as GameObject);
        }

    }
	void Update () {
		
	}
}
