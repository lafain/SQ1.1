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
                G.UIM.UpdateHeroStats(Hero1);
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
        Hero1.GetComponent<Placable>().cHealth = 100;
        Hero1.GetComponent<Placable>().MaxHealth = 100;
        G.UIM.UpdateHeroStats(Hero1);
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
                dProbability = cZVP.Probability * 100.0f;
                fProbability = Mathf.Round((float) dProbability * 100f) / 100f;
                Debug.Log("Probability is " + fProbability.ToString() + "%");

            }
            else if(ZScore < -3.89)
            {
                fProbability = 100.0f;
            }
            else if(ZScore > 3.89)
            {
                fProbability = 0.0f;
            }
        }

        G.UIM.DisplayNewInfoText(CurrentTurnActor.GetComponent<Placable>().Name + "'s Mean Attack Damage is " + CurrentTurnActor.GetComponent<Placable>().MeanDamage + 
            " and Standard Deviation is " + CurrentTurnActor.GetComponent<Placable>().DamageSD + ". " + ValidTarget.GetComponent<Placable>().Name + "'s Armor is " + ValidTarget.GetComponent<Placable>().cArmor + ".", true);
        G.UIM.DisplayNewInfoText(CurrentTurnActor.GetComponent<Placable>().Name+ "'s probability of besting thier Armor is " + fProbability.ToString() + "%.",true);

        int AttackValue = Mathf.RoundToInt(RandomFromDistribution.RandomNormalDistribution( CurrentTurnActor.GetComponent<Placable>().MeanDamage, 
            CurrentTurnActor.GetComponent<Placable>().DamageSD));
        bool ArmorDestroyed = false;

        if (ValidTarget.GetComponent<Placable>().cArmor > 0)
        {
            ArmorDestroyed = false;
            
            G.UIM.DisplayNewInfoText(CurrentTurnActor.GetComponent<Placable>().Name.ToString() + " attacks with " + AttackValue.ToString() + " against " + ValidTarget.GetComponent<Placable>().Name + "'s armor of " + ValidTarget.GetComponent<Placable>().cArmor + ".", true);
        }
        else
        {
            ArmorDestroyed = true;
            G.UIM.DisplayNewInfoText(CurrentTurnActor.GetComponent<Placable>().Name.ToString() + " attacks with " + AttackValue.ToString() + " against " + ValidTarget.GetComponent<Placable>().Name + "'s health of " + ValidTarget.GetComponent<Placable>().cHealth + ".", true);
        }

        Debug.Log("Target's health is " + ValidTarget.GetComponent<Placable>().cHealth);

        if(!ArmorDestroyed)
        {
            if (AttackValue >= ValidTarget.GetComponent<Placable>().cArmor)
            {
                ValidTarget.GetComponent<Placable>().cArmor = 0;
                Debug.Log("Attack Value against " + CurrentTurnActor.GetComponent<Placable>().Name + "'s Armor is " + AttackValue);
            }
            else
            {
                ValidTarget.GetComponent<Placable>().cArmor -= 1;
                Debug.Log("Attack Value against " + CurrentTurnActor.GetComponent<Placable>().Name + "'s Armor is -1");
                //ValidTarget.GetComponent<Placable>().cArmor = ValidTarget.GetComponent<Placable>().cArmor - ValidTarget.GetComponent<Placable>().cArmor * 
                //    Mathf.RoundToInt( 1 - (AttackValue / ValidTarget.GetComponent<Placable>().cArmor));
            }
        }
        else
        {
            ValidTarget.GetComponent<Placable>().cHealth -= AttackValue;
            Debug.Log("Attack Value against " + CurrentTurnActor.GetComponent<Placable>().Name + "'s Health is " + AttackValue);
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

        yield return new WaitForSecondsRealtime(0.5f);

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
                            G.UIM.UpdateHeroStats(Hero1);
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
                            yield return new WaitForSecondsRealtime(0.5f);

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
        ZList.Add(new ZValuePair(-3.89,0.999949878));
        ZList.Add(new ZValuePair(-3.88,0.999947772));
        ZList.Add(new ZValuePair(-3.87,0.999945582));
        ZList.Add(new ZValuePair(-3.86,0.999943306));
        ZList.Add(new ZValuePair(-3.85,0.999940941));
        ZList.Add(new ZValuePair(-3.84,0.999938483));
        ZList.Add(new ZValuePair(-3.83,0.999935928));
        ZList.Add(new ZValuePair(-3.82,0.999933274));
        ZList.Add(new ZValuePair(-3.81,0.999930517));
        ZList.Add(new ZValuePair(-3.8,0.999927652));
        ZList.Add(new ZValuePair(-3.79,0.999924676));
        ZList.Add(new ZValuePair(-3.78,0.999921586));
        ZList.Add(new ZValuePair(-3.77,0.999918376));
        ZList.Add(new ZValuePair(-3.76,0.999915043));
        ZList.Add(new ZValuePair(-3.75,0.999911583));
        ZList.Add(new ZValuePair(-3.74,0.99990799));
        ZList.Add(new ZValuePair(-3.73,0.99990426));
        ZList.Add(new ZValuePair(-3.72,0.999900389));
        ZList.Add(new ZValuePair(-3.71,0.99989637));
        ZList.Add(new ZValuePair(-3.7,0.9998922));
        ZList.Add(new ZValuePair(-3.69,0.999887873));
        ZList.Add(new ZValuePair(-3.68,0.999883383));
        ZList.Add(new ZValuePair(-3.67,0.999878725));
        ZList.Add(new ZValuePair(-3.66,0.999873892));
        ZList.Add(new ZValuePair(-3.65,0.99986888));
        ZList.Add(new ZValuePair(-3.64,0.999863681));
        ZList.Add(new ZValuePair(-3.63,0.999858289));
        ZList.Add(new ZValuePair(-3.62,0.999852698));
        ZList.Add(new ZValuePair(-3.61,0.999846901));
        ZList.Add(new ZValuePair(-3.6,0.999840891));
        ZList.Add(new ZValuePair(-3.59,0.999834661));
        ZList.Add(new ZValuePair(-3.58,0.999828203));
        ZList.Add(new ZValuePair(-3.57,0.999821509));
        ZList.Add(new ZValuePair(-3.56,0.999814573));
        ZList.Add(new ZValuePair(-3.55,0.999807384));
        ZList.Add(new ZValuePair(-3.54,0.999799936));
        ZList.Add(new ZValuePair(-3.53,0.99979222));
        ZList.Add(new ZValuePair(-3.52,0.999784227));
        ZList.Add(new ZValuePair(-3.51,0.999775947));
        ZList.Add(new ZValuePair(-3.5,0.999767371));
        ZList.Add(new ZValuePair(-3.49,0.99975849));
        ZList.Add(new ZValuePair(-3.48,0.999749293));
        ZList.Add(new ZValuePair(-3.47,0.999739771));
        ZList.Add(new ZValuePair(-3.46,0.999729912));
        ZList.Add(new ZValuePair(-3.45,0.999719707));
        ZList.Add(new ZValuePair(-3.44,0.999709143));
        ZList.Add(new ZValuePair(-3.43,0.999698209));
        ZList.Add(new ZValuePair(-3.42,0.999686894));
        ZList.Add(new ZValuePair(-3.41,0.999675186));
        ZList.Add(new ZValuePair(-3.4,0.999663071));
        ZList.Add(new ZValuePair(-3.39,0.999650537));
        ZList.Add(new ZValuePair(-3.38,0.999637571));
        ZList.Add(new ZValuePair(-3.37,0.999624159));
        ZList.Add(new ZValuePair(-3.36,0.999610288));
        ZList.Add(new ZValuePair(-3.35,0.999595942));
        ZList.Add(new ZValuePair(-3.34,0.999581108));
        ZList.Add(new ZValuePair(-3.33,0.99956577));
        ZList.Add(new ZValuePair(-3.32,0.999549913));
        ZList.Add(new ZValuePair(-3.31,0.99953352));
        ZList.Add(new ZValuePair(-3.3,0.999516576));
        ZList.Add(new ZValuePair(-3.29,0.999499063));
        ZList.Add(new ZValuePair(-3.28,0.999480965));
        ZList.Add(new ZValuePair(-3.27,0.999462263));
        ZList.Add(new ZValuePair(-3.26,0.999442939));
        ZList.Add(new ZValuePair(-3.25,0.999422975));
        ZList.Add(new ZValuePair(-3.24,0.999402352));
        ZList.Add(new ZValuePair(-3.23,0.999381049));
        ZList.Add(new ZValuePair(-3.22,0.999359047));
        ZList.Add(new ZValuePair(-3.21,0.999336325));
        ZList.Add(new ZValuePair(-3.2,0.999312862));
        ZList.Add(new ZValuePair(-3.19,0.999288636));
        ZList.Add(new ZValuePair(-3.18,0.999263625));
        ZList.Add(new ZValuePair(-3.17,0.999237805));
        ZList.Add(new ZValuePair(-3.16,0.999211154));
        ZList.Add(new ZValuePair(-3.15,0.999183648));
        ZList.Add(new ZValuePair(-3.14,0.999155261));
        ZList.Add(new ZValuePair(-3.13,0.999125968));
        ZList.Add(new ZValuePair(-3.12,0.999095745));
        ZList.Add(new ZValuePair(-3.11,0.999064563));
        ZList.Add(new ZValuePair(-3.1,0.999032397));
        ZList.Add(new ZValuePair(-3.09,0.998999218));
        ZList.Add(new ZValuePair(-3.08,0.998964997));
        ZList.Add(new ZValuePair(-3.07,0.998929706));
        ZList.Add(new ZValuePair(-3.06,0.998893315));
        ZList.Add(new ZValuePair(-3.05,0.998855793));
        ZList.Add(new ZValuePair(-3.04,0.998817109));
        ZList.Add(new ZValuePair(-3.03,0.998777231));
        ZList.Add(new ZValuePair(-3.02,0.998736127));
        ZList.Add(new ZValuePair(-3.01,0.998693762));
        ZList.Add(new ZValuePair(-3,0.998650102));
        ZList.Add(new ZValuePair(-2.99,0.998605113));
        ZList.Add(new ZValuePair(-2.98,0.998558758));
        ZList.Add(new ZValuePair(-2.97,0.998511001));
        ZList.Add(new ZValuePair(-2.96,0.998461805));
        ZList.Add(new ZValuePair(-2.95,0.99841113));
        ZList.Add(new ZValuePair(-2.94,0.998358939));
        ZList.Add(new ZValuePair(-2.93,0.99830519));
        ZList.Add(new ZValuePair(-2.92,0.998249843));
        ZList.Add(new ZValuePair(-2.91,0.998192856));
        ZList.Add(new ZValuePair(-2.9,0.998134187));
        ZList.Add(new ZValuePair(-2.89,0.998073791));
        ZList.Add(new ZValuePair(-2.88,0.998011624));
        ZList.Add(new ZValuePair(-2.87,0.997947641));
        ZList.Add(new ZValuePair(-2.86,0.997881795));
        ZList.Add(new ZValuePair(-2.85,0.997814039));
        ZList.Add(new ZValuePair(-2.84,0.997744323));
        ZList.Add(new ZValuePair(-2.83,0.9976726));
        ZList.Add(new ZValuePair(-2.82,0.997598818));
        ZList.Add(new ZValuePair(-2.81,0.997522925));
        ZList.Add(new ZValuePair(-2.8,0.99744487));
        ZList.Add(new ZValuePair(-2.79,0.997364598));
        ZList.Add(new ZValuePair(-2.78,0.997282055));
        ZList.Add(new ZValuePair(-2.77,0.997197185));
        ZList.Add(new ZValuePair(-2.76,0.997109932));
        ZList.Add(new ZValuePair(-2.75,0.997020237));
        ZList.Add(new ZValuePair(-2.74,0.996928041));
        ZList.Add(new ZValuePair(-2.73,0.996833284));
        ZList.Add(new ZValuePair(-2.72,0.996735904));
        ZList.Add(new ZValuePair(-2.71,0.99663584));
        ZList.Add(new ZValuePair(-2.7,0.996533026));
        ZList.Add(new ZValuePair(-2.69,0.996427399));
        ZList.Add(new ZValuePair(-2.68,0.996318892));
        ZList.Add(new ZValuePair(-2.67,0.996207438));
        ZList.Add(new ZValuePair(-2.66,0.996092967));
        ZList.Add(new ZValuePair(-2.65,0.995975411));
        ZList.Add(new ZValuePair(-2.64,0.995854699));
        ZList.Add(new ZValuePair(-2.63,0.995730757));
        ZList.Add(new ZValuePair(-2.62,0.995603512));
        ZList.Add(new ZValuePair(-2.61,0.995472889));
        ZList.Add(new ZValuePair(-2.6,0.995338812));
        ZList.Add(new ZValuePair(-2.59,0.995201203));
        ZList.Add(new ZValuePair(-2.58,0.995059984));
        ZList.Add(new ZValuePair(-2.57,0.994915074));
        ZList.Add(new ZValuePair(-2.56,0.994766392));
        ZList.Add(new ZValuePair(-2.55,0.994613854));
        ZList.Add(new ZValuePair(-2.54,0.994457377));
        ZList.Add(new ZValuePair(-2.53,0.994296874));
        ZList.Add(new ZValuePair(-2.52,0.994132258));
        ZList.Add(new ZValuePair(-2.51,0.993963442));
        ZList.Add(new ZValuePair(-2.5,0.993790335));
        ZList.Add(new ZValuePair(-2.49,0.993612845));
        ZList.Add(new ZValuePair(-2.48,0.993430881));
        ZList.Add(new ZValuePair(-2.47,0.993244347));
        ZList.Add(new ZValuePair(-2.46,0.993053149));
        ZList.Add(new ZValuePair(-2.45,0.992857189));
        ZList.Add(new ZValuePair(-2.44,0.992656369));
        ZList.Add(new ZValuePair(-2.43,0.992450589));
        ZList.Add(new ZValuePair(-2.42,0.992239746));
        ZList.Add(new ZValuePair(-2.41,0.99202374));
        ZList.Add(new ZValuePair(-2.4,0.991802464));
        ZList.Add(new ZValuePair(-2.39,0.991575814));
        ZList.Add(new ZValuePair(-2.38,0.991343681));
        ZList.Add(new ZValuePair(-2.37,0.991105957));
        ZList.Add(new ZValuePair(-2.36,0.990862532));
        ZList.Add(new ZValuePair(-2.35,0.990613294));
        ZList.Add(new ZValuePair(-2.34,0.99035813));
        ZList.Add(new ZValuePair(-2.33,0.990096924));
        ZList.Add(new ZValuePair(-2.32,0.989829561));
        ZList.Add(new ZValuePair(-2.31,0.989555923));
        ZList.Add(new ZValuePair(-2.3,0.98927589));
        ZList.Add(new ZValuePair(-2.29,0.988989342));
        ZList.Add(new ZValuePair(-2.28,0.988696156));
        ZList.Add(new ZValuePair(-2.27,0.988396208));
        ZList.Add(new ZValuePair(-2.26,0.988089375));
        ZList.Add(new ZValuePair(-2.25,0.987775527));
        ZList.Add(new ZValuePair(-2.24,0.987454539));
        ZList.Add(new ZValuePair(-2.23,0.987126279));
        ZList.Add(new ZValuePair(-2.22,0.986790616));
        ZList.Add(new ZValuePair(-2.21,0.986447419));
        ZList.Add(new ZValuePair(-2.2,0.986096552));
        ZList.Add(new ZValuePair(-2.19,0.985737882));
        ZList.Add(new ZValuePair(-2.18,0.985371269));
        ZList.Add(new ZValuePair(-2.17,0.984996577));
        ZList.Add(new ZValuePair(-2.16,0.984613665));
        ZList.Add(new ZValuePair(-2.15,0.984222393));
        ZList.Add(new ZValuePair(-2.14,0.983822617));
        ZList.Add(new ZValuePair(-2.13,0.983414193));
        ZList.Add(new ZValuePair(-2.12,0.982996977));
        ZList.Add(new ZValuePair(-2.11,0.982570822));
        ZList.Add(new ZValuePair(-2.1,0.982135579));
        ZList.Add(new ZValuePair(-2.09,0.9816911));
        ZList.Add(new ZValuePair(-2.08,0.981237234));
        ZList.Add(new ZValuePair(-2.07,0.980773828));
        ZList.Add(new ZValuePair(-2.06,0.98030073));
        ZList.Add(new ZValuePair(-2.05,0.979817785));
        ZList.Add(new ZValuePair(-2.04,0.979324837));
        ZList.Add(new ZValuePair(-2.03,0.97882173));
        ZList.Add(new ZValuePair(-2.02,0.978308306));
        ZList.Add(new ZValuePair(-2.01,0.977784406));
        ZList.Add(new ZValuePair(-2,0.977249868));
        ZList.Add(new ZValuePair(-1.99,0.976704532));
        ZList.Add(new ZValuePair(-1.98,0.976148236));
        ZList.Add(new ZValuePair(-1.97,0.975580815));
        ZList.Add(new ZValuePair(-1.96,0.975002105));
        ZList.Add(new ZValuePair(-1.95,0.97441194));
        ZList.Add(new ZValuePair(-1.94,0.973810155));
        ZList.Add(new ZValuePair(-1.93,0.973196581));
        ZList.Add(new ZValuePair(-1.92,0.97257105));
        ZList.Add(new ZValuePair(-1.91,0.971933393));
        ZList.Add(new ZValuePair(-1.9,0.97128344));
        ZList.Add(new ZValuePair(-1.89,0.97062102));
        ZList.Add(new ZValuePair(-1.88,0.969945961));
        ZList.Add(new ZValuePair(-1.87,0.969258091));
        ZList.Add(new ZValuePair(-1.86,0.968557237));
        ZList.Add(new ZValuePair(-1.85,0.967843225));
        ZList.Add(new ZValuePair(-1.84,0.967115881));
        ZList.Add(new ZValuePair(-1.83,0.966375031));
        ZList.Add(new ZValuePair(-1.82,0.965620498));
        ZList.Add(new ZValuePair(-1.81,0.964852106));
        ZList.Add(new ZValuePair(-1.8,0.964069681));
        ZList.Add(new ZValuePair(-1.79,0.963273044));
        ZList.Add(new ZValuePair(-1.78,0.96246202));
        ZList.Add(new ZValuePair(-1.77,0.96163643));
        ZList.Add(new ZValuePair(-1.76,0.960796097));
        ZList.Add(new ZValuePair(-1.75,0.959940843));
        ZList.Add(new ZValuePair(-1.74,0.959070491));
        ZList.Add(new ZValuePair(-1.73,0.958184862));
        ZList.Add(new ZValuePair(-1.72,0.957283779));
        ZList.Add(new ZValuePair(-1.71,0.956367063));
        ZList.Add(new ZValuePair(-1.7,0.955434537));
        ZList.Add(new ZValuePair(-1.69,0.954486023));
        ZList.Add(new ZValuePair(-1.68,0.953521342));
        ZList.Add(new ZValuePair(-1.67,0.952540318));
        ZList.Add(new ZValuePair(-1.66,0.951542774));
        ZList.Add(new ZValuePair(-1.65,0.950528532));
        ZList.Add(new ZValuePair(-1.64,0.949497417));
        ZList.Add(new ZValuePair(-1.63,0.948449252));
        ZList.Add(new ZValuePair(-1.62,0.947383862));
        ZList.Add(new ZValuePair(-1.61,0.946301072));
        ZList.Add(new ZValuePair(-1.6,0.945200708));
        ZList.Add(new ZValuePair(-1.59,0.944082597));
        ZList.Add(new ZValuePair(-1.58,0.942946567));
        ZList.Add(new ZValuePair(-1.57,0.941792444));
        ZList.Add(new ZValuePair(-1.56,0.940620059));
        ZList.Add(new ZValuePair(-1.55,0.939429242));
        ZList.Add(new ZValuePair(-1.54,0.938219823));
        ZList.Add(new ZValuePair(-1.53,0.936991636));
        ZList.Add(new ZValuePair(-1.52,0.935744512));
        ZList.Add(new ZValuePair(-1.51,0.934478288));
        ZList.Add(new ZValuePair(-1.5,0.933192799));
        ZList.Add(new ZValuePair(-1.49,0.931887882));
        ZList.Add(new ZValuePair(-1.48,0.930563377));
        ZList.Add(new ZValuePair(-1.47,0.929219123));
        ZList.Add(new ZValuePair(-1.46,0.927854963));
        ZList.Add(new ZValuePair(-1.45,0.92647074));
        ZList.Add(new ZValuePair(-1.44,0.9250663));
        ZList.Add(new ZValuePair(-1.43,0.92364149));
        ZList.Add(new ZValuePair(-1.42,0.922196159));
        ZList.Add(new ZValuePair(-1.41,0.920730159));
        ZList.Add(new ZValuePair(-1.4,0.919243341));
        ZList.Add(new ZValuePair(-1.39,0.917735561));
        ZList.Add(new ZValuePair(-1.38,0.916206678));
        ZList.Add(new ZValuePair(-1.37,0.914656549));
        ZList.Add(new ZValuePair(-1.36,0.913085038));
        ZList.Add(new ZValuePair(-1.35,0.911492009));
        ZList.Add(new ZValuePair(-1.34,0.909877328));
        ZList.Add(new ZValuePair(-1.33,0.908240864));
        ZList.Add(new ZValuePair(-1.32,0.906582491));
        ZList.Add(new ZValuePair(-1.31,0.904902082));
        ZList.Add(new ZValuePair(-1.3,0.903199515));
        ZList.Add(new ZValuePair(-1.29,0.901474671));
        ZList.Add(new ZValuePair(-1.28,0.899727432));
        ZList.Add(new ZValuePair(-1.27,0.897957685));
        ZList.Add(new ZValuePair(-1.26,0.896165319));
        ZList.Add(new ZValuePair(-1.25,0.894350226));
        ZList.Add(new ZValuePair(-1.24,0.892512303));
        ZList.Add(new ZValuePair(-1.23,0.890651448));
        ZList.Add(new ZValuePair(-1.22,0.888767563));
        ZList.Add(new ZValuePair(-1.21,0.886860554));
        ZList.Add(new ZValuePair(-1.2,0.88493033));
        ZList.Add(new ZValuePair(-1.19,0.882976804));
        ZList.Add(new ZValuePair(-1.18,0.880999893));
        ZList.Add(new ZValuePair(-1.17,0.878999516));
        ZList.Add(new ZValuePair(-1.16,0.876975597));
        ZList.Add(new ZValuePair(-1.15,0.874928064));
        ZList.Add(new ZValuePair(-1.14,0.872856849));
        ZList.Add(new ZValuePair(-1.13,0.870761888));
        ZList.Add(new ZValuePair(-1.12,0.868643119));
        ZList.Add(new ZValuePair(-1.11,0.866500487));
        ZList.Add(new ZValuePair(-1.1,0.864333939));
        ZList.Add(new ZValuePair(-1.09,0.862143428));
        ZList.Add(new ZValuePair(-1.08,0.85992891));
        ZList.Add(new ZValuePair(-1.07,0.857690346));
        ZList.Add(new ZValuePair(-1.06,0.8554277));
        ZList.Add(new ZValuePair(-1.05,0.853140944));
        ZList.Add(new ZValuePair(-1.04,0.85083005));
        ZList.Add(new ZValuePair(-1.03,0.848494997));
        ZList.Add(new ZValuePair(-1.02,0.84613577));
        ZList.Add(new ZValuePair(-1.01,0.843752355));
        ZList.Add(new ZValuePair(-1,0.841344746));
        ZList.Add(new ZValuePair(-0.99,0.83891294));
        ZList.Add(new ZValuePair(-0.98,0.836456941));
        ZList.Add(new ZValuePair(-0.97,0.833976754));
        ZList.Add(new ZValuePair(-0.96,0.831472393));
        ZList.Add(new ZValuePair(-0.95,0.828943874));
        ZList.Add(new ZValuePair(-0.94,0.82639122));
        ZList.Add(new ZValuePair(-0.93,0.823814458));
        ZList.Add(new ZValuePair(-0.92,0.82121362));
        ZList.Add(new ZValuePair(-0.91,0.818588745));
        ZList.Add(new ZValuePair(-0.9,0.815939875));
        ZList.Add(new ZValuePair(-0.89,0.813267057));
        ZList.Add(new ZValuePair(-0.88,0.810570345));
        ZList.Add(new ZValuePair(-0.87,0.807849798));
        ZList.Add(new ZValuePair(-0.86,0.805105479));
        ZList.Add(new ZValuePair(-0.85,0.802337457));
        ZList.Add(new ZValuePair(-0.84,0.799545807));
        ZList.Add(new ZValuePair(-0.83,0.796730608));
        ZList.Add(new ZValuePair(-0.82,0.793891946));
        ZList.Add(new ZValuePair(-0.81,0.791029912));
        ZList.Add(new ZValuePair(-0.8,0.788144601));
        ZList.Add(new ZValuePair(-0.79,0.785236116));
        ZList.Add(new ZValuePair(-0.78,0.782304562));
        ZList.Add(new ZValuePair(-0.77,0.779350054));
        ZList.Add(new ZValuePair(-0.76,0.776372708));
        ZList.Add(new ZValuePair(-0.75,0.773372648));
        ZList.Add(new ZValuePair(-0.74,0.770350003));
        ZList.Add(new ZValuePair(-0.73,0.767304908));
        ZList.Add(new ZValuePair(-0.72,0.764237502));
        ZList.Add(new ZValuePair(-0.71,0.761147932));
        ZList.Add(new ZValuePair(-0.7,0.758036348));
        ZList.Add(new ZValuePair(-0.69,0.754902906));
        ZList.Add(new ZValuePair(-0.68,0.75174777));
        ZList.Add(new ZValuePair(-0.67,0.748571105));
        ZList.Add(new ZValuePair(-0.66,0.745373085));
        ZList.Add(new ZValuePair(-0.65,0.742153889));
        ZList.Add(new ZValuePair(-0.64,0.7389137));
        ZList.Add(new ZValuePair(-0.63,0.735652708));
        ZList.Add(new ZValuePair(-0.62,0.732371107));
        ZList.Add(new ZValuePair(-0.61,0.729069096));
        ZList.Add(new ZValuePair(-0.6,0.725746882));
        ZList.Add(new ZValuePair(-0.59,0.722404675));
        ZList.Add(new ZValuePair(-0.58,0.719042691));
        ZList.Add(new ZValuePair(-0.57,0.715661151));
        ZList.Add(new ZValuePair(-0.56,0.712260281));
        ZList.Add(new ZValuePair(-0.55,0.708840313));
        ZList.Add(new ZValuePair(-0.54,0.705401484));
        ZList.Add(new ZValuePair(-0.53,0.701944035));
        ZList.Add(new ZValuePair(-0.52,0.698468212));
        ZList.Add(new ZValuePair(-0.51,0.694974269));
        ZList.Add(new ZValuePair(-0.5,0.691462461));
        ZList.Add(new ZValuePair(-0.49,0.687933051));
        ZList.Add(new ZValuePair(-0.48,0.684386303));
        ZList.Add(new ZValuePair(-0.47,0.680822491));
        ZList.Add(new ZValuePair(-0.46,0.67724189));
        ZList.Add(new ZValuePair(-0.45,0.67364478));
        ZList.Add(new ZValuePair(-0.44,0.670031446));
        ZList.Add(new ZValuePair(-0.43,0.666402179));
        ZList.Add(new ZValuePair(-0.42,0.662757273));
        ZList.Add(new ZValuePair(-0.41,0.659097026));
        ZList.Add(new ZValuePair(-0.4,0.655421742));
        ZList.Add(new ZValuePair(-0.39,0.651731727));
        ZList.Add(new ZValuePair(-0.38,0.648027292));
        ZList.Add(new ZValuePair(-0.37,0.644308755));
        ZList.Add(new ZValuePair(-0.36,0.640576433));
        ZList.Add(new ZValuePair(-0.35,0.636830651));
        ZList.Add(new ZValuePair(-0.34,0.633071736));
        ZList.Add(new ZValuePair(-0.33,0.629300019));
        ZList.Add(new ZValuePair(-0.32,0.625515835));
        ZList.Add(new ZValuePair(-0.31,0.621719522));
        ZList.Add(new ZValuePair(-0.3,0.617911422));
        ZList.Add(new ZValuePair(-0.29,0.614091881));
        ZList.Add(new ZValuePair(-0.28,0.610261248));
        ZList.Add(new ZValuePair(-0.27,0.606419873));
        ZList.Add(new ZValuePair(-0.26,0.602568113));
        ZList.Add(new ZValuePair(-0.25,0.598706326));
        ZList.Add(new ZValuePair(-0.24,0.594834872));
        ZList.Add(new ZValuePair(-0.23,0.590954115));
        ZList.Add(new ZValuePair(-0.22,0.587064423));
        ZList.Add(new ZValuePair(-0.21,0.583166163));
        ZList.Add(new ZValuePair(-0.2,0.579259709));
        ZList.Add(new ZValuePair(-0.19,0.575345435));
        ZList.Add(new ZValuePair(-0.18,0.571423716));
        ZList.Add(new ZValuePair(-0.17,0.567494932));
        ZList.Add(new ZValuePair(-0.16,0.563559463));
        ZList.Add(new ZValuePair(-0.15,0.559617692));
        ZList.Add(new ZValuePair(-0.14,0.555670005));
        ZList.Add(new ZValuePair(-0.13,0.551716787));
        ZList.Add(new ZValuePair(-0.12,0.547758426));
        ZList.Add(new ZValuePair(-0.11,0.543795313));
        ZList.Add(new ZValuePair(-0.1,0.539827837));
        ZList.Add(new ZValuePair(-0.09,0.535856393));
        ZList.Add(new ZValuePair(-0.08,0.531881372));
        ZList.Add(new ZValuePair(-0.07,0.52790317));
        ZList.Add(new ZValuePair(-0.06,0.523922183));
        ZList.Add(new ZValuePair(-0.05,0.519938806));
        ZList.Add(new ZValuePair(-0.04,0.515953437));
        ZList.Add(new ZValuePair(-0.03,0.511966473));
        ZList.Add(new ZValuePair(-0.02,0.507978314));
        ZList.Add(new ZValuePair(-0.01,0.503989356));
        ZList.Add(new ZValuePair(-0.009,0.503590432));
        ZList.Add(new ZValuePair(-0.008,0.503191504));
        ZList.Add(new ZValuePair(-0.007,0.502792573));
        ZList.Add(new ZValuePair(-0.006,0.502393639));
        ZList.Add(new ZValuePair(-0.005,0.501994703));
        ZList.Add(new ZValuePair(-0.004,0.501595765));
        ZList.Add(new ZValuePair(-0.003,0.501196825));
        ZList.Add(new ZValuePair(-0.002,0.500797884));
        ZList.Add(new ZValuePair(-0.001,0.500398942));
        ZList.Add(new ZValuePair(0,0.5));
        ZList.Add(new ZValuePair(0.001,0.499601058));
        ZList.Add(new ZValuePair(0.002,0.499202116));
        ZList.Add(new ZValuePair(0.003,0.498803175));
        ZList.Add(new ZValuePair(0.004,0.498404235));
        ZList.Add(new ZValuePair(0.005,0.498005297));
        ZList.Add(new ZValuePair(0.006,0.497606361));
        ZList.Add(new ZValuePair(0.007,0.497207427));
        ZList.Add(new ZValuePair(0.008,0.496808496));
        ZList.Add(new ZValuePair(0.009,0.496409568));
        ZList.Add(new ZValuePair(0.01,0.496010644));
        ZList.Add(new ZValuePair(0.02,0.492021686));
        ZList.Add(new ZValuePair(0.03,0.488033527));
        ZList.Add(new ZValuePair(0.04,0.484046563));
        ZList.Add(new ZValuePair(0.05,0.480061194));
        ZList.Add(new ZValuePair(0.06,0.476077817));
        ZList.Add(new ZValuePair(0.07,0.47209683));
        ZList.Add(new ZValuePair(0.08,0.468118628));
        ZList.Add(new ZValuePair(0.09,0.464143607));
        ZList.Add(new ZValuePair(0.1,0.460172163));
        ZList.Add(new ZValuePair(0.11,0.456204687));
        ZList.Add(new ZValuePair(0.12,0.452241574));
        ZList.Add(new ZValuePair(0.13,0.448283213));
        ZList.Add(new ZValuePair(0.14,0.444329995));
        ZList.Add(new ZValuePair(0.15,0.440382308));
        ZList.Add(new ZValuePair(0.16,0.436440537));
        ZList.Add(new ZValuePair(0.17,0.432505068));
        ZList.Add(new ZValuePair(0.18,0.428576284));
        ZList.Add(new ZValuePair(0.19,0.424654565));
        ZList.Add(new ZValuePair(0.2,0.420740291));
        ZList.Add(new ZValuePair(0.21,0.416833837));
        ZList.Add(new ZValuePair(0.22,0.412935577));
        ZList.Add(new ZValuePair(0.23,0.409045885));
        ZList.Add(new ZValuePair(0.24,0.405165128));
        ZList.Add(new ZValuePair(0.25,0.401293674));
        ZList.Add(new ZValuePair(0.26,0.397431887));
        ZList.Add(new ZValuePair(0.27,0.393580127));
        ZList.Add(new ZValuePair(0.28,0.389738752));
        ZList.Add(new ZValuePair(0.29,0.385908119));
        ZList.Add(new ZValuePair(0.3,0.382088578));
        ZList.Add(new ZValuePair(0.31,0.378280478));
        ZList.Add(new ZValuePair(0.32,0.374484165));
        ZList.Add(new ZValuePair(0.33,0.370699981));
        ZList.Add(new ZValuePair(0.34,0.366928264));
        ZList.Add(new ZValuePair(0.35,0.363169349));
        ZList.Add(new ZValuePair(0.36,0.359423567));
        ZList.Add(new ZValuePair(0.37,0.355691245));
        ZList.Add(new ZValuePair(0.38,0.351972708));
        ZList.Add(new ZValuePair(0.39,0.348268273));
        ZList.Add(new ZValuePair(0.4,0.344578258));
        ZList.Add(new ZValuePair(0.41,0.340902974));
        ZList.Add(new ZValuePair(0.42,0.337242727));
        ZList.Add(new ZValuePair(0.43,0.333597821));
        ZList.Add(new ZValuePair(0.44,0.329968554));
        ZList.Add(new ZValuePair(0.45,0.32635522));
        ZList.Add(new ZValuePair(0.46,0.32275811));
        ZList.Add(new ZValuePair(0.47,0.319177509));
        ZList.Add(new ZValuePair(0.48,0.315613697));
        ZList.Add(new ZValuePair(0.49,0.312066949));
        ZList.Add(new ZValuePair(0.5,0.308537539));
        ZList.Add(new ZValuePair(0.51,0.305025731));
        ZList.Add(new ZValuePair(0.52,0.301531788));
        ZList.Add(new ZValuePair(0.53,0.298055965));
        ZList.Add(new ZValuePair(0.54,0.294598516));
        ZList.Add(new ZValuePair(0.55,0.291159687));
        ZList.Add(new ZValuePair(0.56,0.287739719));
        ZList.Add(new ZValuePair(0.57,0.284338849));
        ZList.Add(new ZValuePair(0.58,0.280957309));
        ZList.Add(new ZValuePair(0.59,0.277595325));
        ZList.Add(new ZValuePair(0.6,0.274253118));
        ZList.Add(new ZValuePair(0.61,0.270930904));
        ZList.Add(new ZValuePair(0.62,0.267628893));
        ZList.Add(new ZValuePair(0.63,0.264347292));
        ZList.Add(new ZValuePair(0.64,0.2610863));
        ZList.Add(new ZValuePair(0.65,0.257846111));
        ZList.Add(new ZValuePair(0.66,0.254626915));
        ZList.Add(new ZValuePair(0.67,0.251428895));
        ZList.Add(new ZValuePair(0.68,0.24825223));
        ZList.Add(new ZValuePair(0.69,0.245097094));
        ZList.Add(new ZValuePair(0.7,0.241963652));
        ZList.Add(new ZValuePair(0.71,0.238852068));
        ZList.Add(new ZValuePair(0.72,0.235762498));
        ZList.Add(new ZValuePair(0.73,0.232695092));
        ZList.Add(new ZValuePair(0.74,0.229649997));
        ZList.Add(new ZValuePair(0.75,0.226627352));
        ZList.Add(new ZValuePair(0.76,0.223627292));
        ZList.Add(new ZValuePair(0.77,0.220649946));
        ZList.Add(new ZValuePair(0.78,0.217695438));
        ZList.Add(new ZValuePair(0.79,0.214763884));
        ZList.Add(new ZValuePair(0.8,0.211855399));
        ZList.Add(new ZValuePair(0.81,0.208970088));
        ZList.Add(new ZValuePair(0.82,0.206108054));
        ZList.Add(new ZValuePair(0.83,0.203269392));
        ZList.Add(new ZValuePair(0.84,0.200454193));
        ZList.Add(new ZValuePair(0.85,0.197662543));
        ZList.Add(new ZValuePair(0.86,0.194894521));
        ZList.Add(new ZValuePair(0.87,0.192150202));
        ZList.Add(new ZValuePair(0.88,0.189429655));
        ZList.Add(new ZValuePair(0.89,0.186732943));
        ZList.Add(new ZValuePair(0.9,0.184060125));
        ZList.Add(new ZValuePair(0.91,0.181411255));
        ZList.Add(new ZValuePair(0.92,0.17878638));
        ZList.Add(new ZValuePair(0.93,0.176185542));
        ZList.Add(new ZValuePair(0.94,0.17360878));
        ZList.Add(new ZValuePair(0.95,0.171056126));
        ZList.Add(new ZValuePair(0.96,0.168527607));
        ZList.Add(new ZValuePair(0.97,0.166023246));
        ZList.Add(new ZValuePair(0.98,0.163543059));
        ZList.Add(new ZValuePair(0.99,0.16108706));
        ZList.Add(new ZValuePair(1,0.158655254));
        ZList.Add(new ZValuePair(1.01,0.156247645));
        ZList.Add(new ZValuePair(1.02,0.15386423));
        ZList.Add(new ZValuePair(1.03,0.151505003));
        ZList.Add(new ZValuePair(1.04,0.14916995));
        ZList.Add(new ZValuePair(1.05,0.146859056));
        ZList.Add(new ZValuePair(1.06,0.1445723));
        ZList.Add(new ZValuePair(1.07,0.142309654));
        ZList.Add(new ZValuePair(1.08,0.14007109));
        ZList.Add(new ZValuePair(1.09,0.137856572));
        ZList.Add(new ZValuePair(1.1,0.135666061));
        ZList.Add(new ZValuePair(1.11,0.133499513));
        ZList.Add(new ZValuePair(1.12,0.131356881));
        ZList.Add(new ZValuePair(1.13,0.129238112));
        ZList.Add(new ZValuePair(1.14,0.127143151));
        ZList.Add(new ZValuePair(1.15,0.125071936));
        ZList.Add(new ZValuePair(1.16,0.123024403));
        ZList.Add(new ZValuePair(1.17,0.121000484));
        ZList.Add(new ZValuePair(1.18,0.119000107));
        ZList.Add(new ZValuePair(1.19,0.117023196));
        ZList.Add(new ZValuePair(1.2,0.11506967));
        ZList.Add(new ZValuePair(1.21,0.113139446));
        ZList.Add(new ZValuePair(1.22,0.111232437));
        ZList.Add(new ZValuePair(1.23,0.109348552));
        ZList.Add(new ZValuePair(1.24,0.107487697));
        ZList.Add(new ZValuePair(1.25,0.105649774));
        ZList.Add(new ZValuePair(1.26,0.103834681));
        ZList.Add(new ZValuePair(1.27,0.102042315));
        ZList.Add(new ZValuePair(1.28,0.100272568));
        ZList.Add(new ZValuePair(1.29,0.098525329));
        ZList.Add(new ZValuePair(1.3,0.096800485));
        ZList.Add(new ZValuePair(1.31,0.095097918));
        ZList.Add(new ZValuePair(1.32,0.093417509));
        ZList.Add(new ZValuePair(1.33,0.091759136));
        ZList.Add(new ZValuePair(1.34,0.090122672));
        ZList.Add(new ZValuePair(1.35,0.088507991));
        ZList.Add(new ZValuePair(1.36,0.086914962));
        ZList.Add(new ZValuePair(1.37,0.085343451));
        ZList.Add(new ZValuePair(1.38,0.083793322));
        ZList.Add(new ZValuePair(1.39,0.082264439));
        ZList.Add(new ZValuePair(1.4,0.080756659));
        ZList.Add(new ZValuePair(1.41,0.079269841));
        ZList.Add(new ZValuePair(1.42,0.077803841));
        ZList.Add(new ZValuePair(1.43,0.07635851));
        ZList.Add(new ZValuePair(1.44,0.0749337));
        ZList.Add(new ZValuePair(1.45,0.07352926));
        ZList.Add(new ZValuePair(1.46,0.072145037));
        ZList.Add(new ZValuePair(1.47,0.070780877));
        ZList.Add(new ZValuePair(1.48,0.069436623));
        ZList.Add(new ZValuePair(1.49,0.068112118));
        ZList.Add(new ZValuePair(1.5,0.066807201));
        ZList.Add(new ZValuePair(1.51,0.065521712));
        ZList.Add(new ZValuePair(1.52,0.064255488));
        ZList.Add(new ZValuePair(1.53,0.063008364));
        ZList.Add(new ZValuePair(1.54,0.061780177));
        ZList.Add(new ZValuePair(1.55,0.060570758));
        ZList.Add(new ZValuePair(1.56,0.059379941));
        ZList.Add(new ZValuePair(1.57,0.058207556));
        ZList.Add(new ZValuePair(1.58,0.057053433));
        ZList.Add(new ZValuePair(1.59,0.055917403));
        ZList.Add(new ZValuePair(1.6,0.054799292));
        ZList.Add(new ZValuePair(1.61,0.053698928));
        ZList.Add(new ZValuePair(1.62,0.052616138));
        ZList.Add(new ZValuePair(1.63,0.051550748));
        ZList.Add(new ZValuePair(1.64,0.050502583));
        ZList.Add(new ZValuePair(1.65,0.049471468));
        ZList.Add(new ZValuePair(1.66,0.048457226));
        ZList.Add(new ZValuePair(1.67,0.047459682));
        ZList.Add(new ZValuePair(1.68,0.046478658));
        ZList.Add(new ZValuePair(1.69,0.045513977));
        ZList.Add(new ZValuePair(1.7,0.044565463));
        ZList.Add(new ZValuePair(1.71,0.043632937));
        ZList.Add(new ZValuePair(1.72,0.042716221));
        ZList.Add(new ZValuePair(1.73,0.041815138));
        ZList.Add(new ZValuePair(1.74,0.040929509));
        ZList.Add(new ZValuePair(1.75,0.040059157));
        ZList.Add(new ZValuePair(1.76,0.039203903));
        ZList.Add(new ZValuePair(1.77,0.03836357));
        ZList.Add(new ZValuePair(1.78,0.03753798));
        ZList.Add(new ZValuePair(1.79,0.036726956));
        ZList.Add(new ZValuePair(1.8,0.035930319));
        ZList.Add(new ZValuePair(1.81,0.035147894));
        ZList.Add(new ZValuePair(1.82,0.034379502));
        ZList.Add(new ZValuePair(1.83,0.033624969));
        ZList.Add(new ZValuePair(1.84,0.032884119));
        ZList.Add(new ZValuePair(1.85,0.032156775));
        ZList.Add(new ZValuePair(1.86,0.031442763));
        ZList.Add(new ZValuePair(1.87,0.030741909));
        ZList.Add(new ZValuePair(1.88,0.030054039));
        ZList.Add(new ZValuePair(1.89,0.02937898));
        ZList.Add(new ZValuePair(1.9,0.02871656));
        ZList.Add(new ZValuePair(1.91,0.028066607));
        ZList.Add(new ZValuePair(1.92,0.02742895));
        ZList.Add(new ZValuePair(1.93,0.026803419));
        ZList.Add(new ZValuePair(1.94,0.026189845));
        ZList.Add(new ZValuePair(1.95,0.02558806));
        ZList.Add(new ZValuePair(1.96,0.024997895));
        ZList.Add(new ZValuePair(1.97,0.024419185));
        ZList.Add(new ZValuePair(1.98,0.023851764));
        ZList.Add(new ZValuePair(1.99,0.023295468));
        ZList.Add(new ZValuePair(2,0.022750132));
        ZList.Add(new ZValuePair(2.01,0.022215594));
        ZList.Add(new ZValuePair(2.02,0.021691694));
        ZList.Add(new ZValuePair(2.03,0.02117827));
        ZList.Add(new ZValuePair(2.04,0.020675163));
        ZList.Add(new ZValuePair(2.05,0.020182215));
        ZList.Add(new ZValuePair(2.06,0.01969927));
        ZList.Add(new ZValuePair(2.07,0.019226172));
        ZList.Add(new ZValuePair(2.08,0.018762766));
        ZList.Add(new ZValuePair(2.09,0.0183089));
        ZList.Add(new ZValuePair(2.1,0.017864421));
        ZList.Add(new ZValuePair(2.11,0.017429178));
        ZList.Add(new ZValuePair(2.12,0.017003023));
        ZList.Add(new ZValuePair(2.13,0.016585807));
        ZList.Add(new ZValuePair(2.14,0.016177383));
        ZList.Add(new ZValuePair(2.15,0.015777607));
        ZList.Add(new ZValuePair(2.16,0.015386335));
        ZList.Add(new ZValuePair(2.17,0.015003423));
        ZList.Add(new ZValuePair(2.18,0.014628731));
        ZList.Add(new ZValuePair(2.19,0.014262118));
        ZList.Add(new ZValuePair(2.2,0.013903448));
        ZList.Add(new ZValuePair(2.21,0.013552581));
        ZList.Add(new ZValuePair(2.22,0.013209384));
        ZList.Add(new ZValuePair(2.23,0.012873721));
        ZList.Add(new ZValuePair(2.24,0.012545461));
        ZList.Add(new ZValuePair(2.25,0.012224473));
        ZList.Add(new ZValuePair(2.26,0.011910625));
        ZList.Add(new ZValuePair(2.27,0.011603792));
        ZList.Add(new ZValuePair(2.28,0.011303844));
        ZList.Add(new ZValuePair(2.29,0.011010658));
        ZList.Add(new ZValuePair(2.3,0.01072411));
        ZList.Add(new ZValuePair(2.31,0.010444077));
        ZList.Add(new ZValuePair(2.32,0.010170439));
        ZList.Add(new ZValuePair(2.33,0.009903076));
        ZList.Add(new ZValuePair(2.34,0.00964187));
        ZList.Add(new ZValuePair(2.35,0.009386706));
        ZList.Add(new ZValuePair(2.36,0.009137468));
        ZList.Add(new ZValuePair(2.37,0.008894043));
        ZList.Add(new ZValuePair(2.38,0.008656319));
        ZList.Add(new ZValuePair(2.39,0.008424186));
        ZList.Add(new ZValuePair(2.4,0.008197536));
        ZList.Add(new ZValuePair(2.41,0.00797626));
        ZList.Add(new ZValuePair(2.42,0.007760254));
        ZList.Add(new ZValuePair(2.43,0.007549411));
        ZList.Add(new ZValuePair(2.44,0.007343631));
        ZList.Add(new ZValuePair(2.45,0.007142811));
        ZList.Add(new ZValuePair(2.46,0.006946851));
        ZList.Add(new ZValuePair(2.47,0.006755653));
        ZList.Add(new ZValuePair(2.48,0.006569119));
        ZList.Add(new ZValuePair(2.49,0.006387155));
        ZList.Add(new ZValuePair(2.5,0.006209665));
        ZList.Add(new ZValuePair(2.51,0.006036558));
        ZList.Add(new ZValuePair(2.52,0.005867742));
        ZList.Add(new ZValuePair(2.53,0.005703126));
        ZList.Add(new ZValuePair(2.54,0.005542623));
        ZList.Add(new ZValuePair(2.55,0.005386146));
        ZList.Add(new ZValuePair(2.56,0.005233608));
        ZList.Add(new ZValuePair(2.57,0.005084926));
        ZList.Add(new ZValuePair(2.58,0.004940016));
        ZList.Add(new ZValuePair(2.59,0.004798797));
        ZList.Add(new ZValuePair(2.6,0.004661188));
        ZList.Add(new ZValuePair(2.61,0.004527111));
        ZList.Add(new ZValuePair(2.62,0.004396488));
        ZList.Add(new ZValuePair(2.63,0.004269243));
        ZList.Add(new ZValuePair(2.64,0.004145301));
        ZList.Add(new ZValuePair(2.65,0.004024589));
        ZList.Add(new ZValuePair(2.66,0.003907033));
        ZList.Add(new ZValuePair(2.67,0.003792562));
        ZList.Add(new ZValuePair(2.68,0.003681108));
        ZList.Add(new ZValuePair(2.69,0.003572601));
        ZList.Add(new ZValuePair(2.7,0.003466974));
        ZList.Add(new ZValuePair(2.71,0.00336416));
        ZList.Add(new ZValuePair(2.72,0.003264096));
        ZList.Add(new ZValuePair(2.73,0.003166716));
        ZList.Add(new ZValuePair(2.74,0.003071959));
        ZList.Add(new ZValuePair(2.75,0.002979763));
        ZList.Add(new ZValuePair(2.76,0.002890068));
        ZList.Add(new ZValuePair(2.77,0.002802815));
        ZList.Add(new ZValuePair(2.78,0.002717945));
        ZList.Add(new ZValuePair(2.79,0.002635402));
        ZList.Add(new ZValuePair(2.8,0.00255513));
        ZList.Add(new ZValuePair(2.81,0.002477075));
        ZList.Add(new ZValuePair(2.82,0.002401182));
        ZList.Add(new ZValuePair(2.83,0.0023274));
        ZList.Add(new ZValuePair(2.84,0.002255677));
        ZList.Add(new ZValuePair(2.85,0.002185961));
        ZList.Add(new ZValuePair(2.86,0.002118205));
        ZList.Add(new ZValuePair(2.87,0.002052359));
        ZList.Add(new ZValuePair(2.88,0.001988376));
        ZList.Add(new ZValuePair(2.89,0.001926209));
        ZList.Add(new ZValuePair(2.9,0.001865813));
        ZList.Add(new ZValuePair(2.91,0.001807144));
        ZList.Add(new ZValuePair(2.92,0.001750157));
        ZList.Add(new ZValuePair(2.93,0.00169481));
        ZList.Add(new ZValuePair(2.94,0.001641061));
        ZList.Add(new ZValuePair(2.95,0.00158887));
        ZList.Add(new ZValuePair(2.96,0.001538195));
        ZList.Add(new ZValuePair(2.97,0.001488999));
        ZList.Add(new ZValuePair(2.98,0.001441242));
        ZList.Add(new ZValuePair(2.99,0.001394887));
        ZList.Add(new ZValuePair(3,0.001349898));
        ZList.Add(new ZValuePair(3.01,0.001306238));
        ZList.Add(new ZValuePair(3.02,0.001263873));
        ZList.Add(new ZValuePair(3.03,0.001222769));
        ZList.Add(new ZValuePair(3.04,0.001182891));
        ZList.Add(new ZValuePair(3.05,0.001144207));
        ZList.Add(new ZValuePair(3.06,0.001106685));
        ZList.Add(new ZValuePair(3.07,0.001070294));
        ZList.Add(new ZValuePair(3.08,0.001035003));
        ZList.Add(new ZValuePair(3.09,0.001000782));
        ZList.Add(new ZValuePair(3.1,0.000967603));
        ZList.Add(new ZValuePair(3.11,0.000935437));
        ZList.Add(new ZValuePair(3.12,0.000904255));
        ZList.Add(new ZValuePair(3.13,0.000874032));
        ZList.Add(new ZValuePair(3.14,0.000844739));
        ZList.Add(new ZValuePair(3.15,0.000816352));
        ZList.Add(new ZValuePair(3.16,0.000788846));
        ZList.Add(new ZValuePair(3.17,0.000762195));
        ZList.Add(new ZValuePair(3.18,0.000736375));
        ZList.Add(new ZValuePair(3.19,0.000711364));
        ZList.Add(new ZValuePair(3.2,0.000687138));
        ZList.Add(new ZValuePair(3.21,0.000663675));
        ZList.Add(new ZValuePair(3.22,0.000640953));
        ZList.Add(new ZValuePair(3.23,0.000618951));
        ZList.Add(new ZValuePair(3.24,0.000597648));
        ZList.Add(new ZValuePair(3.25,0.000577025));
        ZList.Add(new ZValuePair(3.26,0.000557061));
        ZList.Add(new ZValuePair(3.27,0.000537737));
        ZList.Add(new ZValuePair(3.28,0.000519035));
        ZList.Add(new ZValuePair(3.29,0.000500937));
        ZList.Add(new ZValuePair(3.3,0.000483424));
        ZList.Add(new ZValuePair(3.31,0.00046648));
        ZList.Add(new ZValuePair(3.32,0.000450087));
        ZList.Add(new ZValuePair(3.33,0.00043423));
        ZList.Add(new ZValuePair(3.34,0.000418892));
        ZList.Add(new ZValuePair(3.35,0.000404058));
        ZList.Add(new ZValuePair(3.36,0.000389712));
        ZList.Add(new ZValuePair(3.37,0.000375841));
        ZList.Add(new ZValuePair(3.38,0.000362429));
        ZList.Add(new ZValuePair(3.39,0.000349463));
        ZList.Add(new ZValuePair(3.4,0.000336929));
        ZList.Add(new ZValuePair(3.41,0.000324814));
        ZList.Add(new ZValuePair(3.42,0.000313106));
        ZList.Add(new ZValuePair(3.43,0.000301791));
        ZList.Add(new ZValuePair(3.44,0.000290857));
        ZList.Add(new ZValuePair(3.45,0.000280293));
        ZList.Add(new ZValuePair(3.46,0.000270088));
        ZList.Add(new ZValuePair(3.47,0.000260229));
        ZList.Add(new ZValuePair(3.48,0.000250707));
        ZList.Add(new ZValuePair(3.49,0.00024151));
        ZList.Add(new ZValuePair(3.5,0.000232629));
        ZList.Add(new ZValuePair(3.51,0.000224053));
        ZList.Add(new ZValuePair(3.52,0.000215773));
        ZList.Add(new ZValuePair(3.53,0.00020778));
        ZList.Add(new ZValuePair(3.54,0.000200064));
        ZList.Add(new ZValuePair(3.55,0.000192616));
        ZList.Add(new ZValuePair(3.56,0.000185427));
        ZList.Add(new ZValuePair(3.57,0.000178491));
        ZList.Add(new ZValuePair(3.58,0.000171797));
        ZList.Add(new ZValuePair(3.59,0.000165339));
        ZList.Add(new ZValuePair(3.6,0.000159109));
        ZList.Add(new ZValuePair(3.61,0.000153099));
        ZList.Add(new ZValuePair(3.62,0.000147302));
        ZList.Add(new ZValuePair(3.63,0.000141711));
        ZList.Add(new ZValuePair(3.64,0.000136319));
        ZList.Add(new ZValuePair(3.65,0.00013112));
        ZList.Add(new ZValuePair(3.66,0.000126108));
        ZList.Add(new ZValuePair(3.67,0.000121275));
        ZList.Add(new ZValuePair(3.68,0.000116617));
        ZList.Add(new ZValuePair(3.69,0.000112127));
        ZList.Add(new ZValuePair(3.7,0.0001078));
        ZList.Add(new ZValuePair(3.71,0.00010363));
        ZList.Add(new ZValuePair(3.72,0.0000996114));
        ZList.Add(new ZValuePair(3.73, 0.0000957399));
        ZList.Add(new ZValuePair(3.74, 0.0000920101));
        ZList.Add(new ZValuePair(3.75, 0.0000884173));
        ZList.Add(new ZValuePair(3.76, 0.0000849567));
        ZList.Add(new ZValuePair(3.77, 0.0000816238));
        ZList.Add(new ZValuePair(3.78, 0.0000784142));
        ZList.Add(new ZValuePair(3.79, 0.0000753236));
        ZList.Add(new ZValuePair(3.8, 0.000072348));
        ZList.Add(new ZValuePair(3.81, 0.0000694834));
        ZList.Add(new ZValuePair(3.82, 0.0000667258));
        ZList.Add(new ZValuePair(3.83, 0.0000640716));
        ZList.Add(new ZValuePair(3.84, 0.0000615172));
        ZList.Add(new ZValuePair(3.85, 0.0000590589));
        ZList.Add(new ZValuePair(3.86, 0.0000566935));
        ZList.Add(new ZValuePair(3.87, 0.0000544177));
        ZList.Add(new ZValuePair(3.88, 0.0000522282));
        ZList.Add(new ZValuePair(3.89, 0.0000501221));

    }
}
