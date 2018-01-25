using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
    
    public int PX;
    public int PY;
    public Placable cPlacable;
    public int cPlacableTag; //1 - Hero, 2 - Bad
    //going through water costs 1 additional AP
    public int Type; //1 - water, 2 - rock, 3 - grass  
    public float HeightOffset;
    public float PlaceOffset;
    Color32 OriginalColor;
    public bool Highlighted;
    public bool Attackable;
    public int APCostToMove;
    //used to highlight square
    public GameObject TileCoverGO;

    public void InitTile(GameObject _TileCoverGO)
    {
        Attackable = false;
        TileCoverGO = _TileCoverGO;
        TileCoverGO.SetActive(false);
        OriginalColor = GetComponent<Renderer>().material.color;
        Highlighted = false;
        APCostToMove = -1;
        //outline = GO.GetComponent<Outline>();
    }

    public void ToggleHighlight(bool Highlight)
    {
        if (Highlight)
        {
            if (!Highlighted)
            {
                Highlighted = true;
                //GO.GetComponent<Renderer>().material.color = new Color(GO.GetComponent<Renderer>().material.color.r + 0.05f, GO.GetComponent<Renderer>().material.color.b + 0.05f, GO.GetComponent<Renderer>().material.color.g + 0.05f, GO.GetComponent<Renderer>().material.color.a);
                TileCoverGO.SetActive(true);
            }
        }
        else
        {
            Highlighted = false;
            //GO.GetComponent<Renderer>().material.color = OriginalColor;
            TileCoverGO.SetActive(false);
            APCostToMove = -1;
        }
    }
   
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ToggleAttackable(bool _Attackable)
    {
        Attackable = _Attackable;
        TileCoverGO.GetComponent<TileCover>().Attackable = Attackable;
        if (cPlacable != null)
        {
            cPlacable.Attackable = Attackable;
        }
        
    }
}
