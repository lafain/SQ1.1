using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Placable : MonoBehaviour {
    
    public bool Alive;
    public int MaxHealth;
    public int cHealth;
    public int MaxAP;
    public int cAP;
    public int PX;
    public int PY;
    public int TilesMovedPerAP; //(Speed) - 2: move two tiles per ap point
    public int PrimaryWeaponType;// 1 - short edged, 2 - long edged, 3 - ranged
    public int PrimaryWeaponMean;
    public int PrimaryWeaponSD; // Standard Deviation
    public bool TurnDone;
    public bool PlayerControlled;
    public List<Tile> PossibleMoves;
    public string Name;
    public bool Attackable;
    public int MeanDamage;
    public int DamageSD;
    public int Level;
    public int MaxArmor;
    public int cArmor;
    public int TeamTag; // 1 for heros, 2 for bads

    public void InitPlacable()
    {
        TeamTag = 0;
        TilesMovedPerAP = 2;

        PrimaryWeaponType = 1;
        PrimaryWeaponMean = 50;
        PrimaryWeaponSD = 1;
        TurnDone = true;
        PlayerControlled = false;
        PossibleMoves = new List<Tile>();
        Name = "nudfllo";
    }
    
    //AI attack and move
    public IEnumerator ExecuteTurn()
    {
        G.GM.GS = GameManager.GameState.ExecutingTurn;
        G.GM.CurrentIndicator.transform.position = G.GM.CurrentTurnActor.transform.position + new Vector3(0, 1.5f, 0);
        G.GM.ToggleAttack(true);
        if(G.GM.Hero1.GetComponent<Placable>().Attackable && cAP >= 4)
        {
            Debug.Log("Hero is attackable");
            StartCoroutine(G.GM.AttackTargetClicked(G.GM.Hero1));
        }
        G.GM.HighlightAvailableTiles(PX, PY, cAP, 123456789, 123456789, 0);
        yield return new WaitForSecondsRealtime(2);
        if (PossibleMoves.Count != 0)
        {
            Tile TargetMove = PossibleMoves[Random.Range(0, PossibleMoves.Count)];
            transform.position = new Vector3(TargetMove.transform.position.x, TargetMove.PlaceOffset, TargetMove.transform.position.z);
            TargetMove.cPlacableTag = 2;
            TargetMove.cPlacable = this;
            G.GM.TileList[PX][PY].GetComponent<Tile>().cPlacableTag = 0;
            G.GM.TileList[PX][PY].GetComponent<Tile>().cPlacable = null;
            PX = TargetMove.PX;
            PY = TargetMove.PY;
            G.GM.CurrentIndicator.transform.position = G.GM.CurrentTurnActor.transform.position + new Vector3(0, 1.5f, 0);
            G.GM.Camera1.gameObject.transform.LookAt(G.GM.CurrentTurnActor.transform);
            G.GM.Camera1.transform.position = G.GM.CurrentTurnActor.transform.position + G.GM.CameraOffset;
        }

        if (G.GM.Hero1.GetComponent<Placable>().Attackable && cAP >= 4)
        {
            G.GM.AttackTargetClicked(G.GM.Hero1);
        }

        TurnDone = true;
        G.GM.ToggleMoving(false);

        yield return new WaitForSecondsRealtime(2);
        G.GM.GS = GameManager.GameState.Play;
    }
    

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //probably not use this
    public void SetLevel(int _Level)
    {
        Level = _Level;
        MeanDamage = 10 + Mathf.RoundToInt(Level * 1.5f);
        DamageSD = 1;
        MaxAP = MeanDamage;
        cAP = MaxAP;
    }
}
