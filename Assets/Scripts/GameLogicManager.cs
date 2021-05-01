/* 
 * Author:  Dylan Klingensmith
 * Last Edited By:  Eric Henderson
 * Date Created:  1-22-2021
 * Description:  This script manages the turn-based gameplay logic.
 * Filename:  GameLogicManager.cs
 */

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using System.Collections;

namespace Capstone
{
    public class GameLogicManager : MonoBehaviourPunCallbacks
    {
        public static GameLogicManager Instance;
        public UnityAction<string, string, int> placeUnitAction;
        [Header("State")]
        public static bool animationLock;
        public int myID;
        public int otherID;
        public static int turnCounter;
        public bool isReadyToPlay;
        public bool inEndlessMode;
        [Header("Players")]
        public const string humanPrefab = "_PlayerController";
        public const string aiPrefab = "_AIController";
        public GameController[] controllers;
        public int controllerIDWithControl;
        public int endlessWaveClearCount;
        public int endlessPopulationPoints;
        
        public bool isPlayerArmyEmpty;
        public float fogClear = 0.00001f;
        public bool PCsAreReady = false;    //play controller(s) fully initialized flag

        /// <summary>
        /// Results UI script.  
        /// </summary>
        public ResultsPanelScript resultsScript;

        public GameObject ResultsUIPanel;

        [SerializeField] public Transform playerUnitSpawnSpot;

        // Initialize game state variables.
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            isReadyToPlay = false;
            inEndlessMode = false;
            animationLock = false;
            turnCounter = 1;
            endlessWaveClearCount = 0;
            endlessPopulationPoints = 0;
            placeUnitAction = new UnityAction<string, string, int>(PlaceUnit);
            controllers = new GameController[2]; // always 2 controllers
            ResultsUIPanel = GameObject.Find("EndlessResultsPanel");
        }


        // Instantiate all units in the unit dictionary.
        private void Start()
        {
            controllerIDWithControl = 0;
            if (PhotonNetwork.IsConnected)
            {
                SpawnNetworkedControllers();
                if (PhotonNetwork.IsMasterClient)
                {
                    Global.rngSeed = (int)(UnityEngine.Random.value * 10000);
                    photonView.RPC("ReceiveSeed", RpcTarget.Others, Global.rngSeed);
                    UnityEngine.Random.InitState(Global.rngSeed);
                }
            }
            else { SpawnPlayerController(); SpawnAI(); }
        }

        /// <summary>Given some allegiance, check if it's that player's turn.</summary>
        public static bool IsMyTurn(int allegiance)
        {
            // if turn is odd, 0's turn
            // if turn is even, 1's turn
            // if TC % 2 is 1, 0's turn
            // if TC % 2 is 0, 1's turn
            return (turnCounter % 2) != allegiance;
        }

        // Ensure that a unit is placed correctly on the gameboard.
        public void PlaceUnit(string name, string tile, int controllerID = 0)
        {
            Debug.LogFormat("Placing {0} By {1}", name, controllerID.ToString());
            PlayerController pc = controllers[controllerID] as PlayerController;
            GameObject unit = pc.popArmy.First(x => x.name.Contains(name));
            GameObject g_tile = GameObject.Find(tile);

            unit.transform.SetParent(g_tile.transform);
            unit.transform.localScale = LevelGenerator.unitScale;
            unit.transform.localPosition = LevelGenerator.defaultPosition;
            unit.transform.eulerAngles = pc.allegiance == 0 ? Vector3.zero : new Vector3(0, 180, 0);

            //initially obfuscate opponent's units during placement phase in multiplayer
            if (PhotonNetwork.IsConnected && controllerID == otherID)
            {
                unit.SetActive(false);
            }
            else
            {
                unit.SetActive(true);

                //Set allegiance UI
                if (pc.allegiance == 0)
                {
                    unit.transform.Find("blue_team_aura").GetComponent<ParticleSystem>().Play();
                }
                else
                {
                    unit.transform.Find("red_team_aura").GetComponent<ParticleSystem>().Play();
                }
            }
            pc.placedUnits.Push(unit);
            pc.popArmy.Remove(unit);

            if (inEndlessMode) endlessPopulationPoints = endlessPopulationPoints > 0 ? endlessPopulationPoints - Global.LookUpPopulationCost(unit.name) : 0;

            g_tile.tag = Global.Tags.occupied;
        }

        // get turnCounter
        public int GetTurnCounter()
        {
            return turnCounter;
        }

        // set turnCounter
        [PunRPC]
        public void NextTurn()
        {
            int controllerIDWithoutControl = 1;

            if (controllerIDWithControl == 1)
            {
                controllerIDWithoutControl = 0;
            }

            // Handle unit events that occur on turn changes.
            for (int unit = 0; unit < controllers[controllerIDWithoutControl].gameArmy.Count; unit++)
            {
                UnitController unitController = controllers[controllerIDWithoutControl].gameArmy[unit].GetComponent<UnitController>();
                unitController.TurnEvents();
            }

            // Handle tile events that occur on turn changes.
            for (int tileNumber = 0; tileNumber < LevelGenerator.map.transform.childCount; tileNumber++)
            {
                TileHelper tileHelper = LevelGenerator.map.transform.GetChild(tileNumber).gameObject.GetComponent<TileHelper>();
                tileHelper.HandleRockFallEvents();
            }
        
            turnCounter++;
            controllerIDWithControl = turnCounter % 2 == 0 ? 1 : 0;

            // Reset all player units' current status variables.
            bool buffedMovementRange = DetermineStructureBuff("Movement Range");
            bool buffedAttackRange = DetermineStructureBuff("Attack Range");
            for (int unit = 0; unit < controllers[controllerIDWithControl].gameArmy.Count; unit++)
            {
                UnitController unitController = controllers[controllerIDWithControl].gameArmy[unit].GetComponent<UnitController>();
                unitController.hasActed = false;
                unitController.currentMovementRange = (buffedMovementRange && unitController.defaultAttackRange == 1) ? unitController.defaultMovementRange + 1 : unitController.defaultMovementRange;
                unitController.attackRange = (buffedAttackRange && unitController.defaultAttackRange > 1) ? unitController.defaultAttackRange + 1 : unitController.defaultAttackRange;
                unitController.attackDamage = unitController.defaultAttackDamage;
                unitController.remainingSpecialAbilityCooldown = unitController.remainingSpecialAbilityCooldown > 0 ? unitController.remainingSpecialAbilityCooldown - 1 : 0;
            }

            UIEffectController.instance.ShowTurnBeginEffect(myID == controllerIDWithControl);

            return;
        }

        [PunRPC]
        public void ReceiveSeed(int masterSeed)
        {
            Global.rngSeed = masterSeed;
            UnityEngine.Random.InitState(Global.rngSeed);
        }

        // Finish the current turn.
        public void EndTurn()
        {
            // Clear selections and formally end the turn.
            TileManager.DeselectCurrentSelection();
            (controllers[controllerIDWithControl] as PlayerController).TogglePhase(); // set back to move phase

            // Issue healing structure healing if applicable.
            if (PhotonNetwork.IsConnected) photonView.RPC("HealingStructureHeal", RpcTarget.All);
            else HealingStructureHeal();

            if (inEndlessMode && (controllers[1] as AIController).gameArmy.Count == 0)
            {
                endlessWaveClearCount++;
                endlessPopulationPoints += 1;

                if (endlessPopulationPoints > 1)
                {
                    PlayerController pc = controllers[0] as PlayerController;
                    isReadyToPlay = false;
                    CombatUIController.instance.SwitchUIDisplay(false, true);
                    SpawnEndlessModeUnits(pc);
                }
                else
                {
                    NextTurn();
                }
            }
            else
            {
                if (PhotonNetwork.IsConnected)
                    photonView.RPC("NextTurn", RpcTarget.All);
                else
                    NextTurn();
            }
        }

        public virtual IEnumerator StructureHealing()
        {
            for (int unit = 0; unit < controllers[controllerIDWithControl].gameArmy.Count; unit++)
            {
                UnitController unitController = controllers[controllerIDWithControl].gameArmy[unit].GetComponent<UnitController>();
                //stop the healing particle system on the target
                ParticleSystem tPS = unitController.transform.Find("Healing_Particles").GetComponent<ParticleSystem>();
                tPS.Stop();
            }
            yield return null;
        }

        /// <summary>
        /// At the conclusion of the player's turn, two of their units adjacent to a healing structure receive healing (chosen randomly).
        /// </summary>
        [PunRPC]
        public void HealingStructureHeal()
        {
            List<UnitController> healableUnits = new List<UnitController>();
            int previouslyChosenIndex = -1;

            // Find and store all eligible units for receiving a heal from the well.
            for (int unit = 0; unit < controllers[controllerIDWithControl].gameArmy.Count; unit++)
            {
                UnitController unitController = controllers[controllerIDWithControl].gameArmy[unit].GetComponent<UnitController>();

                int healingStructureLocation = Global.LookUpActiveMapHealingStructureLocation();
                if (healingStructureLocation != -1 && TileManager.CombatDistanceBetweenTiles(unitController.transform.parent.gameObject, LevelGenerator.map.transform.GetChild(healingStructureLocation).gameObject) == 1)
                {
                    healableUnits.Add(unitController);
                }
            }

            // Randomly select two of the eligible units to heal.
            for (int healNumber = 0; healNumber < Mathf.Min(2, healableUnits.Count); healNumber++)
            {
                int unitIndex = Random.Range(0, healableUnits.Count);
                while (unitIndex == previouslyChosenIndex)
                {
                    unitIndex = Random.Range(0, healableUnits.Count);
                }
                previouslyChosenIndex = unitIndex;

                ParticleSystem tPS = healableUnits[unitIndex].transform.Find("Healing_Particles").GetComponent<ParticleSystem>();
                tPS.Play();
                if (healableUnits[unitIndex].currentHealth > 0)
                {
                    healableUnits[unitIndex].currentHealth += 15;

                    if (healableUnits[unitIndex].currentHealth > healableUnits[unitIndex].totalHealth)
                    {
                        healableUnits[unitIndex].currentHealth = healableUnits[unitIndex].totalHealth;
                    }

                    healableUnits[unitIndex].healthBarController.SetCurrentHealth(healableUnits[unitIndex].currentHealth);
                }
            }

            // Stop active healing particle effects.
            StartCoroutine("StructureHealing");
        }

        // used in CombatUIController to indicate that the client's army is finished placing, so the host may click start battle
        [PunRPC]
        public void ClientReadyStatus(bool _clientReady)
        {
            CombatUIController.instance.clientReady = _clientReady;
        }

        [PunRPC]
        public void HandleNetworkedUnits(string[] names, int _controllerID)
        {
            PlayerController pc = controllers[_controllerID] as PlayerController;
            Debug.Log("Units received: " + string.Join(", ", names));
            Debug.LogFormat("Received from {0}", _controllerID);
            foreach (string _name in names)
            {
                var temp = _name.Remove(_name.Length - 1); // gives us name
                GameObject unit = Instantiate(Resources.Load<GameObject>(temp));
                unit.name = temp + _controllerID;
                pc.popArmy.Add(unit);
                unit.SetActive(false);
            }
        }

        public void HandleTurnLogic()
        {
            int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
            PlayerController pc = controllers[id] as PlayerController;
            Debug.LogFormat("Player {0} Clicked Turn Btn. Player {1} in control.", id, controllerIDWithControl);
            if (!IsMyTurn(pc.allegiance)) return;
            if (animationLock) return;
            if (pc.movePhase) { pc.TogglePhase(); return; }
            else { EndTurn(); }
        }

        public void OnStartBattleClick()
        {
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("StartBattle", RpcTarget.All);
            }
            // If a staging army exists, that must mean that we are in the middle of an endless mode game.
            else if ((controllers[1] as AIController).stagingArmy.Count > 0)
            {
                CombatUIController.instance.SwitchUIDisplay(true, false);
                isReadyToPlay = true;
                NextTurn();
                //not sure if it's safe but requires it for now
                PlayerController pc = controllers[0] as PlayerController;
                pc.SetPlayerUnitList();
                pc.placedUnits.Clear();
                ResetPopArmy(pc);
            }
            else
            {
                StartBattle();
            }
        }

        [PunRPC]
        private void StartBattle()
        {
            //RenderSettings.fog = false;
            CombatUIController.instance.SwitchUIDisplay(true, false);
            isReadyToPlay = true; // temp..

            int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
            (controllers[id] as PlayerController).SetPlayerUnitList();

            if (PhotonNetwork.IsConnected)
            {
                (controllers[otherID] as PlayerController).SetPlayerUnitList();
            }
            else
            {
                // Handle single player responsibilities when applicable.
                AIController aiController = controllers[1] as AIController;
                aiController.Spawn();

                if (inEndlessMode)
                {
                    aiController.Spawn(true);
                }
            }
            UIEffectController.instance.ShowTurnBeginEffect(myID == controllerIDWithControl);
        }

        void SpawnAI()
        {
            GameObject ai = (GameObject)Instantiate(Resources.Load(aiPrefab), Vector3.zero, Quaternion.identity);
            AIController aiScript = ai.GetComponent<AIController>();
            aiScript.Initialize();
        }

        // spawns networked player prefabs
        void SpawnNetworkedControllers()
        {
            GameObject player = PhotonNetwork.Instantiate(humanPrefab, Vector3.zero, Quaternion.identity);
        }

        void SpawnPlayerController()
        {
            GameObject player = (GameObject)Instantiate(Resources.Load(humanPrefab), Vector3.zero, Quaternion.identity);
            PlayerController playerScript = player.GetComponent<PlayerController>();
            playerScript.Initialize();
        }

        [PunRPC]
        public void RevisePlacement(int _controllerID = 0)
        {
            // if we're the host and this func is called by client, then client is no longer ready
            if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient && myID != _controllerID) ClientReadyStatus(false);
            PlayerController pc = controllers[_controllerID] as PlayerController;
            GameObject obj = pc.placedUnits.Pop();
            UnitController uc = obj.GetComponent<UnitController>();
            if (!PhotonNetwork.IsConnected || pc.PV.IsMine)
            {
                pc.populationLogicScript.unitDict[uc.unitType] += 1;
                Debug.LogFormat("Player {0} Undoing {1}", _controllerID, uc.unitType);
            }

            isReadyToPlay = false;
            pc.popArmy.Add(obj);
            obj.SetActive(false);
            obj.transform.parent.tag = Global.Tags.inactive;
            obj.transform.parent = playerUnitSpawnSpot;

            if (inEndlessMode && endlessWaveClearCount > 0) endlessPopulationPoints += Global.LookUpPopulationCost(obj.name);
        }

        public void OnUndoClick()
        {
            if (PhotonNetwork.IsConnected) photonView.RPC("RevisePlacement", RpcTarget.All, myID);
            else RevisePlacement(myID);
        }

        public int getNumOfUnitLeft2BPlaced()
        {
            return PhotonNetwork.IsConnected ? (controllers[0] as PlayerController).popArmy.Count() + (controllers[1] as PlayerController).popArmy.Count() : (controllers[0] as PlayerController).popArmy.Count();
        }

        public int getNumOfUnitPlaced()
        {
            int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
            return (controllers[id] as PlayerController).placedUnits.Count();
        }

        [PunRPC]
        public void Restart()
        {
            int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
            PlayerController pc = controllers[id] as PlayerController;
            while (pc.gameArmy.Count > 0)
            {
                GameObject obj = pc.gameArmy[0];
                pc.placedUnits.Push(obj);
                pc.gameArmy.RemoveAt(0);
            }
            while (pc.placedUnits.Count > 0)
            {
                RevisePlacement();
            }
        }

        //temporary solution for spawning units in endless mode
        public void SpawnEndlessModeUnits(PlayerController pc)
        {
            string[] units = { "Knight", "Archer", "Cleric", "Siege" };
            foreach (string unitName in units)
            {
                for (int i = 0; i < 8; ++i)
                {
                    GameObject unit = Instantiate(Resources.Load<GameObject>(unitName));
                    unit.name = unitName;
                    pc.popArmy.Add(unit);
                    unit.SetActive(false);
                }
            }
        }

        public void ResetPopArmy(PlayerController pc)
        {
            for (int i = 0; i < pc.popArmy.Count; ++i)
            {
                Debug.Log("Destroy units " + pc.popArmy[i].name);
                Destroy(pc.popArmy[i]);
            }
            pc.popArmy.Clear();
        }

        // Determine whether or not the current player should receive buffs for this turn from a structure.
        public bool DetermineStructureBuff(string buffType)
        {
            GameObject map = LevelGenerator.map;
            int adjacentPlayerUnitCount = 0;
            int adjacentOpponentUnitCount = 0;
            int structureLocation = -1;

            if (buffType.Equals("Movement Range"))
            {
                structureLocation = Global.LookUpActiveMapMovementRangeStructureLocation();
            }
            else if (buffType.Equals("Attack Range"))
            {
                structureLocation = Global.LookUpActiveMapAttackRangeStructureLocation();
            }

            if (structureLocation != -1)
            {
                // For each tile adjacent to the move structure, determine if a unit is on it and which team the unit belongs to.
                for (int adjacentTileIndex = 0; adjacentTileIndex < map.transform.childCount; adjacentTileIndex++)
                {
                    GameObject candidateTile = map.transform.GetChild(adjacentTileIndex).gameObject;

                    if (TileManager.CombatDistanceBetweenTiles(candidateTile, map.transform.GetChild(structureLocation).gameObject) == 1)
                    {
                        GameObject unit = TileManager.FindSpecificChildByTag(candidateTile, "Unit");
                        if (unit != null)
                        {
                            if (unit.GetComponent<UnitController>().allegiance == controllerIDWithControl)
                            {
                                adjacentPlayerUnitCount++;
                            }
                            else
                            {
                                adjacentOpponentUnitCount++;
                            }
                        }
                    }
                }

                // If the current player has a majority of the units adjacent to the move structure, their army receives a buff this turn.
                if (adjacentPlayerUnitCount > adjacentOpponentUnitCount)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public ResultsPanelScript getResultsPanelScript()
        {
            resultsScript = ResultsUIPanel.GetComponent<ResultsPanelScript>();
            return resultsScript;
        }
    }
}
