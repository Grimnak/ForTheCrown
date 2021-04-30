/* 
 * Author: Garrett Morse
 * Last Edited By: Will Bartlett
 * Date Created: 3-14-2021
 * Description: Game controller for human players.
 * Filename: PlayerController.cs
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

namespace Capstone
{
public class PlayerController : GameController
{
    public int controllerID;

    [HideInInspector]
    public bool isReadyToPlay;
    [SerializeField]
    public int allegiance { get; set; }
    [HideInInspector]
    public bool movePhase;

    [SerializeField]
    public List<GameObject> popArmy { get; set; }
    public Stack<GameObject> placedUnits { get; set; }
    public PhotonView PV;
    public Player photonPlayer;
    public PopulationSelectLogicManager populationLogicScript;
    [SerializeField]
    private string[] unitNames; // for mp pop army communication
    private static bool player0Ready;
    private static bool player1Ready;
    public int enemiesKilledByPlayer;
    public int unitsLostByPlayer;
    public int AttacksThrown;
    public string endResult;
    private ResultsPanelScript resultsScript;

        void Start()
        {
        PV = gameObject.GetComponent<PhotonView>();
        isReadyToPlay = false;
        movePhase = true;
        if (PhotonNetwork.IsConnected) // figure out allegiance values
        {
            if (PhotonNetwork.IsMasterClient)
            { // if we're the host, then our PC is 0 and enemy is 1
                if (PV.IsMine) { GameLogicManager.Instance.myID = 0; allegiance = 0; }
                else { GameLogicManager.Instance.otherID = 1; allegiance = 1; }
            }
            else // else if we're not the host, then our PC is 1 and the enemy is 0
            {
                if (PV.IsMine) { GameLogicManager.Instance.myID = 1; allegiance = 1; }
                else { GameLogicManager.Instance.otherID = 0; allegiance = 0; }
            }
        }
        else {
            allegiance = 0;
            GameLogicManager.Instance.myID = 0;
        }
        controllerID = allegiance;
        gameArmy = new List<GameObject>();
        popArmy = new List<GameObject>();
        placedUnits = new Stack<GameObject>();
        GameLogicManager.Instance.controllers[controllerID] = this;
        if (!PhotonNetwork.IsConnected || PV.IsMine)
        {
            populationLogicScript = GameObject.Find("Population Select Manager").GetComponent<PopulationSelectLogicManager>();
            GameLogicManager.Instance.PCsAreReady = true;
            Spawn();
        } 
        else if (PhotonNetwork.IsConnected && !PV.IsMine) PV.RPC("ReadyToReceive", RpcTarget.All, allegiance);
    }
    
    void Update()
    {
        if (PhotonNetwork.IsConnected && PV.IsMine)
        {
            if (player0Ready && player1Ready)
            {
                GameLogicManager.Instance.PCsAreReady = true;
                player0Ready = false; player1Ready = false;
                Debug.Log("Units sent over: " + string.Join(", ", unitNames));
                GameLogicManager.Instance.photonView.RPC("HandleNetworkedUnits", RpcTarget.Others, unitNames, this.allegiance);
                
                // target is others and not all because we already know what our army is. just have to tell the other guy.
            }
        }
        // Perform an action provided that it is currently the player's turn.
        if (GameLogicManager.IsMyTurn(this.allegiance))
        {
            GameObject currentSelection = TileManager.FindSpecificChildByTag(TileManager.currentlySelectedTile, "Unit");
            
            // Perform an action provided that a unit is selected and the appropriate target exists.
            if (!GameLogicManager.animationLock && currentSelection != null)
            {
                TakeAction(currentSelection);
            }
        }
    }

    /// <param name="_cs">Current Selection</param>
    /// <summary>Given the player's current selection, determine whether we issue an attack, move or heal.</summary>
    void TakeAction(GameObject _cs)
    {
        UnitController csUnit = _cs.GetComponent<UnitController>();
        GameObject currentTarget = TileManager.FindSpecificChildByTag(TileManager.currentlyTargetedTile, "Unit");
        // Perform an action provided that the player is in their combat phase, the unit has not yet acted this turn, and the target is alive.
        if (!movePhase && currentTarget != null && !csUnit.hasActed && currentTarget.GetComponent<UnitController>().currentHealth > 0)
        {
            if (csUnit.allegiance != currentTarget?.GetComponent<UnitController>().allegiance)
            {
                if (PhotonNetwork.IsConnected) PV.RPC("Attack", RpcTarget.All, TileManager.currentlySelectedTile.name, TileManager.currentlyTargetedTile.name);
                else Attack(TileManager.currentlySelectedTile.name, TileManager.currentlyTargetedTile.name);
            }
            else
            {
                if (PhotonNetwork.IsConnected) PV.RPC("Heal", RpcTarget.All, TileManager.currentlySelectedTile.name, TileManager.currentlyTargetedTile.name);
                else Heal(TileManager.currentlySelectedTile.name, TileManager.currentlyTargetedTile.name);
            }
        }

        // Perform a move provided that the player is in their move phase and an empty tile is targeted.
        if (movePhase && TileManager.currentlyTargetedTile != null && currentTarget == null)
        {
            if (PhotonNetwork.IsConnected) PV.RPC("Move", RpcTarget.All, TileManager.currentlySelectedTile.name, TileManager.currentlyTargetedTile.name);
            else Move(TileManager.currentlySelectedTile.name, TileManager.currentlyTargetedTile.name);
        }
    }

    [PunRPC]
    void ReadyToReceive(int _controllerID)
    {
        if (_controllerID == 0) player0Ready = true;
        else player1Ready = true;
    }

    public void TogglePhase()
    {
        TileManager.DeselectCurrentSelection();
        movePhase = !movePhase;
    }

    public void SetPlayerUnitList()
    {
        int unitNumber = 0;

        while (placedUnits.Count > 0)
        {
            gameArmy.Add(placedUnits.Pop());

            // activate obfuscated opponent units
            if (controllerID != GameLogicManager.Instance.myID)
            {
                GameObject unit = (GameLogicManager.Instance.controllers[controllerID] as PlayerController).gameArmy[unitNumber];
                unit.SetActive(true);

                //Set allegiance UI
                if (controllerID == 0)
                {
                    unit.transform.Find("blue_team_aura").GetComponent<ParticleSystem>().Play();
                }
                else
                {
                    unit.transform.Find("red_team_aura").GetComponent<ParticleSystem>().Play();
                }
            }

            unitNumber++;
        }
    }

    [PunRPC]
    public override void Attack(string _me, string _target)
    {
        UnitController myUC = TileManager.FindSpecificChildByTag(GameObject.Find(_me), "Unit").GetComponent<UnitController>();
        myUC.target = TileManager.FindSpecificChildByTag(GameObject.Find(_target), "Unit");
        myUC.selected = true;
        myUC.Attack();
        AttacksThrown++;
        myUC.selected = false;
        TileManager.DeselectCurrentSelection();
    }
    
    public override void Initialize()
    {
        GameLogicManager.Instance.controllers[controllerID] = this;
    }

    [PunRPC]
    public void Special(string _me)
    {
        Debug.Log("Special: " + _me + " from " + this.PV.Owner);
        TileManager.FindSpecificChildByTag(GameObject.Find(_me), "Unit").GetComponent<UnitController>().UseSpecialAbility();
    }

    [PunRPC]
    public override void Move(string _me, string _target)
    {
        GameObject me = GameObject.Find(_me);
        GameObject target = GameObject.Find(_target);
        GameObject myUnit = TileManager.FindSpecificChildByTag(me, "Unit");
        UnitController myUC = myUnit.GetComponent<UnitController>();
        myUC.target = target;
        myUC.selected = true;
        myUC.Move();
        me.tag = Global.Tags.inactive;
        TileManager.FindSpecificChildByTag(me, "Reticle").SetActive(false); // patch for reticle remaining after an input
        myUC.selected = false;
        myUC.currentMovementRange -= TileManager.MoveDistanceBetweenTiles(me, target);
        myUC.statusData.MovingRange = myUC.currentMovementRange;
        myUnit.transform.parent = target.transform;
        target.tag = Global.Tags.occupied;
        TileManager.DeselectCurrentSelection();
    }

    [PunRPC]
    public override void Heal(string _me, string _target)
    {
        UnitController myUC = TileManager.FindSpecificChildByTag(GameObject.Find(_me), "Unit").GetComponent<UnitController>();
        myUC.target = TileManager.FindSpecificChildByTag(GameObject.Find(_target), "Unit");
        myUC.selected = true;
        myUC.Heal();
        myUC.selected = false;
        TileManager.DeselectCurrentSelection();
    }

    public override void Spawn(bool inStagingArea = false)
    {
        var _ud = populationLogicScript.unitDict;
        CombatUIController.instance.GetUnitDict(ref _ud);    //temporary solution for UI to monitor unitDict
        foreach (var pair in _ud)
        {
            for (int val = 0; val < pair.Value; val++)
            {
                GameObject unit = Instantiate(Resources.Load<GameObject>(pair.Key));
                unit.name = pair.Key + this.allegiance;
                popArmy.Add(unit);
                unit.SetActive(false);
            }
        }
        unitNames = (from go in popArmy select go.name).ToArray();
    }
        public void ShowEndlessResultsPanel()
        {
            resultsScript = GameLogicManager.Instance.getResultsPanelScript();
            resultsScript.ShowEndlessPanel();
        }

        public void ShowTrainingLossPanel()
        {

            resultsScript = GameLogicManager.Instance.getResultsPanelScript();
            resultsScript.ShowTrainingLossPanel();
        }

        public void ShowTrainingWinPanel()
        {

            resultsScript = GameLogicManager.Instance.getResultsPanelScript();
            resultsScript.ShowTrainingWinPanel();
        }

        public void ShowMPWinResultsPanel()
        {
            this.endResult = "You are victorious.";
            resultsScript = GameLogicManager.Instance.getResultsPanelScript();
            resultsScript.ShowMPWinPanel();
        }

        public void ShowMPLoseResultsPanel()
        {
            this.endResult = "You have been defeated.";
            resultsScript = GameLogicManager.Instance.getResultsPanelScript();
            resultsScript.ShowMPLosePanel();
        }
    }
}