using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCover : MonoBehaviour {

    public bool Attackable;
    public int PX;
    public int PY;

	// Use this for initialization
	public void Awake () {
        Attackable = false;
        PX = -1;
        PY = -1;	
	}

    public void Start()
    {

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
