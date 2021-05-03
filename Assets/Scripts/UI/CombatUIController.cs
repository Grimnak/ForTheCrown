/* 
 * Author:  Shengyu Jin
 * Last Edited By:  Will Bartlett
 * Date Created:  not sure
 * Description:  This script handles UI related to combat.
 * Filename:  CombatUIController.cs
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.EventSystems;

namespace Capstone
{
    public class CombatUIController : MonoBehaviourPun
    {
        public static CombatUIController instance;
        public Canvas rootCanvas;
        [SerializeField] private GameObject unitProfilePanel;
        [SerializeField] private Canvas combatUICanvas;
        [SerializeField] private Canvas popuUICanvas;
        [SerializeField] private GameObject selectedUnitStatusPanel;
        [SerializeField] private GameObject previewUnitStatusPanel;
        [SerializeField] private GameObject structurePanel;
        [SerializeField] private GameObject actionBar;
        [SerializeField] private Button[] actionButtons;
        [SerializeField] private Text turnCounterTxt;
        [SerializeField] private Text turnEndBtnTxt;
        [SerializeField] private GameObject[] Containers;
        [SerializeField] private Button startBtn;
        [SerializeField] private Button undoBtn;
        [SerializeField] private UICircle ConfirmedActionUI;
        [SerializeField] private Text populationText;
        

        public bool clientReady; // in multiplayer, if client army is ready for battle
        private Coroutine panelCoroutine = null;
        private Dictionary<string, int> placeableUnitDict;
        private Coroutine checkPlaceableUnitsCorutine;
        private Coroutine endlessModePlaceUnitsCoroutine;
        public UnitController selectedUnitController;
        private int selectedAction;
        private bool sent;
        private string healingStructInfo;
        private string attackRangeStructInfo;
        private string movementRangeStructInfo;
        private string obstacleInfo;

        private void Awake()
        {
            PopulationUI.instance.SetGraphicRaycaster(popuUICanvas.gameObject);
            instance = this;    //singleton
            sent = false;
        }
        // Start is called before the first frame update
        void Start()
        {
            SwitchUIDisplay(false, true);
            //placeableUnitDict = GameObject.Find("Population Select Manager").GetComponent<PopulationSelectLogicManager>().unitDict;
            startBtn.gameObject.SetActive(false);
            selectedAction = 0; //temporarily only special ability can be counted as action
            setUpTexts();
        }

        // Update is called once per frame
        void Update()
        {
            if (GameLogicManager.Instance.isReadyToPlay)
            {
                int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
                PlayerController pc = GameLogicManager.Instance.controllers[id] as PlayerController;
                turnCounterTxt.text = "Turn : " + GameLogicManager.turnCounter;
                if (pc.movePhase)
                {
                    turnEndBtnTxt.text = "Fight!";
                }
                else
                {
                    turnEndBtnTxt.text = "End Turn";
                }
            }
            else if (GameLogicManager.Instance.PCsAreReady)
            {      
                int left = GameLogicManager.Instance.getNumOfUnitLeft2BPlaced();
                bool setActiveCondition = (GameLogicManager.Instance.inEndlessMode || left == 0);
                if (PhotonNetwork.IsConnected)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        startBtn.gameObject.SetActive(setActiveCondition && clientReady);
                    }
                    else
                    {
                        if (setActiveCondition) 
                        {
                            GameLogicManager.Instance.photonView.RPC("ClientReadyStatus", RpcTarget.Others, true);
                        }
                    }
                }
                else
                {
                    startBtn.gameObject.SetActive(setActiveCondition);
                } 
             
                undoBtn.gameObject.SetActive(GameLogicManager.Instance.getNumOfUnitPlaced() > 0);
            }
        }
        //set health bar initial value and profile image
        public UnitHealthBarController onRequestHealthBarController(Transform trans, int unitIdx, int initialHealth)
        {
            GameObject panel = GameObject.Instantiate(unitProfilePanel, trans);
            UnitHealthBarController toRet = panel.GetComponent<UnitHealthBarController>();
            toRet.SetMaxHealth(initialHealth);
            toRet.SetCurrentHealth(initialHealth);
            toRet.SetProfileImg(unitIdx);
            return panel.GetComponent<UnitHealthBarController>();
        }

        void OnDisable()
        {
            if (panelCoroutine != null)
                StopCoroutine(panelCoroutine);
            panelCoroutine = null;
        }

        IEnumerator DisplayUnitStatusPanel()
        {
            while (true)
            {
                GameObject selectedObj = TileManager.FindSpecificChildByTag(TileManager.currentlySelectedTile, "Unit");
                if (selectedObj != null)
                {
                    int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
                    PlayerController pc = GameLogicManager.Instance.controllers[id] as PlayerController;
                    selectedUnitStatusPanel.SetActive(true);
                    structurePanel.SetActive(false);
                    selectedUnitController = selectedObj.GetComponent<UnitController>();
                    selectedUnitController.SetHealthBarActive(true);
                    selectedUnitController.UpdataStatus();
                    selectedUnitStatusPanel.GetComponent<UnitStatusPanel>().SetPanel(selectedUnitController.statusData);
                    actionBar.SetActive(!pc.movePhase);
                    if (actionBar.activeSelf && selectedUnitController.remainingSpecialAbilityCooldown == 0)
                    {
                        actionButtons[0].interactable = true;
                    }
                    else
                    {
                        actionButtons[0].interactable = false;
                    }
                }
                else
                {
                    
                    selectedUnitStatusPanel.SetActive(false);
                    actionBar.SetActive(false);
                    //ConfirmedActionUI.texSelector(3);
                }
                GameObject[] activeReticle = GameObject.FindGameObjectsWithTag("Reticle");
                if (activeReticle.Length > 0)
                {
                    GameObject activedObj = TileManager.FindSpecificChildByTag(activeReticle[activeReticle.Length-1].transform.parent.gameObject, "Unit");
                    if (activedObj != null)
                    {
                        previewUnitStatusPanel.SetActive(true);
                        UnitController activedUnitController = activedObj.GetComponent<UnitController>();
                        activedUnitController.SetHealthBarActive(true);
                        structurePanel.SetActive(false);
                        activedUnitController.UpdataStatus();
                        previewUnitStatusPanel.GetComponent<UnitStatusPanel>().SetPanel(activedUnitController.statusData);
                    }
                    else
                    {
                        GameObject newObj = TileManager.FindSpecificChildByTag(activeReticle[activeReticle.Length - 1].transform.parent.gameObject, "Structure");
                        if(newObj != null)
                        {
                            structurePanel.SetActive(true);
                            //structurePanel.GetComponentInChildren<Text>().text = "This is a " + newObj.name;
                            if (newObj.name.Equals("Healing Structure"))
                            {
                                structurePanel.GetComponentInChildren<Text>().text = healingStructInfo;
                            }
                            else if(newObj.name.Equals("Attack Range Structure"))
                            {
                                structurePanel.GetComponentInChildren<Text>().text = attackRangeStructInfo;
                            }
                            else if (newObj.name.Equals("Movement Range Structure"))
                            {
                                structurePanel.GetComponentInChildren<Text>().text = movementRangeStructInfo;
                            }
                            else if (newObj.name.Equals("Obstacle"))
                            {
                                structurePanel.GetComponentInChildren<Text>().text = obstacleInfo;
                            }
                        }
                        else
                        {
                            structurePanel.SetActive(false);
                        }
                        previewUnitStatusPanel.SetActive(false);
                    }
                }
                else
                {
                    previewUnitStatusPanel.SetActive(false);
                    structurePanel.SetActive(false);
                }
                yield return null;
            }
        }

        IEnumerator RemainingUnitsInspector()
        {
            while (true)
            {
                foreach (var unitContainer in Containers)
                {
                    foreach (var pair in placeableUnitDict)
                    {
                        if (unitContainer.name.Contains(pair.Key))
                        {
                            unitContainer.SetActive(pair.Value > 0);
                        }
                    }
                }    
                yield return null;
            }
        }

        IEnumerator EndlessModeUnitPlacement()
        {
            while (true) 
            {
                foreach (var unitContainer in Containers)
                {
                    int cost = Global.LookUpPopulationCost(unitContainer.name.Substring(0, unitContainer.name.Length - 9));
                    unitContainer.SetActive(GameLogicManager.Instance.endlessPopulationPoints >= cost);
                }
                yield return true;
            }
        }

        public void SwitchUIDisplay(bool showCombatUI, bool showPopuUI)
        {
            combatUICanvas.gameObject.SetActive(showCombatUI);
            popuUICanvas.gameObject.SetActive(showPopuUI);
            if (showCombatUI)
            {
                panelCoroutine = StartCoroutine("DisplayUnitStatusPanel");
                StopCoroutine("RemainingUnitsInspector");
                // if (GameLogicManager.Instance.inEndlessMode) StopCoroutine(endlessModePlaceUnitsCoroutine); // tmp fix for executable version
                checkPlaceableUnitsCorutine = null;         
            }
            if (showPopuUI && GameLogicManager.Instance.inEndlessMode && GameLogicManager.Instance.endlessWaveClearCount > 0)
            {
                populationText.gameObject.SetActive(true);
                populationText.text = "Available Points: " + GameLogicManager.Instance.endlessPopulationPoints.ToString();
                endlessModePlaceUnitsCoroutine = StartCoroutine("EndlessModeUnitPlacement");
                StopCoroutine(panelCoroutine);
            }
        }

        public void ManagePromotion(int x)
        {
            bool? b = selectedUnitController?.PromotionAvailable();
            if (b != null && b == true && selectedUnitController.allegiance == GameLogicManager.Instance.myID) 
                if (PhotonNetwork.IsConnected) photonView.RPC("IssuePromotion", RpcTarget.All, selectedUnitController.gameObject.transform.parent.name, x);
                else IssuePromotion(selectedUnitController.gameObject.transform.parent.name, x);
            else return;
        }

        /// <summary>Communicate promotions across the network.</summary>
        /// <param name="unitParent">The tile to which the unit is bound.</param>
        [PunRPC]
        public void IssuePromotion(string unitParent, int x)
        {
            UnitController uc = TileManager.FindSpecificChildByTag(GameObject.Find(unitParent), "Unit").GetComponent<UnitController>();  
            if (x % 2 == 0)
            {
                uc.PromoteStatsOption();
            }
            else
            {
                uc.PromoteSingleHealOption();
            }
        }

        public void SpecialAction(int idx)
        {
            selectedAction = idx;
            ConfirmedActionUI.texSelector(idx);
        }

        public void ConfirmAction()
        {
            if (selectedAction == 0 && selectedUnitController != null)
            {
                // lots of room for optimization here, just hacking this in for now. revisit if performance sucks.
                PlayerController pc = GameLogicManager.Instance.controllers[GameLogicManager.Instance.myID] as PlayerController;
                if (PhotonNetwork.IsConnected) pc.PV.RPC("Special", RpcTarget.All, TileManager.currentlySelectedTile.name);
                else pc.Special(TileManager.currentlySelectedTile.name);    
            }
            actionButtons[selectedAction].interactable = false;
        }

        public void GetUnitDict(ref Dictionary<string, int> ud)
        {
            placeableUnitDict = ud;
            checkPlaceableUnitsCorutine = StartCoroutine("RemainingUnitsInspector");
        }

        private void setUpTexts()
        {
            healingStructInfo = "Healing Well\n\nProvides +15 health to two random units in the player's army who are adjacent to the well at the conclusion of their turn.";
            attackRangeStructInfo = "Tower\n\nProvides +1 attack range for each ranged unit in the occupying army when controlled.\nControl is gained if, at the start of the player's turn, the player has more units adjacent to the tower than the opponent.";
            movementRangeStructInfo = "Castle\n\nProvides +1 movement range for each melee unit in the occupying army when controlled.\nControl is gained if, at the start of the player's turn, the player has more units adjacent to the castle than the opponent.";
            obstacleInfo = "Obstacle\n\nInhibits unit movement.  Ranged attacks may pass over obstacles.";
        }

        public bool IsClickOnUI()
        {
            if (EventSystem.current != null)
            {
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                eventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);
                return results.Count > 0;
            }
            return false;
        }
    }
}