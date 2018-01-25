using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public Camera WorldCamera;
    
    public Text NameText;
    public Text HealthText;
    public Text ArmorText;
    public Text APText;

    public Text MoveText;
    public Text AttackText;
    public Text FortifyText;
    public Text EquipText;
    public Text ItemText;
    public Text DetectText;
    public Text EndTurnText;

    public Text MoveAPText;
    public Text AttackAPText;
    public Text FortifyAPText;
    public Text EquipAPText;
    public Text ItemAPText;
    public Text DetectAPText;

    public RectTransform PlayerPanel;
    public RectTransform BadPanel;
    public RectTransform InfoPanel;

    public Text InfoText;

    public Text BadNameText;
    public Text BadHealthText;
    public Text BadArmorText;
    public Text BadAPText;
    
    bool Acting;
    bool NextInfo;
    bool RequireNext;

    List<string> InfoPanelUpdates;
    public int InfoPanelTimer;

    public enum ActingState
    {
        Moving, Attacking, Fortifying, Equiping, Iteming, Detecting, None
    }

    public ActingState AS;

    // Use this for initialization
    void Start () {
        RequireNext = false;
        NextInfo = false;
        InfoPanelTimer = 0;
        InfoPanelUpdates = new List<string>();
        G.UIM = this;
        Acting = false;
        AS = ActingState.None;
	}
	
	// Update is called once per frame
	void Update () {
        if(InfoPanelUpdates.Count != 0)
        {
            InfoPanel.gameObject.SetActive(true);
            InfoPanelTimer--;
            InfoText.text = InfoPanelUpdates[0];

            if(RequireNext)
            {
                if(NextInfo)
                {
                    InfoPanelUpdates.RemoveAt(0);
                    InfoPanelTimer = 250;
                    NextInfo = false;
                }
            }
            else
            if(InfoPanelTimer == 0)
            {
                InfoPanelUpdates.RemoveAt(0);
                InfoPanelTimer = 250;
            }
        }
        else
        {
            InfoPanel.gameObject.SetActive(false);
        }

        Ray mRay = WorldCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit mHit;

        if (Physics.Raycast(mRay, out mHit))
        {
            if(G.GM.GetHoveredPlacable(mHit.collider.gameObject))
            {
                BadNameText.text = mHit.collider.gameObject.GetComponent<Placable>().name;
                BadHealthText.text = mHit.collider.gameObject.GetComponent<Placable>().cHealth.ToString();
                BadArmorText.text = mHit.collider.gameObject.GetComponent<Placable>().cArmor.ToString();
                BadAPText.text = mHit.collider.gameObject.GetComponent<Placable>().cAP.ToString();
                BadPanel.gameObject.SetActive(true);
            }
            else
            {
                BadPanel.gameObject.SetActive(false);
            }
        }

        if (Input.GetMouseButtonDown(0) && Acting && AS == ActingState.Moving)
        {
            StartCoroutine(G.GM.MoveDestinationClicked());
        }

        if (AS == ActingState.Moving)
        {
            mRay = WorldCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mRay, out mHit))
            {
                MoveAPText.text = G.GM.GetHoveredMoveCost(mHit.collider.gameObject);
            }
        }
        else
        {
            MoveAPText.text = G.GM.GetCurrentActorAPCostPerTile().ToString() + " per tile";
        }
        
        if (Input.GetMouseButtonDown(0) && Acting && AS == ActingState.Attacking)
        {
            if (Physics.Raycast(mRay, out mHit))
            {
                StartCoroutine(G.GM.AttackTargetClicked(mHit.collider.gameObject));
            }
        }
                
        if(Input.mousePosition.x < Screen.width * 0.03f)
        {
            WorldCamera.gameObject.transform.position += -WorldCamera.gameObject.transform.right * 0.1f;
        }

        if (Input.mousePosition.x > Screen.width * 0.97f)
        {
            WorldCamera.gameObject.transform.position += WorldCamera.gameObject.transform.right * 0.1f;
        }

        if (Input.mousePosition.y < Screen.height * 0.07f)
        {
            WorldCamera.gameObject.transform.position += Vector3.RotateTowards(-WorldCamera.gameObject.transform.right,Vector3.forward, 1.5708f, 1.0f) * 0.1f;
        }

        if (Input.mousePosition.y > Screen.height * 0.93f)
        {
            WorldCamera.gameObject.transform.position += -Vector3.RotateTowards(-WorldCamera.gameObject.transform.right, Vector3.forward, 1.5708f, 1.0f) * 0.1f;
        }

        if (Physics.Raycast(mRay, out mHit))
        {
            if( G.GM.IsMouseOverBad(mHit.collider.gameObject))
            {

            }
        }
    }
        
    public void MoveClicked()
    {
        if (!Acting)
        {
            Acting = true;
            AS = ActingState.Moving;
            MoveText.text = "> Move";
            G.GM.ToggleMoving(true);
        }
        else if(AS == ActingState.Moving)
        {
            G.GM.ToggleMoving(false);
            Acting = false;
            AS = ActingState.None;
            MoveText.text = "Move";
        }
    }

    public void AttackClicked()
    {
        if (!Acting && G.GM.Hero1.GetComponent<Placable>().cAP >= 4)
        {
            Acting = true;
            AS = ActingState.Attacking;
            AttackText.text = "> Attack";
            G.GM.ToggleAttack(true);
        }
        else if (AS == ActingState.Attacking)
        {
            Acting = false;
            AS = ActingState.None;
            AttackText.text = "Attack";
        }
    }

    public void FortifyClicked()
    {
        if (!Acting)
        {
            Acting = true;
            AS = ActingState.Fortifying;
            FortifyText.text = "> Fortity";
        }
        else if (AS == ActingState.Fortifying)
        {
            Acting = false;
            AS = ActingState.None;
            FortifyText.text = "Fortify";
        }
    }

    public void EquipClicked()
    {
        if (!Acting)
        {
            Acting = true;
            AS = ActingState.Equiping;
            EquipText.text = "> Equip";
        }
        else if (AS == ActingState.Equiping)
        {
            Acting = false;
            AS = ActingState.None;
            EquipText.text = "Equip";
        }
    }

    public void ItemClicked()
    {
        if (!Acting)
        {
            Acting = true;
            AS = ActingState.Iteming;
            ItemText.text = "> Item";
        }
        else if (AS == ActingState.Iteming)
        {
            Acting = false;
            AS = ActingState.None;
            ItemText.text = "Item";
        }
    }

    public void DetectClicked()
    {
        if (!Acting)
        {
            Acting = true;
            AS = ActingState.Detecting;
            DetectText.text = "> Detect";
        }
        else if (AS == ActingState.Detecting)
        {
            Acting = false;
            AS = ActingState.None;
            ItemText.text = "Detect";
        }
    }

    public void EndTurnClicked()
    {
        //confirm with user

        DoneActing();
        G.GM.CurrentTurnActor.GetComponent<Placable>().TurnDone = true;
    }

    public void DoneActing()
    {
        
            Acting = false;
            AS = ActingState.None;

            MoveText.text = "Move";
            AttackText.text = "Attack";
            FortifyText.text = "Fortify";
            EquipText.text = "Equip";
            ItemText.text = "Item";
       
    }

    public void UpdateHeroCAP(int newCAP)
    {
        APText.text = newCAP.ToString();
    }

    public void TogglePlayerMenu(bool Enable)
    {
        if(Enable)
        {
            PlayerPanel.gameObject.SetActive(true);
        }
        else
        {
            PlayerPanel.gameObject.SetActive(false);
        }
    }

    public void DisplayNewInfoText(string NewInfo, bool _RequireNext)
    {
        RequireNext = _RequireNext;

        if(InfoPanelUpdates.Count == 0)
        {
            
            InfoPanelTimer = 250;
            
        }
        InfoPanelUpdates.Add(NewInfo);
       
    }

    public void OnNextButtonClicked()
    {
        NextInfo = true;
    }

}
