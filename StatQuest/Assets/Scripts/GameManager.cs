using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public Vector3 CameraOffset;
    public GameObject TilePrefab;
    public GameObject TileCoverPrefab;
    public List<List<GameObject>> TileList;
    int TilesPerSide;
    
    public GameObject Hero1Prefab;
    public GameObject Hero1;

    public GameObject Bad1Prefab;
    public List<GameObject> BadList;
    public int MaxBads;

    public Material Grass1;
    public Material Rock1;
    public Material Water1;

    public Camera Camera1;
    public Camera PlayerPortraitCamera;

    public GameObject CurrentTurnActor;
    public GameObject CurrentIndicator;
    public List<GameObject> TurnList;

    public GameState GS;

    public struct ZValuePair
    {
        public double ZScore;
        public double Probability;

        public ZValuePair(double _ZScore, double _Probability)
        {
            ZScore = _ZScore;
            Probability = _Probability;
        }
    }

    List<ZValuePair> ZList;
    
    public enum GameState
    {
        Play, Pause, ExecutingTurn
    }
    
    // Use this for initialization
    void Start()
    {
        G.GM = this;

        InitZList();

        CameraOffset = new Vector3(4f, 6.0f, 4f);
        TilesPerSide = 15;
        InitTiles();
        InitBackgroundTiles();
        InitHero();
        InitBads();
        TurnList = new List<GameObject>();

        Camera1.gameObject.transform.position = Hero1.transform.position + CameraOffset;
        Camera1.gameObject.transform.LookAt(Hero1.transform);

        TurnList.Add(Hero1); //add other heros later
        foreach (GameObject cBad in BadList)
        {
            TurnList.Add(cBad);
        }
        CurrentTurnActor = Hero1;
        CurrentTurnActor.GetComponent<Placable>().TurnDone = false;
        TileList[CurrentTurnActor.GetComponent<Placable>().PX][CurrentTurnActor.GetComponent<Placable>().PY].GetComponent<Tile>().TileCoverGO.SetActive(true);
        GS = GameState.Play;
        CurrentIndicator.transform.position = CurrentTurnActor.transform.position + new Vector3(0, 1.5f, 0);

    }

    // Update is called once per frame
    void Update()
    {

        if (GS == GameState.Play)
        {
            PlayerPortraitCamera.transform.position = Hero1.transform.position + new Vector3(0.5f, 0.5f, 0.5f);
            PlayerPortraitCamera.transform.LookAt(Hero1.transform);

            if (CurrentTurnActor.GetComponent<Placable>().TurnDone == true)
            {
                Debug.Log(TurnList.Count.ToString());
                //Closeout last CTActor
                TileList[CurrentTurnActor.GetComponent<Placable>().PX][CurrentTurnActor.GetComponent<Placable>().PY].GetComponent<Tile>().TileCoverGO.SetActive(false);

                if(BadList.Count == 0)
                {
                    G.UIM.DisplayNewInfoText("Enemies Defeated! You Win!", true);
                    InitBads();
                }

                //Chose CTActor
                int cTIndex = TurnList.IndexOf(CurrentTurnActor);

                if (TurnList.IndexOf(CurrentTurnActor) != TurnList.Count - 1)
                {
                    CurrentTurnActor = TurnList[TurnList.IndexOf(CurrentTurnActor) + 1];
                }
                else
                {
                    CurrentTurnActor = TurnList[0];
                }

                //Setup CTActor
                Camera1.transform.position = CurrentTurnActor.transform.position + CameraOffset;
                Camera1.gameObject.transform.LookAt(CurrentTurnActor.transform);
                CurrentTurnActor.GetComponent<Placable>().PossibleMoves = new List<Tile>();
                CurrentTurnActor.GetComponent<Placable>().TurnDone = false;
                CurrentTurnActor.GetComponent<Placable>().cAP = CurrentTurnActor.GetComponent<Placable>().MaxAP;
                TileList[CurrentTurnActor.GetComponent<Placable>().PX][CurrentTurnActor.GetComponent<Placable>().PY].GetComponent<Tile>().TileCoverGO.SetActive(true);

                if (CurrentTurnActor.GetComponent<Placable>().PlayerControlled)
                {
                    G.UIM.TogglePlayerMenu(true);
                }
                else
                {
                    G.UIM.TogglePlayerMenu(false);
                }
                Debug.Log("Current Turn: " + CurrentTurnActor.name.ToString());

            }

            //Control AI or return control to player
            if (CurrentTurnActor.GetComponent<Placable>().PlayerControlled == false)
            {
                StartCoroutine(CurrentTurnActor.GetComponent<Placable>().ExecuteTurn());
            }
            else
            {
                G.UIM.UpdateHeroCAP(Hero1.GetComponent<Placable>().cAP);
            }

            //End turn if CurrentTurn is out of AP
            //if (CurrentTurnActor.GetComponent<Placable>().cAP == 0)
            //{
            //    G.UIM.EndTurnClicked();
            //}
        }
    }

    public string GetHoveredMoveCost(GameObject HoveredObject)
    {
        string MoveCost = "X";
        for (int x = 0; x < TilesPerSide; x++)
        {
            for (int y = 0; y < TilesPerSide; y++)
            {
                if (TileList[x][y].GetComponent<Tile>().Highlighted)
                {
                    if (HoveredObject == TileList[x][y].GetComponent<Tile>().TileCoverGO)
                    {
                        MoveCost = TileList[x][y].GetComponent<Tile>().APCostToMove.ToString() + " AP";
                    }
                }
            }
        }

        return MoveCost;
    }

    public void InitTiles()
    {
        TileList = new List<List<GameObject>>();

        for (int x = 0; x < TilesPerSide; x++)
        {
            List<GameObject> cTileRow = new List<GameObject>();
            for (int y = 0; y < TilesPerSide; y++)
            {
                GameObject cTile = (GameObject)Instantiate(TilePrefab);
                cTile.GetComponent<Tile>().InitTile((GameObject) Instantiate(TileCoverPrefab));
                int TileType = Random.Range(1, 4);

                switch (TileType)
                {
                    case 1:
                        cTile.GetComponent<Tile>().HeightOffset = 1;
                        cTile.GetComponent<Tile>().PlaceOffset = 1.0f;
                        cTile.GetComponent<Renderer>().material = Water1;
                        cTile.GetComponent<Tile>().Type = 1;
                        break;
                    case 2:
                        cTile.GetComponent<Tile>().HeightOffset = 1.125f;
                        cTile.GetComponent<Tile>().PlaceOffset = 1.36f;
                        cTile.GetComponent<Renderer>().material = Rock1;
                        cTile.GetComponent<Tile>().Type = 2;
                        break;
                    case 3:
                        cTile.GetComponent<Tile>().HeightOffset = 1.25f;
                        cTile.GetComponent<Tile>().PlaceOffset = 1.6f;
                        cTile.GetComponent<Renderer>().material = Grass1;
                        cTile.GetComponent<Tile>().Type = 3;
                        break;
                    default:
                        break;

                }
                cTile.transform.position = new Vector3(x, cTile.GetComponent<Tile>().HeightOffset, y);
                cTile.transform.localScale = new Vector3(cTile.transform.localScale.x, cTile.transform.localScale.y * TileType, cTile.transform.localScale.z);
                cTile.GetComponent<Tile>().TileCoverGO.transform.position = new Vector3(x, cTile.transform.lossyScale.y / 2 + cTile.transform.position.y + 0.001f, y);


                cTile.GetComponent<Tile>().PX = x;
                cTile.GetComponent<Tile>().PY = y;
                cTile.GetComponent<Tile>().TileCoverGO.GetComponent<TileCover>().PX = x;
                cTile.GetComponent<Tile>().TileCoverGO.GetComponent<TileCover>().PY = y;
                cTileRow.Add(cTile);
            }
            TileList.Add(cTileRow);
        }
    }

    public void InitBackgroundTiles()
    {

    }

    public void HighlightAvailableTiles(int x, int y, int APDistanceAvailable, int px, int py, int MoveCost)
    {
        if (APDistanceAvailable < 0)
            return;
        
        if (TileList[x][y].GetComponent<Tile>().cPlacableTag != 1 && TileList[x][y].GetComponent<Tile>().cPlacableTag != 2)
        {
            TileList[x][y].GetComponent<Tile>().ToggleHighlight(true);

            if (TileList[x][y].GetComponent<Tile>().APCostToMove == -1)
            {
                TileList[x][y].GetComponent<Tile>().APCostToMove = MoveCost;
            }
            else
            if (TileList[x][y].GetComponent<Tile>().APCostToMove > MoveCost)
            {
                TileList[x][y].GetComponent<Tile>().APCostToMove = MoveCost;
            }

            if (!CurrentTurnActor.GetComponent<Placable>().PossibleMoves.Contains(TileList[x][y].GetComponent<Tile>()))
            {
                //Debug.Log("Adding: " + x.ToString() + "," + y.ToString());
                CurrentTurnActor.GetComponent<Placable>().PossibleMoves.Add(TileList[x][y].GetComponent<Tile>());
            }

        }

        if ((x - 1) >= 0)
        {
            if (!(x - 1 == px && y == py) && (TileList[x - 1][y].GetComponent<Tile>().cPlacableTag != 1) && (TileList[x - 1][y].GetComponent<Tile>().cPlacableTag != 2))
            {
                if (TileList[x - 1][y].GetComponent<Tile>().Type == 1)
                {
                    HighlightAvailableTiles(x - 1, y, APDistanceAvailable - CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP - 1, x, y, MoveCost + CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP + 1);
                }
                else
                {
                    HighlightAvailableTiles(x - 1, y, APDistanceAvailable - CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP, x, y, MoveCost + CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP);
                }
            }
        }

        if ((y - 1) >= 0)
        {
            if (!(x == px && y - 1 == py) && (TileList[x][y - 1].GetComponent<Tile>().cPlacableTag != 1) && (TileList[x][y - 1].GetComponent<Tile>().cPlacableTag != 2))
            {
                if (TileList[x][y - 1].GetComponent<Tile>().Type == 1)
                {
                    HighlightAvailableTiles(x, y - 1, APDistanceAvailable - CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP - 1, x, y, MoveCost + CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP + 1);
                }
                else
                {
                    HighlightAvailableTiles(x, y - 1, APDistanceAvailable - CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP, x, y, MoveCost + CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP);
                }
            }
        }

        if ((x + 1) < TilesPerSide)
        {
            if (!(x + 1 == px && y == py) && (TileList[x + 1][y].GetComponent<Tile>().cPlacableTag != 1) && (TileList[x + 1][y].GetComponent<Tile>().cPlacableTag != 2))
            {
                if (TileList[x + 1][y].GetComponent<Tile>().Type == 1)
                {
                    HighlightAvailableTiles(x + 1, y, APDistanceAvailable - CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP - 1, x, y, MoveCost + CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP + 1);
                }
                else
                {
                    HighlightAvailableTiles(x + 1, y, APDistanceAvailable - CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP, x, y, MoveCost + CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP);
                }
            }
        }

        if ((y + 1) < TilesPerSide)
        {
            if (!(x == px && y + 1 == py) && (TileList[x][y + 1].GetComponent<Tile>().cPlacableTag != 1) && (TileList[x][y + 1].GetComponent<Tile>().cPlacableTag != 2))
            {
                if (TileList[x][y + 1].GetComponent<Tile>().Type == 1)
                {
                    HighlightAvailableTiles(x, y + 1, APDistanceAvailable - CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP - 1, x, y, MoveCost + CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP + 1);
                }
                else
                {
                    HighlightAvailableTiles(x, y + 1, APDistanceAvailable - CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP, x, y, MoveCost + CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP);
                }
            }
        }
    }

    

    void InitHero()
    {
        Hero1 = (GameObject)Instantiate(Hero1Prefab);
        Hero1.GetComponent<Placable>().InitPlacable();
        Hero1.transform.position = new Vector3(TileList[0][0].transform.position.x, TileList[0][0].transform.position.y + 0.13f, TileList[0][0].transform.position.z);
        TileList[0][0].GetComponent<Tile>().cPlacable = Hero1.GetComponent<Placable>();
        Hero1.GetComponent<Placable>().SetLevel(1);
        Hero1.GetComponent<Placable>().cArmor = 20;
        Hero1.GetComponent<Placable>().MaxArmor = 20;
        G.UIM.UpdateHeroCAP(Hero1.GetComponent<Placable>().cAP);
        Hero1.GetComponent<Placable>().PlayerControlled = true;
        Hero1.GetComponent<Placable>().Name = "Niko";
        Hero1.GetComponent<Placable>().TeamTag = 1;

        bool ValidPlacement = false;

        while (!ValidPlacement)
        {

            int OX = Mathf.RoundToInt(TilesPerSide * .1f);
            int OY = Mathf.RoundToInt(TilesPerSide * .1f);
            int TX = Random.Range(-2, 2); //make percentage
            int TY = Random.Range(-2, 2);

            if ((OX + TX < TilesPerSide) && (OY + TY < TilesPerSide))
            {
                if (TileList[OX + TX][OY + TY].GetComponent<Tile>().Type != 1 && TileList[OX + TX][OY + TY].GetComponent<Tile>().cPlacable == null)
                {
                    TileList[OX + TX][OY + TY].GetComponent<Tile>().cPlacable = Hero1.GetComponent<Placable>();
                    TileList[OX + TX][OY + TY].GetComponent<Tile>().cPlacableTag = 1;
                    Hero1.transform.position = new Vector3(TileList[OX + TX][OY + TY].GetComponent<Tile>().PX, TileList[OX + TX][OY + TY].GetComponent<Tile>().PlaceOffset, TileList[OX + TX][OY + TY].GetComponent<Tile>().PY);
                    Hero1.GetComponent<Placable>().PX = OX + TX;
                    Hero1.GetComponent<Placable>().PY = OY + TY;
                    ValidPlacement = true;
                }
            }
        }
    }

    void InitBads()
    {
        MaxBads = 2;
        BadList = new List<GameObject>();

        for(int x=0; x<MaxBads; x++)
        {
            GameObject cBad = (GameObject)Instantiate(Bad1Prefab);
            cBad.GetComponent<Placable>().InitPlacable();

            cBad.GetComponent<Placable>().MaxAP = Random.Range(4,8);
            cBad.GetComponent<Placable>().cAP = cBad.GetComponent<Placable>().MaxAP;
            cBad.GetComponent<Placable>().MaxArmor = Random.Range(10, 14);
            cBad.GetComponent<Placable>().cArmor = cBad.GetComponent<Placable>().MaxArmor;
            cBad.GetComponent<Placable>().MaxHealth = 20;
            cBad.GetComponent<Placable>().cHealth = 20;
            cBad.GetComponent<Placable>().Name = "Sludger " + x.ToString();
            cBad.GetComponent<Placable>().TeamTag = 2;
            cBad.GetComponent<Placable>().SetLevel(1);
            cBad.name = "Sludger " + x.ToString();

            bool ValidPlacement = false;

            while (!ValidPlacement)
            {

                int OX = Mathf.RoundToInt(TilesPerSide * .8f);
                int OY = Mathf.RoundToInt(TilesPerSide * .8f);
                int TX = Random.Range(-5, 5); //make percentage
                int TY = Random.Range(-5, 5);

                if ((OX + TX < TilesPerSide) && (OY + TY < TilesPerSide))
                {
                    if(TileList[OX + TX][OY + TY].GetComponent<Tile>().Type != 1 && TileList[OX + TX][OY + TY].GetComponent<Tile>().cPlacable == null)
                    {
                        TileList[OX + TX][OY + TY].GetComponent<Tile>().cPlacable = cBad.GetComponent<Placable>();
                        TileList[OX + TX][OY + TY].GetComponent<Tile>().cPlacableTag = 2;

                        cBad.transform.position = new Vector3(TileList[OX + TX][OY + TY].GetComponent<Tile>().PX, TileList[OX + TX][OY + TY].GetComponent<Tile>().PlaceOffset, TileList[OX + TX][OY + TY].GetComponent<Tile>().PY);
                        cBad.GetComponent<Placable>().PX = OX + TX;
                        cBad.GetComponent<Placable>().PY = OY + TY;
                        ValidPlacement = true;
                    }
                }
            }

            BadList.Add(cBad);
        }
    }

    public void ToggleMoving(bool Moving)
    {
        if (Moving)
        {
            HighlightAvailableTiles(CurrentTurnActor.GetComponent<Placable>().PX, CurrentTurnActor.GetComponent<Placable>().PY, CurrentTurnActor.GetComponent<Placable>().cAP, 123456789, 123456789, 0);
        }
        else
        {
            for (int x = 0; x < TilesPerSide; x++)
            {
                for (int y = 0; y < TilesPerSide; y++)
                {
                    TileList[x][y].GetComponent<Tile>().ToggleHighlight(false);
                }
            }
        }
    }

    

    public void ToggleAttack(bool Attacking)
    {
        if(Attacking)
        {
            Debug.Log(CurrentTurnActor.GetComponent<Placable>().Name + " is looking to attacking");
            if(CurrentTurnActor.GetComponent<Placable>().PrimaryWeaponType == 1)
            {
                Debug.Log("Highlighting attackable locations for " + CurrentTurnActor.GetComponent<Placable>().Name);
                int x = CurrentTurnActor.GetComponent<Placable>().PX;
                int y = CurrentTurnActor.GetComponent<Placable>().PY;
                if ((x - 1) >= 0)
                {
                    if ((CurrentTurnActor.GetComponent<Placable>().TeamTag == 1 && TileList[x - 1][y].GetComponent<Tile>().cPlacableTag == 2) || 
                        (CurrentTurnActor.GetComponent<Placable>().TeamTag == 2 && TileList[x - 1][y].GetComponent<Tile>().cPlacableTag == 1))
                    {
                        TileList[x - 1][y].GetComponent<Tile>().ToggleHighlight(true);
                        TileList[x - 1][y].GetComponent<Tile>().ToggleAttackable(true);
                    }
                }

                if ((y - 1) >= 0)
                {
                    if
                        ((CurrentTurnActor.GetComponent<Placable>().TeamTag == 1 && TileList[x][y-1].GetComponent<Tile>().cPlacableTag == 2) ||
                        (CurrentTurnActor.GetComponent<Placable>().TeamTag == 2 && TileList[x][y-1].GetComponent<Tile>().cPlacableTag == 1))
                    {
                        TileList[x][y - 1].GetComponent<Tile>().ToggleHighlight(true);
                        TileList[x][y - 1].GetComponent<Tile>().ToggleAttackable(true);
                    }
                }

                if ((x + 1) < TilesPerSide)
                {
                    if ((CurrentTurnActor.GetComponent<Placable>().TeamTag == 1 && TileList[x + 1][y].GetComponent<Tile>().cPlacableTag == 2) ||
                        (CurrentTurnActor.GetComponent<Placable>().TeamTag == 2 && TileList[x + 1][y].GetComponent<Tile>().cPlacableTag == 1))
                    {
                        TileList[x + 1][y].GetComponent<Tile>().ToggleHighlight(true);
                        TileList[x + 1][y].GetComponent<Tile>().ToggleAttackable(true);
                    }
                }

                if ((y + 1) < TilesPerSide)
                {
                    if ((CurrentTurnActor.GetComponent<Placable>().TeamTag == 1 && TileList[x][y+1].GetComponent<Tile>().cPlacableTag == 2) ||
                        (CurrentTurnActor.GetComponent<Placable>().TeamTag == 2 && TileList[x][y+1].GetComponent<Tile>().cPlacableTag == 1))
                    {
                        TileList[x][y + 1].GetComponent<Tile>().ToggleHighlight(true);
                        TileList[x][y + 1].GetComponent<Tile>().ToggleAttackable(true);
                    }
                }
            }
        }
    }

    public IEnumerator AttackTargetClicked(GameObject Target)
    {
        G.GM.GS = GameState.ExecutingTurn;

        Debug.Log(CurrentTurnActor.GetComponent<Placable>().Name + " is Attacking!");

        if (CurrentTurnActor.GetComponent<Placable>().TeamTag == 1)
        {
            G.UIM.AttackClicked(); //Not really, but toggles UI back to non-acting state. I think I do this elsewhere with moving?
        }

        CurrentTurnActor.GetComponent<Placable>().cAP -= 4;

        GameObject ValidTarget = null;

        if (Target.GetComponent<Tile>() != null)
        {
            if (Target.GetComponent<Tile>().Attackable)
            {
                ValidTarget = Target.GetComponent<Tile>().cPlacable.gameObject;
            }
        }

        if (Target.GetComponent<Placable>() != null)
        {
            if (Target.GetComponent<Placable>().Attackable)
            {
                ValidTarget = Target.GetComponent<Placable>().gameObject;
            }
        }

        if (Target.GetComponent<TileCover>() != null)
        {
            if (Target.GetComponent<TileCover>().Attackable)
            {
                ValidTarget = TileList[Target.GetComponent<TileCover>().PX][Target.GetComponent<TileCover>().PY].GetComponent<Tile>().cPlacable.gameObject;
            }
        }

        if (ValidTarget != null)
        {
            Debug.Log("Target is " + ValidTarget.name.ToString());
        }
        else
        {
            Debug.Log("Target is null");
        }

        float ZScore = (ValidTarget.GetComponent<Placable>().cArmor - CurrentTurnActor.GetComponent<Placable>().MeanDamage) / 
            CurrentTurnActor.GetComponent<Placable>().DamageSD;
        double dProbability = 100.0f;
        float fProbability = 100.0f;
        ZScore = Mathf.Round(ZScore * 100f) / 100f;

        foreach(ZValuePair cZVP in ZList)
        {
            if(ZScore == cZVP.ZScore)
            {
                dProbability = (1.0f - cZVP.Probability) * 100.0f;
                fProbability = Mathf.Round((float) dProbability * 100f) / 100f;
                Debug.Log("Probability is " + fProbability.ToString() + "%");

            }
        }

        G.UIM.DisplayNewInfoText(CurrentTurnActor.GetComponent<Placable>().Name.ToString() + "'s Mean Attack Damage is " + CurrentTurnActor.GetComponent<Placable>().MeanDamage + 
            " and Standard Deviation is " + CurrentTurnActor.GetComponent<Placable>().DamageSD + ". " + ValidTarget.GetComponent<Placable>().Name + "'s Armor is " + ValidTarget.GetComponent<Placable>().cArmor + ".", true);
        G.UIM.DisplayNewInfoText("You're probability of besting thier Armor is " + fProbability.ToString() + "%.",true);

        int AttackValue = Mathf.RoundToInt(RandomFromDistribution.RandomNormalDistribution( CurrentTurnActor.GetComponent<Placable>().MeanDamage, 
            CurrentTurnActor.GetComponent<Placable>().DamageSD));
        bool ArmorDestroyed = false;

        if (ValidTarget.GetComponent<Placable>().cArmor != 0)
        {
            ArmorDestroyed = false;
            
            G.UIM.DisplayNewInfoText(CurrentTurnActor.GetComponent<Placable>().Name.ToString() + " attacks with " + AttackValue.ToString() + " against " + ValidTarget.GetComponent<Placable>().Name + "'s armor of " + ValidTarget.GetComponent<Placable>().cArmor + ".", true);
        }
        else
        {
            ArmorDestroyed = true;
            G.UIM.DisplayNewInfoText(CurrentTurnActor.GetComponent<Placable>().Name.ToString() + " attacks with " + AttackValue.ToString() + " against " + ValidTarget.GetComponent<Placable>().Name + "'s health of " + ValidTarget.GetComponent<Placable>().cHealth + ".", true);
        }

        if(!ArmorDestroyed)
        {
            if (AttackValue >= ValidTarget.GetComponent<Placable>().cArmor)
            {
                ValidTarget.GetComponent<Placable>().cArmor = 0;
            }
            else
            {
                ValidTarget.GetComponent<Placable>().cArmor -= 1;
                //ValidTarget.GetComponent<Placable>().cArmor = ValidTarget.GetComponent<Placable>().cArmor - ValidTarget.GetComponent<Placable>().cArmor * 
                //    Mathf.RoundToInt( 1 - (AttackValue / ValidTarget.GetComponent<Placable>().cArmor));
            }
        }
        else
        {
            ValidTarget.GetComponent<Placable>().cHealth -= AttackValue;
        }

        if(ValidTarget.GetComponent<Placable>().cHealth < 1)
        {
            G.UIM.DisplayNewInfoText(ValidTarget.GetComponent<Placable>().Name + " has been destroyed!", true);
            ValidTarget.GetComponent<Placable>().Alive = false;
            BadList.Remove(ValidTarget);
            TurnList.Remove(ValidTarget);
            GameObject.Destroy(ValidTarget);

        }

        foreach(List<GameObject> cTileRow in TileList)
        {
            foreach (GameObject cTile in cTileRow)
            {
                cTile.GetComponent<Tile>().ToggleAttackable(false);
                cTile.GetComponent<Tile>().ToggleHighlight(false);
            }
        }

        //erase all 'attackable'ness

        yield return new WaitForSecondsRealtime(2);

        G.GM.GS = GameState.Play;
    }

    public bool GetHoveredPlacable(GameObject Target)
    {
        if(Target.gameObject.GetComponent<Placable>() != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public IEnumerator MoveDestinationClicked()
    {
        G.GM.GS = GameState.ExecutingTurn;

        Ray ray = Camera1.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            for (int x = 0; x < TilesPerSide; x++)
            {
                for (int y = 0; y < TilesPerSide; y++)
                {
                    if(TileList[x][y].GetComponent<Tile>().Highlighted)
                    {
                        if(hit.collider.gameObject == TileList[x][y].GetComponent<Tile>().TileCoverGO)
                        {
                            Hero1.transform.position = new Vector3(TileList[x][y].transform.position.x, TileList[x][y].GetComponent<Tile>().PlaceOffset, TileList[x][y].transform.position.z);
                            G.UIM.DoneActing();
                            CurrentTurnActor.GetComponent<Placable>().cAP = CurrentTurnActor.GetComponent<Placable>().cAP - TileList[x][y].GetComponent<Tile>().APCostToMove;
                            G.UIM.UpdateHeroCAP(CurrentTurnActor.GetComponent<Placable>().cAP);
                            Debug.Log(CurrentTurnActor.GetComponent<Placable>().cAP.ToString() + "AP left");
                            TileList[x][y].GetComponent<Tile>().cPlacableTag = 1;
                            TileList[x][y].GetComponent<Tile>().cPlacable = CurrentTurnActor.GetComponent<Placable>();
                            TileList[CurrentTurnActor.GetComponent<Placable>().PX][CurrentTurnActor.GetComponent<Placable>().PY].GetComponent<Tile>().cPlacableTag = 0;
                            TileList[CurrentTurnActor.GetComponent<Placable>().PX][CurrentTurnActor.GetComponent<Placable>().PY].GetComponent<Tile>().cPlacable = null;
                            CurrentTurnActor.GetComponent<Placable>().PX = x;
                            CurrentTurnActor.GetComponent<Placable>().PY = y;
                            ToggleMoving(false);
                            CurrentIndicator.transform.position = CurrentTurnActor.transform.position + new Vector3(0, 1.5f, 0);
                            Camera1.transform.position = CurrentTurnActor.transform.position + CameraOffset;
                            Camera1.gameObject.transform.LookAt(CurrentTurnActor.transform);
                            yield return new WaitForSecondsRealtime(2);

                        }
                    }
                }
            }
        }
        G.GM.GS = GameState.Play;

    }

    public bool IsMouseOverBad(GameObject possibleBad)
    {
        bool IsOverBad = false;

        foreach(GameObject cBad in BadList)
        {
            if(possibleBad == cBad)
            {
                IsOverBad = true;
            }
        }

        return IsOverBad;
    }

    public int GetCurrentActorAPCostPerTile()
    {
        return CurrentTurnActor.GetComponent<Placable>().TilesMovedPerAP;
    }

    public void InitZList()
    {
        ZList = new List<ZValuePair>();
        ZList.Add(new ZValuePair(-3.89, 5.01221109961885));
        ZList.Add(new ZValuePair(-3.88,5.22282324018202));
        ZList.Add(new ZValuePair(-3.87,5.44176766336998));
        ZList.Add(new ZValuePair(-3.86,5.66935125342567));
        ZList.Add(new ZValuePair(-3.85,5.90589124189226));
        ZList.Add(new ZValuePair(-3.84,6.15171551832554));
        ZList.Add(new ZValuePair(-3.83,6.40716294888746));
        ZList.Add(new ZValuePair(-3.82,6.67258370296849));
        ZList.Add(new ZValuePair(-3.81,6.94833958798652));
        ZList.Add(new ZValuePair(-3.8,7.23480439251201));
        ZList.Add(new ZValuePair(-3.79,7.53236423786834));
        ZList.Add(new ZValuePair(-3.78,7.84141793835852));
        ZList.Add(new ZValuePair(-3.77,8.16237737026862));
        ZList.Add(new ZValuePair(-3.76,8.49566784979981));
        ZList.Add(new ZValuePair(-3.75,8.8417285200804));
        ZList.Add(new ZValuePair(-3.74,9.20101274741056));
        ZList.Add(new ZValuePair(-3.73,9.57398852689147));
        ZList.Add(new ZValuePair(-3.72,9.96113889759168));
        ZList.Add(new ZValuePair(-3.71,0.000103629623674));
        ZList.Add(new ZValuePair(-3.7,0.000107799733477));
        ZList.Add(new ZValuePair(-3.69,0.000112127025982));
        ZList.Add(new ZValuePair(-3.68,0.000116616976815));
        ZList.Add(new ZValuePair(-3.67,0.000121275234285));
        ZList.Add(new ZValuePair(-3.66,0.000126107624138));
        ZList.Add(new ZValuePair(-3.65,0.00013112015442));
        ZList.Add(new ZValuePair(-3.64,0.000136319020446));
        ZList.Add(new ZValuePair(-3.63,0.000141710609876));
        ZList.Add(new ZValuePair(-3.62,0.000147301507907));
        ZList.Add(new ZValuePair(-3.61,0.000153098502574));
        ZList.Add(new ZValuePair(-3.6,0.000159108590158));
        ZList.Add(new ZValuePair(-3.59,0.00016533898072));
        ZList.Add(new ZValuePair(-3.58,0.000171797103746));
        ZList.Add(new ZValuePair(-3.57,0.000178490613905));
        ZList.Add(new ZValuePair(-3.56,0.000185427396933));
        ZList.Add(new ZValuePair(-3.55,0.000192615575636));
        ZList.Add(new ZValuePair(-3.54,0.000200063516007));
        ZList.Add(new ZValuePair(-3.53,0.000207779833481));
        ZList.Add(new ZValuePair(-3.52,0.000215773399295));
        ZList.Add(new ZValuePair(-3.51,0.000224053346991));
        ZList.Add(new ZValuePair(-3.5,0.000232629079036));
        ZList.Add(new ZValuePair(-3.49,0.000241510273568));
        ZList.Add(new ZValuePair(-3.48,0.000250706891281));
        ZList.Add(new ZValuePair(-3.47,0.000260229182427));
        ZList.Add(new ZValuePair(-3.46,0.000270087693963));
        ZList.Add(new ZValuePair(-3.45,0.000280293276816));
        ZList.Add(new ZValuePair(-3.44,0.000290857093291));
        ZList.Add(new ZValuePair(-3.43,0.000301790624609));
        ZList.Add(new ZValuePair(-3.42,0.000313105678581));
        ZList.Add(new ZValuePair(-3.41,0.000324814397419));
        ZList.Add(new ZValuePair(-3.4,0.000336929265677));
        ZList.Add(new ZValuePair(-3.39,0.000349463118338));
        ZList.Add(new ZValuePair(-3.38,0.000362429149033));
        ZList.Add(new ZValuePair(-3.37,0.0003758409184));
        ZList.Add(new ZValuePair(-3.36,0.000389712362582));
        ZList.Add(new ZValuePair(-3.35,0.000404057801864));
        ZList.Add(new ZValuePair(-3.34,0.00041889194945));
        ZList.Add(new ZValuePair(-3.33,0.000434229920382));
        ZList.Add(new ZValuePair(-3.32,0.000450087240592));
        ZList.Add(new ZValuePair(-3.31,0.000466479856108));
        ZList.Add(new ZValuePair(-3.3,0.000483424142384));
        ZList.Add(new ZValuePair(-3.29,0.000500936913786));
        ZList.Add(new ZValuePair(-3.28,0.000519035433207));
        ZList.Add(new ZValuePair(-3.27,0.00053773742183));
        ZList.Add(new ZValuePair(-3.26,0.000557061069025));
        ZList.Add(new ZValuePair(-3.25,0.000577025042391));
        ZList.Add(new ZValuePair(-3.24,0.000597648497934));
        ZList.Add(new ZValuePair(-3.23,0.000618951090387));
        ZList.Add(new ZValuePair(-3.22,0.00064095298366));
        ZList.Add(new ZValuePair(-3.21,0.00066367486144));
        ZList.Add(new ZValuePair(-3.2,0.000687137937916));
        ZList.Add(new ZValuePair(-3.19,0.000711363968645));
        ZList.Add(new ZValuePair(-3.18,0.000736375261554));
        ZList.Add(new ZValuePair(-3.17,0.000762194688067));
        ZList.Add(new ZValuePair(-3.16,0.000788845694376));
        ZList.Add(new ZValuePair(-3.15,0.000816352312829));
        ZList.Add(new ZValuePair(-3.14,0.000844739173459));
        ZList.Add(new ZValuePair(-3.13,0.000874031515632));
        ZList.Add(new ZValuePair(-3.12,0.000904255199822));
        ZList.Add(new ZValuePair(-3.11,0.000935436719514));
        ZList.Add(new ZValuePair(-3.1,0.000967603213218));
        ZList.Add(new ZValuePair(-3.09,0.001000782476614));
        ZList.Add(new ZValuePair(-3.08,0.001035002974803));
        ZList.Add(new ZValuePair(-3.07,0.001070293854679));
        ZList.Add(new ZValuePair(-3.06,0.001106684957409));
        ZList.Add(new ZValuePair(-3.05,0.001144206831023));
        ZList.Add(new ZValuePair(-3.04,0.001182890743104));
        ZList.Add(new ZValuePair(-3.03,0.001222768693592));
        ZList.Add(new ZValuePair(-3.02,0.001263873427672));
        ZList.Add(new ZValuePair(-3.01,0.001306238448769));
        ZList.Add(new ZValuePair(-3,0.00134989803163));
        ZList.Add(new ZValuePair(-2.99,0.001394887235492));
        ZList.Add(new ZValuePair(-2.98,0.00144124191734));
        ZList.Add(new ZValuePair(-2.97,0.001488998745237));
        ZList.Add(new ZValuePair(-2.96,0.001538195211738));
        ZList.Add(new ZValuePair(-2.95,0.001588869647365));
        ZList.Add(new ZValuePair(-2.94,0.001641061234157));
        ZList.Add(new ZValuePair(-2.93,0.001694810019277));
        ZList.Add(new ZValuePair(-2.92,0.001750156928676));
        ZList.Add(new ZValuePair(-2.91,0.001807143780806));
        ZList.Add(new ZValuePair(-2.9,0.001865813300384));
        ZList.Add(new ZValuePair(-2.89,0.001926209132188));
        ZList.Add(new ZValuePair(-2.88,0.001988375854894));
        ZList.Add(new ZValuePair(-2.87,0.00205235899494));
        ZList.Add(new ZValuePair(-2.86,0.002118205040405));
        ZList.Add(new ZValuePair(-2.85,0.002185961454913));
        ZList.Add(new ZValuePair(-2.84,0.002255676691542));
        ZList.Add(new ZValuePair(-2.83,0.002327400206732));
        ZList.Add(new ZValuePair(-2.82,0.002401182474189));
        ZList.Add(new ZValuePair(-2.81,0.002477074998786));
        ZList.Add(new ZValuePair(-2.8,0.002555130330428));
        ZList.Add(new ZValuePair(-2.79,0.002635402077905));
        ZList.Add(new ZValuePair(-2.78,0.002717944922701));
        ZList.Add(new ZValuePair(-2.77,0.002802814632765));
        ZList.Add(new ZValuePair(-2.76,0.002890068076226));
        ZList.Add(new ZValuePair(-2.75,0.002979763235055));
        ZList.Add(new ZValuePair(-2.74,0.003071959218651));
        ZList.Add(new ZValuePair(-2.73,0.003166716277358));
        ZList.Add(new ZValuePair(-2.72,0.003264095815891));
        ZList.Add(new ZValuePair(-2.71,0.003364160406669));
        ZList.Add(new ZValuePair(-2.7,0.003466973803041));
        ZList.Add(new ZValuePair(-2.69,0.0035726009524));
        ZList.Add(new ZValuePair(-2.68,0.003681108009175));
        ZList.Add(new ZValuePair(-2.67,0.003792562347686));
        ZList.Add(new ZValuePair(-2.66,0.003907032574853));
        ZList.Add(new ZValuePair(-2.65,0.004024588542758));
        ZList.Add(new ZValuePair(-2.64,0.004145301361036));
        ZList.Add(new ZValuePair(-2.63,0.004269243409089));
        ZList.Add(new ZValuePair(-2.62,0.004396488348121));
        ZList.Add(new ZValuePair(-2.61,0.004527111132967));
        ZList.Add(new ZValuePair(-2.6,0.004661188023719));
        ZList.Add(new ZValuePair(-2.59,0.004798796597126));
        ZList.Add(new ZValuePair(-2.58,0.004940015757771));
        ZList.Add(new ZValuePair(-2.57,0.005084925748991));
        ZList.Add(new ZValuePair(-2.56,0.005233608163556));
        ZList.Add(new ZValuePair(-2.55,0.005386145954067));
        ZList.Add(new ZValuePair(-2.54,0.005542623443083));
        ZList.Add(new ZValuePair(-2.53,0.005703126332951));
        ZList.Add(new ZValuePair(-2.52,0.005867741715333));
        ZList.Add(new ZValuePair(-2.51,0.006036558080413));
        ZList.Add(new ZValuePair(-2.5,0.006209665325776));
        ZList.Add(new ZValuePair(-2.49,0.006387154764943));
        ZList.Add(new ZValuePair(-2.48,0.006569119135547));
        ZList.Add(new ZValuePair(-2.47,0.006755652607141));
        ZList.Add(new ZValuePair(-2.46,0.006946850788624));
        ZList.Add(new ZValuePair(-2.45,0.007142810735271));
        ZList.Add(new ZValuePair(-2.44,0.007343630955348));
        ZList.Add(new ZValuePair(-2.43,0.007549411416309));
        ZList.Add(new ZValuePair(-2.42,0.007760253550554));
        ZList.Add(new ZValuePair(-2.41,0.007976260260734));
        ZList.Add(new ZValuePair(-2.4,0.008197535924596));
        ZList.Add(new ZValuePair(-2.39,0.008424186399346));
        ZList.Add(new ZValuePair(-2.38,0.008656319025517));
        ZList.Add(new ZValuePair(-2.37,0.008894042630337));
        ZList.Add(new ZValuePair(-2.36,0.009137467530573));
        ZList.Add(new ZValuePair(-2.35,0.009386705534839));
        ZList.Add(new ZValuePair(-2.34,0.009641869945358));
        ZList.Add(new ZValuePair(-2.33,0.009903075559164));
        ZList.Add(new ZValuePair(-2.32,0.01017043866872));
        ZList.Add(new ZValuePair(-2.31,0.010444077061951));
        ZList.Add(new ZValuePair(-2.3,0.010724110021676));
        ZList.Add(new ZValuePair(-2.29,0.011010658324411));
        ZList.Add(new ZValuePair(-2.28,0.011303844238553));
        ZList.Add(new ZValuePair(-2.27,0.011603791521904));
        ZList.Add(new ZValuePair(-2.26,0.011910625418547));
        ZList.Add(new ZValuePair(-2.25,0.012224472655045));
        ZList.Add(new ZValuePair(-2.24,0.012545461435947));
        ZList.Add(new ZValuePair(-2.23,0.012873721438602));
        ZList.Add(new ZValuePair(-2.22,0.013209383807256));
        ZList.Add(new ZValuePair(-2.21,0.01355258114642));
        ZList.Add(new ZValuePair(-2.2,0.013903447513499));
        ZList.Add(new ZValuePair(-2.19,0.014262118410669));
        ZList.Add(new ZValuePair(-2.18,0.014628730775989));
        ZList.Add(new ZValuePair(-2.17,0.015003422973732));
        ZList.Add(new ZValuePair(-2.16,0.015386334783926));
        ZList.Add(new ZValuePair(-2.15,0.015777607391091));
        ZList.Add(new ZValuePair(-2.14,0.016177383372166));
        ZList.Add(new ZValuePair(-2.13,0.016585806683605));
        ZList.Add(new ZValuePair(-2.12,0.017003022647633));
        ZList.Add(new ZValuePair(-2.11,0.017429177937657));
        ZList.Add(new ZValuePair(-2.1,0.017864420562817));
        ZList.Add(new ZValuePair(-2.09,0.018308899851659));
        ZList.Add(new ZValuePair(-2.08,0.018762766434938));
        ZList.Add(new ZValuePair(-2.07,0.019226172227517));
        ZList.Add(new ZValuePair(-2.06,0.019699270409377));
        ZList.Add(new ZValuePair(-2.05,0.020182215405705));
        ZList.Add(new ZValuePair(-2.04,0.02067516286607));
        ZList.Add(new ZValuePair(-2.03,0.021178269642672));
        ZList.Add(new ZValuePair(-2.02,0.021691693767647));
        ZList.Add(new ZValuePair(-2.01,0.022215594429432));
        ZList.Add(new ZValuePair(-2,0.022750131948179));
        ZList.Add(new ZValuePair(-1.99,0.023295467750212));
        ZList.Add(new ZValuePair(-1.98,0.023851764341509));
        ZList.Add(new ZValuePair(-1.97,0.024419185280223));
        ZList.Add(new ZValuePair(-1.96,0.024997895148221));
        ZList.Add(new ZValuePair(-1.95,0.025588059521639));
        ZList.Add(new ZValuePair(-1.94,0.026189844940453));
        ZList.Add(new ZValuePair(-1.93,0.026803418877055));
        ZList.Add(new ZValuePair(-1.92,0.027428949703837));
        ZList.Add(new ZValuePair(-1.91,0.028066606659773));
        ZList.Add(new ZValuePair(-1.9,0.028716559816002));
        ZList.Add(new ZValuePair(-1.89,0.02937898004041));
        ZList.Add(new ZValuePair(-1.88,0.0300540389612));
        ZList.Add(new ZValuePair(-1.87,0.030741908929466));
        ZList.Add(new ZValuePair(-1.86,0.031442762980753));
        ZList.Add(new ZValuePair(-1.85,0.032156774795614));
        ZList.Add(new ZValuePair(-1.84,0.032884118659164));
        ZList.Add(new ZValuePair(-1.83,0.033624969419628));
        ZList.Add(new ZValuePair(-1.82,0.03437950244589));
        ZList.Add(new ZValuePair(-1.81,0.035147893584039));
        ZList.Add(new ZValuePair(-1.8,0.035930319112926));
        ZList.Add(new ZValuePair(-1.79,0.036726955698726));
        ZList.Add(new ZValuePair(-1.78,0.037537980348517));
        ZList.Add(new ZValuePair(-1.77,0.038363570362871));
        ZList.Add(new ZValuePair(-1.76,0.039203903287483));
        ZList.Add(new ZValuePair(-1.75,0.040059156863817));
        ZList.Add(new ZValuePair(-1.74,0.040929508978807));
        ZList.Add(new ZValuePair(-1.73,0.041815137613595));
        ZList.Add(new ZValuePair(-1.72,0.042716220791329));
        ZList.Add(new ZValuePair(-1.71,0.043632936524032));
        ZList.Add(new ZValuePair(-1.7,0.044565462758543));
        ZList.Add(new ZValuePair(-1.69,0.04551397732155));
        ZList.Add(new ZValuePair(-1.68,0.04647865786372));
        ZList.Add(new ZValuePair(-1.67,0.047459681802947));
        ZList.Add(new ZValuePair(-1.66,0.048457226266723));
        ZList.Add(new ZValuePair(-1.65,0.049471468033648));
        ZList.Add(new ZValuePair(-1.64,0.050502583474104));
        ZList.Add(new ZValuePair(-1.63,0.051550748490089));
        ZList.Add(new ZValuePair(-1.62,0.052616138454252));
        ZList.Add(new ZValuePair(-1.61,0.05369892814812));
        ZList.Add(new ZValuePair(-1.6,0.054799291699558));
        ZList.Add(new ZValuePair(-1.59,0.05591740251947));
        ZList.Add(new ZValuePair(-1.58,0.057053433237754));
        ZList.Add(new ZValuePair(-1.57,0.058207555638553));
        ZList.Add(new ZValuePair(-1.56,0.059379940594793));
        ZList.Add(new ZValuePair(-1.55,0.060570758002059));
        ZList.Add(new ZValuePair(-1.54,0.061780176711812));
        ZList.Add(new ZValuePair(-1.53,0.063008364463979));
        ZList.Add(new ZValuePair(-1.52,0.064255487818936));
        ZList.Add(new ZValuePair(-1.51,0.065521712088917));
        ZList.Add(new ZValuePair(-1.5,0.066807201268858));
        ZList.Add(new ZValuePair(-1.49,0.068112117966726));
        ZList.Add(new ZValuePair(-1.48,0.069436623333332));
        ZList.Add(new ZValuePair(-1.47,0.070780876991686));
        ZList.Add(new ZValuePair(-1.46,0.072145036965894));
        ZList.Add(new ZValuePair(-1.45,0.073529259609648));
        ZList.Add(new ZValuePair(-1.44,0.074933699534327));
        ZList.Add(new ZValuePair(-1.43,0.076358509536739));
        ZList.Add(new ZValuePair(-1.42,0.077803840526547));
        ZList.Add(new ZValuePair(-1.41,0.079269841453392));
        ZList.Add(new ZValuePair(-1.4,0.080756659233771));
        ZList.Add(new ZValuePair(-1.39,0.082264438677669));
        ZList.Add(new ZValuePair(-1.38,0.083793322415014));
        ZList.Add(new ZValuePair(-1.37,0.085343450821967));
        ZList.Add(new ZValuePair(-1.36,0.086914961947085));
        ZList.Add(new ZValuePair(-1.35,0.088507991437402));
        ZList.Add(new ZValuePair(-1.34,0.090122672464453));
        ZList.Add(new ZValuePair(-1.33,0.091759135650281));
        ZList.Add(new ZValuePair(-1.32,0.093417508993472));
        ZList.Add(new ZValuePair(-1.31,0.095097917795239));
        ZList.Add(new ZValuePair(-1.3,0.09680048458561));
        ZList.Add(new ZValuePair(-1.29,0.098525329049748));
        ZList.Add(new ZValuePair(-1.28,0.100272567954442));
        ZList.Add(new ZValuePair(-1.27,0.102042315074819));
        ZList.Add(new ZValuePair(-1.26,0.1038346811213));
        ZList.Add(new ZValuePair(-1.25,0.105649773666855));
        ZList.Add(new ZValuePair(-1.24,0.107487697074587));
        ZList.Add(new ZValuePair(-1.23,0.109348552425692));
        ZList.Add(new ZValuePair(-1.22,0.111232437447835));
        ZList.Add(new ZValuePair(-1.21,0.113139446443977));
        ZList.Add(new ZValuePair(-1.2,0.115069670221708));
        ZList.Add(new ZValuePair(-1.19,0.117023196023109));
        ZList.Add(new ZValuePair(-1.18,0.119000107455201));
        ZList.Add(new ZValuePair(-1.17,0.121000484421018));
        ZList.Add(new ZValuePair(-1.16,0.123024403051343));
        ZList.Add(new ZValuePair(-1.15,0.12507193563715));
        ZList.Add(new ZValuePair(-1.14,0.127143150562798));
        ZList.Add(new ZValuePair(-1.13,0.129238112240018));
        ZList.Add(new ZValuePair(-1.12,0.131356881042731));
        ZList.Add(new ZValuePair(-1.11,0.133499513242747));
        ZList.Add(new ZValuePair(-1.1,0.135666060946383));
        ZList.Add(new ZValuePair(-1.09,0.137856572032036));
        ZList.Add(new ZValuePair(-1.08,0.140071090088769));
        ZList.Add(new ZValuePair(-1.07,0.142309654355939));
        ZList.Add(new ZValuePair(-1.06,0.14457229966391));
        ZList.Add(new ZValuePair(-1.05,0.146859056375896));
        ZList.Add(new ZValuePair(-1.04,0.149169950330981));
        ZList.Add(new ZValuePair(-1.03,0.151505002788344));
        ZList.Add(new ZValuePair(-1.02,0.153864230372735));
        ZList.Add(new ZValuePair(-1.01,0.156247645021255));
        ZList.Add(new ZValuePair(-1,0.158655253931457));
        ZList.Add(new ZValuePair(-0.99,0.161087059510831));
        ZList.Add(new ZValuePair(-0.98,0.163543059327692));
        ZList.Add(new ZValuePair(-0.97,0.16602324606353));
        ZList.Add(new ZValuePair(-0.96,0.168527607466838));
        ZList.Add(new ZValuePair(-0.95,0.171056126308482));
        ZList.Add(new ZValuePair(-0.94,0.173608780338625));
        ZList.Add(new ZValuePair(-0.93,0.176185542245258));
        ZList.Add(new ZValuePair(-0.92,0.178786379614372));
        ZList.Add(new ZValuePair(-0.91,0.181411254891797));
        ZList.Add(new ZValuePair(-0.9,0.18406012534676));
        ZList.Add(new ZValuePair(-0.89,0.186732943037173));
        ZList.Add(new ZValuePair(-0.88,0.189429654776712));
        ZList.Add(new ZValuePair(-0.87,0.192150202103696));
        ZList.Add(new ZValuePair(-0.86,0.194894521251808));
        ZList.Add(new ZValuePair(-0.85,0.197662543122692));
        ZList.Add(new ZValuePair(-0.84,0.20045419326045));
        ZList.Add(new ZValuePair(-0.83,0.203269391828068));
        ZList.Add(new ZValuePair(-0.82,0.206108053585813));
        ZList.Add(new ZValuePair(-0.81,0.208970087871602));
        ZList.Add(new ZValuePair(-0.8,0.211855398583397));
        ZList.Add(new ZValuePair(-0.79,0.214763884163637));
        ZList.Add(new ZValuePair(-0.78,0.217695437585733));
        ZList.Add(new ZValuePair(-0.77,0.22064994634265));
        ZList.Add(new ZValuePair(-0.76,0.223627292437599));
        ZList.Add(new ZValuePair(-0.75,0.226627352376868));
        ZList.Add(new ZValuePair(-0.74,0.229649997164791));
        ZList.Add(new ZValuePair(-0.73,0.232695092300897));
        ZList.Add(new ZValuePair(-0.72,0.235762497779251));
        ZList.Add(new ZValuePair(-0.71,0.238852068089987));
        ZList.Add(new ZValuePair(-0.7,0.241963652223073));
        ZList.Add(new ZValuePair(-0.69,0.245097093674309));
        ZList.Add(new ZValuePair(-0.68,0.248252230453571));
        ZList.Add(new ZValuePair(-0.67,0.25142889509531));
        ZList.Add(new ZValuePair(-0.66,0.254626914671336));
        ZList.Add(new ZValuePair(-0.65,0.257846110805865));
        ZList.Add(new ZValuePair(-0.64,0.261086299692862));
        ZList.Add(new ZValuePair(-0.63,0.264347292115678));
        ZList.Add(new ZValuePair(-0.62,0.267628893468983));
        ZList.Add(new ZValuePair(-0.61,0.270930903783006));
        ZList.Add(new ZValuePair(-0.6,0.274253117750074));
        ZList.Add(new ZValuePair(-0.59,0.277595324753465));
        ZList.Add(new ZValuePair(-0.58,0.280957308898564));
        ZList.Add(new ZValuePair(-0.57,0.284338849046324));
        ZList.Add(new ZValuePair(-0.56,0.287739718849027));
        ZList.Add(new ZValuePair(-0.55,0.291159686788346));
        ZList.Add(new ZValuePair(-0.54,0.294598516215698));
        ZList.Add(new ZValuePair(-0.53,0.298055965394876));
        ZList.Add(new ZValuePair(-0.52,0.301531787546966));
        ZList.Add(new ZValuePair(-0.51,0.305025730897519));
        ZList.Add(new ZValuePair(-0.5,0.308537538725987));
        ZList.Add(new ZValuePair(-0.49,0.312066949417391));
        ZList.Add(new ZValuePair(-0.48,0.315613696516223));
        ZList.Add(new ZValuePair(-0.47,0.319177508782556));
        ZList.Add(new ZValuePair(-0.46,0.322758110250348));
        ZList.Add(new ZValuePair(-0.45,0.32635522028792));
        ZList.Add(new ZValuePair(-0.44,0.329968553660594));
        ZList.Add(new ZValuePair(-0.43,0.333597820595458));
        ZList.Add(new ZValuePair(-0.42,0.337242726848249));
        ZList.Add(new ZValuePair(-0.41,0.340902973772323));
        ZList.Add(new ZValuePair(-0.4,0.344578258389676));
        ZList.Add(new ZValuePair(-0.39,0.348268273464018));
        ZList.Add(new ZValuePair(-0.38,0.351972707575837));
        ZList.Add(new ZValuePair(-0.37,0.355691245199453));
        ZList.Add(new ZValuePair(-0.36,0.359423566782009));
        ZList.Add(new ZValuePair(-0.35,0.363169348824381));
        ZList.Add(new ZValuePair(-0.34,0.366928263963972));
        ZList.Add(new ZValuePair(-0.33,0.370699981059347));
        ZList.Add(new ZValuePair(-0.32,0.37448416527668));
        ZList.Add(new ZValuePair(-0.31,0.378280478177981));
        ZList.Add(new ZValuePair(-0.3,0.382088577811047));
        ZList.Add(new ZValuePair(-0.29,0.385908118801123));
        ZList.Add(new ZValuePair(-0.28,0.389738752444203));
        ZList.Add(new ZValuePair(-0.27,0.39358012680196));
        ZList.Add(new ZValuePair(-0.26,0.397431886798239));
        ZList.Add(new ZValuePair(-0.25,0.401293674317076));
        ZList.Add(new ZValuePair(-0.24,0.405165128302204));
        ZList.Add(new ZValuePair(-0.23,0.409045884857994));
        ZList.Add(new ZValuePair(-0.22,0.412935577351785));
        ZList.Add(new ZValuePair(-0.21,0.416833836517558));
        ZList.Add(new ZValuePair(-0.2,0.420740290560897));
        ZList.Add(new ZValuePair(-0.19,0.424654565265204));
        ZList.Add(new ZValuePair(-0.18,0.428576284099099));
        ZList.Add(new ZValuePair(-0.17,0.432505068324962));
        ZList.Add(new ZValuePair(-0.16,0.436440537108567));
        ZList.Add(new ZValuePair(-0.15,0.440382307629757));
        ZList.Add(new ZValuePair(-0.14,0.444329995194094));
        ZList.Add(new ZValuePair(-0.13,0.448283213345439));
        ZList.Add(new ZValuePair(-0.12,0.452241573979416));
        ZList.Add(new ZValuePair(-0.11,0.456204687457683));
        ZList.Add(new ZValuePair(-0.1,0.460172162722971));
        ZList.Add(new ZValuePair(-0.09,0.464143607414828));
        ZList.Add(new ZValuePair(-0.08,0.468118627986013));
        ZList.Add(new ZValuePair(-0.07,0.472096829819479));
        ZList.Add(new ZValuePair(-0.06,0.476077817345893));
        ZList.Add(new ZValuePair(-0.05,0.480061194161627));
        ZList.Add(new ZValuePair(-0.04,0.484046563147169));
        ZList.Add(new ZValuePair(-0.03,0.488033526585887));
        ZList.Add(new ZValuePair(-0.02,0.492021686283098));
        ZList.Add(new ZValuePair(-0.01,0.496010643685368));
        ZList.Add(new ZValuePair(-0.009,0.496409567947285));
        ZList.Add(new ZValuePair(-0.008,0.496808495799536));
        ZList.Add(new ZValuePair(-0.007,0.497207426843223));
        ZList.Add(new ZValuePair(-0.006,0.497606360679436));
        ZList.Add(new ZValuePair(-0.005,0.498005296909259));
        ZList.Add(new ZValuePair(-0.004,0.498404235133768));
        ZList.Add(new ZValuePair(-0.003,0.498803174954034));
        ZList.Add(new ZValuePair(-0.002,0.49920211597112));
        ZList.Add(new ZValuePair(-0.001,0.499601057786089));
        ZList.Add(new ZValuePair(0,0.5));
        ZList.Add(new ZValuePair(0.001,0.499601057786089));
        ZList.Add(new ZValuePair(0.002,0.49920211597112));
        ZList.Add(new ZValuePair(0.003,0.498803174954034));
        ZList.Add(new ZValuePair(0.004,0.498404235133768));
        ZList.Add(new ZValuePair(0.005,0.498005296909259));
        ZList.Add(new ZValuePair(0.006,0.497606360679436));
        ZList.Add(new ZValuePair(0.007,0.497207426843223));
        ZList.Add(new ZValuePair(0.008,0.496808495799536));
        ZList.Add(new ZValuePair(0.009,0.496409567947285));
        ZList.Add(new ZValuePair(0.01,0.496010643685368));
        ZList.Add(new ZValuePair(0.02,0.492021686283098));
        ZList.Add(new ZValuePair(0.03,0.488033526585887));
        ZList.Add(new ZValuePair(0.04,0.484046563147169));
        ZList.Add(new ZValuePair(0.05,0.480061194161628));
        ZList.Add(new ZValuePair(0.06,0.476077817345893));
        ZList.Add(new ZValuePair(0.07,0.472096829819479));
        ZList.Add(new ZValuePair(0.08,0.468118627986013));
        ZList.Add(new ZValuePair(0.09,0.464143607414828));
        ZList.Add(new ZValuePair(0.1,0.460172162722971));
        ZList.Add(new ZValuePair(0.11,0.456204687457683));
        ZList.Add(new ZValuePair(0.12,0.452241573979416));
        ZList.Add(new ZValuePair(0.13,0.448283213345439));
        ZList.Add(new ZValuePair(0.14,0.444329995194094));
        ZList.Add(new ZValuePair(0.15,0.440382307629757));
        ZList.Add(new ZValuePair(0.16,0.436440537108567));
        ZList.Add(new ZValuePair(0.17,0.432505068324962));
        ZList.Add(new ZValuePair(0.18,0.428576284099099));
        ZList.Add(new ZValuePair(0.19,0.424654565265204));
        ZList.Add(new ZValuePair(0.2,0.420740290560897));
        ZList.Add(new ZValuePair(0.21,0.416833836517558));
        ZList.Add(new ZValuePair(0.22,0.412935577351785));
        ZList.Add(new ZValuePair(0.23,0.409045884857994));
        ZList.Add(new ZValuePair(0.24,0.405165128302204));
        ZList.Add(new ZValuePair(0.25,0.401293674317076));
        ZList.Add(new ZValuePair(0.26,0.397431886798239));
        ZList.Add(new ZValuePair(0.27,0.393580126801961));
        ZList.Add(new ZValuePair(0.28,0.389738752444203));
        ZList.Add(new ZValuePair(0.29,0.385908118801123));
        ZList.Add(new ZValuePair(0.3,0.382088577811047));
        ZList.Add(new ZValuePair(0.31,0.378280478177981));
        ZList.Add(new ZValuePair(0.32,0.37448416527668));
        ZList.Add(new ZValuePair(0.33,0.370699981059347));
        ZList.Add(new ZValuePair(0.34,0.366928263963972));
        ZList.Add(new ZValuePair(0.35,0.363169348824381));
        ZList.Add(new ZValuePair(0.36,0.359423566782009));
        ZList.Add(new ZValuePair(0.37,0.355691245199453));
        ZList.Add(new ZValuePair(0.38,0.351972707575837));
        ZList.Add(new ZValuePair(0.39,0.348268273464018));
        ZList.Add(new ZValuePair(0.4,0.344578258389676));
        ZList.Add(new ZValuePair(0.41,0.340902973772323));
        ZList.Add(new ZValuePair(0.42,0.337242726848249));
        ZList.Add(new ZValuePair(0.43,0.333597820595458));
        ZList.Add(new ZValuePair(0.44,0.329968553660594));
        ZList.Add(new ZValuePair(0.45,0.32635522028792));
        ZList.Add(new ZValuePair(0.46,0.322758110250348));
        ZList.Add(new ZValuePair(0.47,0.319177508782556));
        ZList.Add(new ZValuePair(0.48,0.315613696516223));
        ZList.Add(new ZValuePair(0.49,0.312066949417391));
        ZList.Add(new ZValuePair(0.5,0.308537538725987));
        ZList.Add(new ZValuePair(0.51,0.305025730897519));
        ZList.Add(new ZValuePair(0.52,0.301531787546966));
        ZList.Add(new ZValuePair(0.53,0.298055965394876));
        ZList.Add(new ZValuePair(0.54,0.294598516215698));
        ZList.Add(new ZValuePair(0.55,0.291159686788346));
        ZList.Add(new ZValuePair(0.56,0.287739718849027));
        ZList.Add(new ZValuePair(0.57,0.284338849046324));
        ZList.Add(new ZValuePair(0.58,0.280957308898564));
        ZList.Add(new ZValuePair(0.59,0.277595324753465));
        ZList.Add(new ZValuePair(0.6,0.274253117750074));
        ZList.Add(new ZValuePair(0.61,0.270930903783006));
        ZList.Add(new ZValuePair(0.62,0.267628893468983));
        ZList.Add(new ZValuePair(0.63,0.264347292115678));
        ZList.Add(new ZValuePair(0.64,0.261086299692862));
        ZList.Add(new ZValuePair(0.65,0.257846110805865));
        ZList.Add(new ZValuePair(0.66,0.254626914671336));
        ZList.Add(new ZValuePair(0.67,0.25142889509531));
        ZList.Add(new ZValuePair(0.68,0.248252230453571));
        ZList.Add(new ZValuePair(0.69,0.24509709367431));
        ZList.Add(new ZValuePair(0.7,0.241963652223073));
        ZList.Add(new ZValuePair(0.71,0.238852068089987));
        ZList.Add(new ZValuePair(0.72,0.235762497779251));
        ZList.Add(new ZValuePair(0.73,0.232695092300897));
        ZList.Add(new ZValuePair(0.74,0.229649997164791));
        ZList.Add(new ZValuePair(0.75,0.226627352376868));
        ZList.Add(new ZValuePair(0.76,0.223627292437599));
        ZList.Add(new ZValuePair(0.77,0.22064994634265));
        ZList.Add(new ZValuePair(0.78,0.217695437585733));
        ZList.Add(new ZValuePair(0.79,0.214763884163637));
        ZList.Add(new ZValuePair(0.8,0.211855398583397));
        ZList.Add(new ZValuePair(0.81,0.208970087871602));
        ZList.Add(new ZValuePair(0.82,0.206108053585813));
        ZList.Add(new ZValuePair(0.83,0.203269391828068));
        ZList.Add(new ZValuePair(0.84,0.20045419326045));
        ZList.Add(new ZValuePair(0.85,0.197662543122692));
        ZList.Add(new ZValuePair(0.86,0.194894521251808));
        ZList.Add(new ZValuePair(0.87,0.192150202103696));
        ZList.Add(new ZValuePair(0.88,0.189429654776712));
        ZList.Add(new ZValuePair(0.89,0.186732943037173));
        ZList.Add(new ZValuePair(0.9,0.18406012534676));
        ZList.Add(new ZValuePair(0.91,0.181411254891797));
        ZList.Add(new ZValuePair(0.92,0.178786379614372));
        ZList.Add(new ZValuePair(0.93,0.176185542245258));
        ZList.Add(new ZValuePair(0.94,0.173608780338625));
        ZList.Add(new ZValuePair(0.95,0.171056126308482));
        ZList.Add(new ZValuePair(0.96,0.168527607466838));
        ZList.Add(new ZValuePair(0.97,0.16602324606353));
        ZList.Add(new ZValuePair(0.98,0.163543059327692));
        ZList.Add(new ZValuePair(0.99,0.161087059510831));
        ZList.Add(new ZValuePair(1,0.158655253931457));
        ZList.Add(new ZValuePair(1.01,0.156247645021255));
        ZList.Add(new ZValuePair(1.02,0.153864230372735));
        ZList.Add(new ZValuePair(1.03,0.151505002788344));
        ZList.Add(new ZValuePair(1.04,0.149169950330981));
        ZList.Add(new ZValuePair(1.05,0.146859056375896));
        ZList.Add(new ZValuePair(1.06,0.14457229966391));
        ZList.Add(new ZValuePair(1.07,0.142309654355939));
        ZList.Add(new ZValuePair(1.08,0.140071090088769));
        ZList.Add(new ZValuePair(1.09,0.137856572032036));
        ZList.Add(new ZValuePair(1.1,0.135666060946383));
        ZList.Add(new ZValuePair(1.11,0.133499513242747));
        ZList.Add(new ZValuePair(1.12,0.131356881042731));
        ZList.Add(new ZValuePair(1.13,0.129238112240018));
        ZList.Add(new ZValuePair(1.14,0.127143150562798));
        ZList.Add(new ZValuePair(1.15,0.12507193563715));
        ZList.Add(new ZValuePair(1.16,0.123024403051343));
        ZList.Add(new ZValuePair(1.17,0.121000484421018));
        ZList.Add(new ZValuePair(1.18,0.119000107455201));
        ZList.Add(new ZValuePair(1.19,0.117023196023109));
        ZList.Add(new ZValuePair(1.2,0.115069670221708));
        ZList.Add(new ZValuePair(1.21,0.113139446443977));
        ZList.Add(new ZValuePair(1.22,0.111232437447835));
        ZList.Add(new ZValuePair(1.23,0.109348552425692));
        ZList.Add(new ZValuePair(1.24,0.107487697074587));
        ZList.Add(new ZValuePair(1.25,0.105649773666855));
        ZList.Add(new ZValuePair(1.26,0.1038346811213));
        ZList.Add(new ZValuePair(1.27,0.102042315074819));
        ZList.Add(new ZValuePair(1.28,0.100272567954442));
        ZList.Add(new ZValuePair(1.29,0.098525329049748));
        ZList.Add(new ZValuePair(1.3,0.09680048458561));
        ZList.Add(new ZValuePair(1.31,0.095097917795239));
        ZList.Add(new ZValuePair(1.32,0.093417508993472));
        ZList.Add(new ZValuePair(1.33,0.091759135650281));
        ZList.Add(new ZValuePair(1.34,0.090122672464452));
        ZList.Add(new ZValuePair(1.35,0.088507991437402));
        ZList.Add(new ZValuePair(1.36,0.086914961947085));
        ZList.Add(new ZValuePair(1.37,0.085343450821967));
        ZList.Add(new ZValuePair(1.38,0.083793322415014));
        ZList.Add(new ZValuePair(1.39,0.082264438677669));
        ZList.Add(new ZValuePair(1.4,0.080756659233771));
        ZList.Add(new ZValuePair(1.41,0.079269841453392));
        ZList.Add(new ZValuePair(1.42,0.077803840526547));
        ZList.Add(new ZValuePair(1.43,0.076358509536739));
        ZList.Add(new ZValuePair(1.44,0.074933699534327));
        ZList.Add(new ZValuePair(1.45,0.073529259609648));
        ZList.Add(new ZValuePair(1.46,0.072145036965894));
        ZList.Add(new ZValuePair(1.47,0.070780876991686));
        ZList.Add(new ZValuePair(1.48,0.069436623333332));
        ZList.Add(new ZValuePair(1.49,0.068112117966726));
        ZList.Add(new ZValuePair(1.5,0.066807201268858));
        ZList.Add(new ZValuePair(1.51,0.065521712088917));
        ZList.Add(new ZValuePair(1.52,0.064255487818936));
        ZList.Add(new ZValuePair(1.53,0.063008364463978));
        ZList.Add(new ZValuePair(1.54,0.061780176711812));
        ZList.Add(new ZValuePair(1.55,0.060570758002059));
        ZList.Add(new ZValuePair(1.56,0.059379940594793));
        ZList.Add(new ZValuePair(1.57,0.058207555638553));
        ZList.Add(new ZValuePair(1.58,0.057053433237754));
        ZList.Add(new ZValuePair(1.59,0.05591740251947));
        ZList.Add(new ZValuePair(1.6,0.054799291699558));
        ZList.Add(new ZValuePair(1.61,0.05369892814812));
        ZList.Add(new ZValuePair(1.62,0.052616138454252));
        ZList.Add(new ZValuePair(1.63,0.05155074849009));
        ZList.Add(new ZValuePair(1.64,0.050502583474104));
        ZList.Add(new ZValuePair(1.65,0.049471468033648));
        ZList.Add(new ZValuePair(1.66,0.048457226266723));
        ZList.Add(new ZValuePair(1.67,0.047459681802947));
        ZList.Add(new ZValuePair(1.68,0.04647865786372));
        ZList.Add(new ZValuePair(1.69,0.04551397732155));
        ZList.Add(new ZValuePair(1.7,0.044565462758543));
        ZList.Add(new ZValuePair(1.71,0.043632936524032));
        ZList.Add(new ZValuePair(1.72,0.042716220791329));
        ZList.Add(new ZValuePair(1.73,0.041815137613595));
        ZList.Add(new ZValuePair(1.74,0.040929508978807));
        ZList.Add(new ZValuePair(1.75,0.040059156863817));
        ZList.Add(new ZValuePair(1.76,0.039203903287483));
        ZList.Add(new ZValuePair(1.77,0.038363570362871));
        ZList.Add(new ZValuePair(1.78,0.037537980348517));
        ZList.Add(new ZValuePair(1.79,0.036726955698726));
        ZList.Add(new ZValuePair(1.8,0.035930319112926));
        ZList.Add(new ZValuePair(1.81,0.035147893584039));
        ZList.Add(new ZValuePair(1.82,0.03437950244589));
        ZList.Add(new ZValuePair(1.83,0.033624969419628));
        ZList.Add(new ZValuePair(1.84,0.032884118659164));
        ZList.Add(new ZValuePair(1.85,0.032156774795614));
        ZList.Add(new ZValuePair(1.86,0.031442762980753));
        ZList.Add(new ZValuePair(1.87,0.030741908929466));
        ZList.Add(new ZValuePair(1.88,0.0300540389612));
        ZList.Add(new ZValuePair(1.89,0.02937898004041));
        ZList.Add(new ZValuePair(1.9,0.028716559816002));
        ZList.Add(new ZValuePair(1.91,0.028066606659773));
        ZList.Add(new ZValuePair(1.92,0.027428949703837));
        ZList.Add(new ZValuePair(1.93,0.026803418877055));
        ZList.Add(new ZValuePair(1.94,0.026189844940453));
        ZList.Add(new ZValuePair(1.95,0.025588059521639));
        ZList.Add(new ZValuePair(1.96,0.024997895148221));
        ZList.Add(new ZValuePair(1.97,0.024419185280223));
        ZList.Add(new ZValuePair(1.98,0.023851764341509));
        ZList.Add(new ZValuePair(1.99,0.023295467750212));
        ZList.Add(new ZValuePair(2,0.022750131948179));
        ZList.Add(new ZValuePair(2.01,0.022215594429432));
        ZList.Add(new ZValuePair(2.02,0.021691693767647));
        ZList.Add(new ZValuePair(2.03,0.021178269642672));
        ZList.Add(new ZValuePair(2.04,0.02067516286607));
        ZList.Add(new ZValuePair(2.05,0.020182215405705));
        ZList.Add(new ZValuePair(2.06,0.019699270409377));
        ZList.Add(new ZValuePair(2.07,0.019226172227517));
        ZList.Add(new ZValuePair(2.08,0.018762766434938));
        ZList.Add(new ZValuePair(2.09,0.018308899851659));
        ZList.Add(new ZValuePair(2.1,0.017864420562817));
        ZList.Add(new ZValuePair(2.11,0.017429177937657));
        ZList.Add(new ZValuePair(2.12,0.017003022647633));
        ZList.Add(new ZValuePair(2.13,0.016585806683605));
        ZList.Add(new ZValuePair(2.14,0.016177383372166));
        ZList.Add(new ZValuePair(2.15,0.015777607391091));
        ZList.Add(new ZValuePair(2.16,0.015386334783925));
        ZList.Add(new ZValuePair(2.17,0.015003422973732));
        ZList.Add(new ZValuePair(2.18,0.014628730775989));
        ZList.Add(new ZValuePair(2.19,0.014262118410669));
        ZList.Add(new ZValuePair(2.2,0.013903447513499));
        ZList.Add(new ZValuePair(2.21,0.01355258114642));
        ZList.Add(new ZValuePair(2.22,0.013209383807256));
        ZList.Add(new ZValuePair(2.23,0.012873721438602));
        ZList.Add(new ZValuePair(2.24,0.012545461435947));
        ZList.Add(new ZValuePair(2.25,0.012224472655045));
        ZList.Add(new ZValuePair(2.26,0.011910625418547));
        ZList.Add(new ZValuePair(2.27,0.011603791521904));
        ZList.Add(new ZValuePair(2.28,0.011303844238553));
        ZList.Add(new ZValuePair(2.29,0.011010658324411));
        ZList.Add(new ZValuePair(2.3,0.010724110021676));
        ZList.Add(new ZValuePair(2.31,0.010444077061951));
        ZList.Add(new ZValuePair(2.32,0.01017043866872));
        ZList.Add(new ZValuePair(2.33,0.009903075559164));
        ZList.Add(new ZValuePair(2.34,0.009641869945358));
        ZList.Add(new ZValuePair(2.35,0.009386705534839));
        ZList.Add(new ZValuePair(2.36,0.009137467530573));
        ZList.Add(new ZValuePair(2.37,0.008894042630337));
        ZList.Add(new ZValuePair(2.38,0.008656319025517));
        ZList.Add(new ZValuePair(2.39,0.008424186399346));
        ZList.Add(new ZValuePair(2.4,0.008197535924596));
        ZList.Add(new ZValuePair(2.41,0.007976260260734));
        ZList.Add(new ZValuePair(2.42,0.007760253550554));
        ZList.Add(new ZValuePair(2.43,0.007549411416309));
        ZList.Add(new ZValuePair(2.44,0.007343630955348));
        ZList.Add(new ZValuePair(2.45,0.007142810735271));
        ZList.Add(new ZValuePair(2.46,0.006946850788624));
        ZList.Add(new ZValuePair(2.47,0.006755652607141));
        ZList.Add(new ZValuePair(2.48,0.006569119135547));
        ZList.Add(new ZValuePair(2.49,0.006387154764943));
        ZList.Add(new ZValuePair(2.5,0.006209665325776));
        ZList.Add(new ZValuePair(2.51,0.006036558080413));
        ZList.Add(new ZValuePair(2.52,0.005867741715333));
        ZList.Add(new ZValuePair(2.53,0.005703126332951));
        ZList.Add(new ZValuePair(2.54,0.005542623443083));
        ZList.Add(new ZValuePair(2.55,0.005386145954067));
        ZList.Add(new ZValuePair(2.56,0.005233608163556));
        ZList.Add(new ZValuePair(2.57,0.005084925748991));
        ZList.Add(new ZValuePair(2.58,0.004940015757771));
        ZList.Add(new ZValuePair(2.59,0.004798796597126));
        ZList.Add(new ZValuePair(2.6,0.004661188023719));
        ZList.Add(new ZValuePair(2.61,0.004527111132967));
        ZList.Add(new ZValuePair(2.62,0.004396488348121));
        ZList.Add(new ZValuePair(2.63,0.004269243409089));
        ZList.Add(new ZValuePair(2.64,0.004145301361036));
        ZList.Add(new ZValuePair(2.65,0.004024588542758));
        ZList.Add(new ZValuePair(2.66,0.003907032574853));
        ZList.Add(new ZValuePair(2.67,0.003792562347686));
        ZList.Add(new ZValuePair(2.68,0.003681108009175));
        ZList.Add(new ZValuePair(2.69,0.0035726009524));
        ZList.Add(new ZValuePair(2.7,0.003466973803041));
        ZList.Add(new ZValuePair(2.71,0.003364160406669));
        ZList.Add(new ZValuePair(2.72,0.003264095815891));
        ZList.Add(new ZValuePair(2.73,0.003166716277358));
        ZList.Add(new ZValuePair(2.74,0.003071959218651));
        ZList.Add(new ZValuePair(2.75,0.002979763235055));
        ZList.Add(new ZValuePair(2.76,0.002890068076226));
        ZList.Add(new ZValuePair(2.77,0.002802814632765));
        ZList.Add(new ZValuePair(2.78,0.002717944922701));
        ZList.Add(new ZValuePair(2.79,0.002635402077905));
        ZList.Add(new ZValuePair(2.8,0.002555130330428));
        ZList.Add(new ZValuePair(2.81,0.002477074998786));
        ZList.Add(new ZValuePair(2.82,0.002401182474189));
        ZList.Add(new ZValuePair(2.83,0.002327400206732));
        ZList.Add(new ZValuePair(2.84,0.002255676691542));
        ZList.Add(new ZValuePair(2.85,0.002185961454913));
        ZList.Add(new ZValuePair(2.86,0.002118205040405));
        ZList.Add(new ZValuePair(2.87,0.00205235899494));
        ZList.Add(new ZValuePair(2.88,0.001988375854894));
        ZList.Add(new ZValuePair(2.89,0.001926209132188));
        ZList.Add(new ZValuePair(2.9,0.001865813300384));
        ZList.Add(new ZValuePair(2.91,0.001807143780806));
        ZList.Add(new ZValuePair(2.92,0.001750156928676));
        ZList.Add(new ZValuePair(2.93,0.001694810019277));
        ZList.Add(new ZValuePair(2.94,0.001641061234157));
        ZList.Add(new ZValuePair(2.95,0.001588869647365));
        ZList.Add(new ZValuePair(2.96,0.001538195211738));
        ZList.Add(new ZValuePair(2.97,0.001488998745237));
        ZList.Add(new ZValuePair(2.98,0.00144124191734));
        ZList.Add(new ZValuePair(2.99,0.001394887235492));
        ZList.Add(new ZValuePair(3,0.00134989803163));
        ZList.Add(new ZValuePair(3.01,0.001306238448769));
        ZList.Add(new ZValuePair(3.02,0.001263873427672));
        ZList.Add(new ZValuePair(3.03,0.001222768693592));
        ZList.Add(new ZValuePair(3.04,0.001182890743104));
        ZList.Add(new ZValuePair(3.05,0.001144206831023));
        ZList.Add(new ZValuePair(3.06,0.001106684957409));
        ZList.Add(new ZValuePair(3.07,0.001070293854679));
        ZList.Add(new ZValuePair(3.08,0.001035002974803));
        ZList.Add(new ZValuePair(3.09,0.001000782476614));
        ZList.Add(new ZValuePair(3.1,0.000967603213218));
        ZList.Add(new ZValuePair(3.11,0.000935436719514));
        ZList.Add(new ZValuePair(3.12,0.000904255199822));
        ZList.Add(new ZValuePair(3.13,0.000874031515632));
        ZList.Add(new ZValuePair(3.14,0.000844739173459));
        ZList.Add(new ZValuePair(3.15,0.000816352312829));
        ZList.Add(new ZValuePair(3.16,0.000788845694376));
        ZList.Add(new ZValuePair(3.17,0.000762194688067));
        ZList.Add(new ZValuePair(3.18,0.000736375261554));
        ZList.Add(new ZValuePair(3.19,0.000711363968645));
        ZList.Add(new ZValuePair(3.2,0.000687137937916));
        ZList.Add(new ZValuePair(3.21,0.00066367486144));
        ZList.Add(new ZValuePair(3.22,0.00064095298366));
        ZList.Add(new ZValuePair(3.23,0.000618951090387));
        ZList.Add(new ZValuePair(3.24,0.000597648497934));
        ZList.Add(new ZValuePair(3.25,0.000577025042391));
        ZList.Add(new ZValuePair(3.26,0.000557061069025));
        ZList.Add(new ZValuePair(3.27,0.00053773742183));
        ZList.Add(new ZValuePair(3.28,0.000519035433207));
        ZList.Add(new ZValuePair(3.29,0.000500936913786));
        ZList.Add(new ZValuePair(3.3,0.000483424142384));
        ZList.Add(new ZValuePair(3.31,0.000466479856108));
        ZList.Add(new ZValuePair(3.32,0.000450087240592));
        ZList.Add(new ZValuePair(3.33,0.000434229920382));
        ZList.Add(new ZValuePair(3.34,0.00041889194945));
        ZList.Add(new ZValuePair(3.35,0.000404057801864));
        ZList.Add(new ZValuePair(3.36,0.000389712362582));
        ZList.Add(new ZValuePair(3.37,0.0003758409184));
        ZList.Add(new ZValuePair(3.38,0.000362429149033));
        ZList.Add(new ZValuePair(3.39,0.000349463118338));
        ZList.Add(new ZValuePair(3.4,0.000336929265677));
        ZList.Add(new ZValuePair(3.41,0.000324814397419));
        ZList.Add(new ZValuePair(3.42,0.000313105678581));
        ZList.Add(new ZValuePair(3.43,0.000301790624609));
        ZList.Add(new ZValuePair(3.44,0.000290857093291));
        ZList.Add(new ZValuePair(3.45,0.000280293276816));
        ZList.Add(new ZValuePair(3.46,0.000270087693963));
        ZList.Add(new ZValuePair(3.47,0.000260229182427));
        ZList.Add(new ZValuePair(3.48,0.00025070689128));
        ZList.Add(new ZValuePair(3.49,0.000241510273568));
        ZList.Add(new ZValuePair(3.5,0.000232629079036));
        ZList.Add(new ZValuePair(3.51,0.000224053346991));
        ZList.Add(new ZValuePair(3.52,0.000215773399295));
        ZList.Add(new ZValuePair(3.53,0.000207779833481));
        ZList.Add(new ZValuePair(3.54,0.000200063516007));
        ZList.Add(new ZValuePair(3.55,0.000192615575636));
        ZList.Add(new ZValuePair(3.56,0.000185427396933));
        ZList.Add(new ZValuePair(3.57,0.000178490613905));
        ZList.Add(new ZValuePair(3.58,0.000171797103746));
        ZList.Add(new ZValuePair(3.59,0.00016533898072));
        ZList.Add(new ZValuePair(3.6,0.000159108590158));
        ZList.Add(new ZValuePair(3.61,0.000153098502574));
        ZList.Add(new ZValuePair(3.62,0.000147301507907));
        ZList.Add(new ZValuePair(3.63,0.000141710609876));
        ZList.Add(new ZValuePair(3.64,0.000136319020446));
        ZList.Add(new ZValuePair(3.65,0.000131120154421));
        ZList.Add(new ZValuePair(3.66,0.000126107624139));
        ZList.Add(new ZValuePair(3.67,0.000121275234285));
        ZList.Add(new ZValuePair(3.68,0.000116616976815));
        ZList.Add(new ZValuePair(3.69,0.000112127025982));
        ZList.Add(new ZValuePair(3.7,0.000107799733477));
        ZList.Add(new ZValuePair(3.71,0.000103629623674));
        ZList.Add(new ZValuePair(3.72,9.9611388975962));
        ZList.Add(new ZValuePair(3.73,9.57398852688973));
        ZList.Add(new ZValuePair(3.74,9.20101274741736));
        ZList.Add(new ZValuePair(3.75,8.84172852008147));
        ZList.Add(new ZValuePair(3.76,8.49566784979494));
        ZList.Add(new ZValuePair(3.77,8.16237737026881));
        ZList.Add(new ZValuePair(3.78,7.84141793835902));
        ZList.Add(new ZValuePair(3.79,7.53236423787218));
        ZList.Add(new ZValuePair(3.8,7.23480439250856));
        ZList.Add(new ZValuePair(3.81,6.94833958798657));
        ZList.Add(new ZValuePair(3.82,6.6725837029713));
        ZList.Add(new ZValuePair(3.83,6.40716294888488));
        ZList.Add(new ZValuePair(3.84,6.15171551832105));
        ZList.Add(new ZValuePair(3.85,5.90589124189744));
        ZList.Add(new ZValuePair(3.86,5.66935125342338));
        ZList.Add(new ZValuePair(3.87,5.44176766337223));
        ZList.Add(new ZValuePair(3.88,5.22282324018075));
        ZList.Add(new ZValuePair(3.89,5.0122110996198));


    }
}
