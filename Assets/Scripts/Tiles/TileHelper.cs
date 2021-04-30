/* 
 * Author:  Eric Henderson
 * Last Edited By:  Eric Henderson
 * Date Created:  1-20-2021
 * Description:  Attached to each tile, this script manages tile specific inputs and interactions.
 * Filename:  TileHelper.cs
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

namespace Capstone
{
    public class TileHelper : MonoBehaviourPunCallbacks
    {
        public UnityEvent<string, string, int> placeUnitEvent;
        public int pendingRockBlastCount;
        public List<(GameObject, int, int)> activeShotSiegeList;  // a tile may have multiple siege units queue up an attack on it, the triple tracks the instance of the siege unit, its attack damage, and its allegiance

        // Initialize relevant variables.
        void Start()
        {
            LevelGenerator.SetUpTile(gameObject);
            if (PhotonNetwork.IsConnected) gameObject.name = (string) photonView.InstantiationData[0]; // this makes more sense in GenerateTiles()
            placeUnitEvent = new UnityEvent<string, string, int>();
            placeUnitEvent.AddListener(GameLogicManager.Instance.placeUnitAction);
            pendingRockBlastCount = 0;
            activeShotSiegeList = new List<(GameObject, int, int)>();
        }

        // Perform background tasks.
        void Update()
        {
            if (!PauseMenu.GameIsPaused)
            {
                RotateTargetReticles();
            }
        }

        // Ensure that a tile becomes selected once the player clicks on it.
        private void OnMouseDown()
        {
            if (CombatUIController.instance.IsClickOnUI()) return;
            if (!PauseMenu.GameIsPaused)
            {
                // Deselect the previously selected tile if one existed.
                if (!gameObject.CompareTag(Global.Tags.selected) && TileManager.currentlySelectedTile != null)
                {
                    TileManager.DeselectCurrentSelection(true);
                    RemovePathIndicators();
                }

                int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
                PlayerController pc = GameLogicManager.Instance.controllers[id] as PlayerController;
                if (GameLogicManager.Instance.isReadyToPlay && !GameLogicManager.animationLock && GameLogicManager.IsMyTurn(pc.allegiance))
                {
                    // Activate tile selection indicators and reticle.
                    TileManager.ActivateProperSelectIndicators(gameObject);
                    TileManager.FindSpecificChildByTag(gameObject, "Reticle").SetActive(true);
                    TileManager.FindSpecificChildByTag(gameObject, "Select Indicator").SetActive(true);

                    // Tag the tile as selected and inform the camera.
                    gameObject.tag = Global.Tags.selected;
                    TileManager.currentlySelectedTile = gameObject;
                }
            }
        }

        [PunRPC]
        private void InvokePlacement(string unit, string tile, int controllerID)
        {
            placeUnitEvent.Invoke(unit, tile, controllerID);
        }

        // Ensure that when the user is hovering over an inactive grid tile it activates and behaves properly.
        private void OnMouseOver()
        {
            int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
            PlayerController pc = GameLogicManager.Instance.controllers[id] as PlayerController;

            // If the game has not been started yet and we are dragging a unit, place it if the location is valid.
            if (!GameLogicManager.Instance.isReadyToPlay)
            {
                if (Input.GetMouseButtonUp(0) && !PopulationUI.instance.Stop && !gameObject.CompareTag(Global.Tags.occupied) && ValidPlacementTile(gameObject))
                {
                    if (PhotonNetwork.IsConnected) photonView.RPC("InvokePlacement", RpcTarget.All, PopulationUI.instance.CurrentUnitType, gameObject.name, pc.allegiance);
                    else placeUnitEvent.Invoke(PopulationUI.instance.CurrentUnitType, gameObject.name, pc.allegiance);
                    GameObject.Find("Population Select Manager").GetComponent<PopulationSelectLogicManager>().OnRemove(PopulationUI.instance.CurrentUnitType);
                }

                // Activate targeting reticle.
                if (!gameObject.CompareTag(Global.Tags.occupied) && ValidPlacementTile(gameObject))
                {
                    TileManager.FindSpecificChildByTag(gameObject, "Reticle").SetActive(true);
                }
            }

            if (!PauseMenu.GameIsPaused)
            {
                if (!gameObject.CompareTag(Global.Tags.selected) && !GameLogicManager.animationLock)
                {
                    // Activate targeting reticle.
                    if (GameLogicManager.Instance.isReadyToPlay || (!gameObject.CompareTag(Global.Tags.occupied) && ValidPlacementTile(gameObject)))
                    {
                        TileManager.FindSpecificChildByTag(gameObject, "Reticle").SetActive(true);

                        // Activate targeting reticle and tag the tile as active.
                        if (!gameObject.CompareTag(Global.Tags.occupied))
                        {
                            gameObject.tag = Global.Tags.active;
                        }
                    }
                }

                // Handle interactions that occur if it is the player's turn, a unit is selected, and the active tile is an acceptable target.
                if (GameLogicManager.IsMyTurn(pc.allegiance) && TileManager.FindSpecificChildByTag(TileManager.currentlySelectedTile, "Unit") != null && !gameObject.CompareTag(Global.Tags.selected) && TileManager.FindSpecificChildByTag(gameObject, "Select Indicator").activeSelf)
                {
                    // If in the movement phase, highlight the path the unit would traverse to reach the active tile.
                    if (pc.movePhase)
                    {
                        List<(GameObject, GameObject)> potentialPathList = TileManager.FindPath(LevelGenerator.map, TileManager.currentlySelectedTile, gameObject);
                        if (potentialPathList != null)
                        {
                            for (int tileNumber = 0; tileNumber < potentialPathList.Count; tileNumber++)
                            {
                                (_, GameObject nextTile) = potentialPathList[tileNumber];
                                TileManager.FindSpecificChildByTag(nextTile, "Path Indicator").SetActive(true);
                            }
                        }
                    }

                    // Set the current target assuming an appropriate tile is right-clicked.
                    if (Input.GetMouseButtonDown(1))
                    {
                        TileManager.currentlyTargetedTile = gameObject;
                        RemovePathIndicators();
                    }
                }
            }
        
        }

        // Ensure that once the player is no longer hovering over an unselected tile the game status updates and its reticle visual deactivates.
        private void OnMouseExit()
        {
            if (!gameObject.CompareTag(Global.Tags.selected))
            {
                // Deactivate tile reticle.
                TileManager.FindSpecificChildByTag(gameObject, "Reticle").SetActive(false);

                // Tag the tile as inactive.
                if (gameObject.CompareTag(Global.Tags.active))
                {
                    gameObject.tag = Global.Tags.inactive;
                }

                // Set the path indicators to inactive.
                RemovePathIndicators();
            }
        }

        // Once a move has been issued, remove the path indicators.
        private void RemovePathIndicators()
        {
            int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
            PlayerController pc = GameLogicManager.Instance.controllers[id] as PlayerController;
            if (pc.movePhase)
            {
                GameObject tilePathIndicator = TileManager.FindSpecificChildByTag(gameObject, "Path Indicator");

                if (tilePathIndicator.activeSelf)
                {
                    for (int tileNumber = 0; tileNumber < LevelGenerator.map.transform.childCount; tileNumber++)
                    {
                        TileManager.FindSpecificChildByTag(LevelGenerator.map.transform.GetChild(tileNumber).gameObject, "Path Indicator").SetActive(false);
                    }
                }
            }
        }

        // Ensure tiles' target reticles rotate.
        private void RotateTargetReticles()
        {
            for (int tileNumber = 0; tileNumber < LevelGenerator.map.transform.childCount; tileNumber++)
            {
                GameObject reticle = TileManager.FindSpecificChildByTag(LevelGenerator.map.transform.GetChild(tileNumber).gameObject, "Reticle");

                if (reticle.activeSelf)
                {
                    reticle.transform.Rotate(Vector3.forward * Time.deltaTime);
                }
            }
        }

        // Depending on the game mode, point in the game, and the tile location, determine whether or not the tile is an acceptable unit placement location.
        private bool ValidPlacementTile(GameObject tile)
        {
            // If playing endless mode and the player is attempting to place units midgame, valid locations are tiles adjacent to existing player units.
            if (Global.activeGameMode.Equals(Global.ActiveGameMode.endlessMode) && ((AIController)GameLogicManager.Instance.controllers[1]).stagingArmy.Count > 0)
            {
                for (int unit = 0; unit < GameLogicManager.Instance.controllers[0].gameArmy.Count; unit++)
                {
                    if (TileManager.CombatDistanceBetweenTiles(GameLogicManager.Instance.controllers[0].gameArmy[unit].transform.parent.gameObject, tile) == 1)
                    {
                        return true;
                    }
                }

                return false;
            }
            // If not playing endless mode or the player has just begun, restrict valid locations to the back two rows, depending on the controller ID.
            else
            {
                if (GameLogicManager.Instance.myID == 0)
                {
                    return tile.transform.GetSiblingIndex() < (LevelGenerator.gridLength * 2) ? true : false;
                }
                else
                {
                    return LevelGenerator.map.transform.childCount - tile.transform.GetSiblingIndex() <= (LevelGenerator.gridLength * 2) ? true : false;
                }
            }
        }

        /// <summary>
        /// Trigger rock fall events at the start of your next turn.
        /// </summary>
        public void HandleRockFallEvents()
        {
            if (activeShotSiegeList.Count > 0 && !GameLogicManager.IsMyTurn(activeShotSiegeList[0].Item3))
            {
                for (int siegeIndex = 0; siegeIndex < activeShotSiegeList.Count; siegeIndex++)
                {
                    StartCoroutine(RockFall(siegeIndex));
                }
            }
        }

        /// <summary>
        /// This coroutine handles a siege's rock falling on a tile.
        /// </summary>
        /// <param name="listIndex">The index in the active shot siege list, representing a triple containing the specific siege unit, its attack damage, and allegiance.</param>
        public IEnumerator RockFall(int listIndex)
        {
            bool actuallyHit = false;

            // Set animation lock and instantiate rock.
            GameLogicManager.animationLock = true;
            yield return new WaitForSeconds(.5f);
            ParticleSystem uPS = transform.Find("Explosion_Effect").GetComponent<ParticleSystem>();
            GameObject rock = (GameObject)Instantiate(Resources.Load("Rock"), new Vector3(transform.position.x, 0, transform.position.z), Quaternion.identity);
            rock.transform.position = new Vector3(transform.position.x, 25f, transform.position.z);
            rock.GetComponent<Rigidbody>().velocity = new Vector3(0, -25, 0);

            // For every tile in the blast radius, deactivate the incoming effect if no additional rockfalls are present on the tile and damage each unit accordingly.
            for (int candidateTileIndex = 0; candidateTileIndex < LevelGenerator.map.transform.childCount; candidateTileIndex++)
            {
                GameObject candidateTile = LevelGenerator.map.transform.GetChild(candidateTileIndex).gameObject;
                int distance = TileManager.CombatDistanceBetweenTiles(gameObject, candidateTile);

                if (distance <= 1)
                {
                    UnitController attackedUnitController = TileManager.FindSpecificChildByTag(candidateTile, "Unit") != null ? TileManager.FindSpecificChildByTag(candidateTile, "Unit").GetComponent<UnitController>() : null;
                    candidateTile.GetComponent<TileHelper>().pendingRockBlastCount--;
                    if (candidateTile.GetComponent<TileHelper>().pendingRockBlastCount <= 0)
                    {
                        candidateTile.transform.Find("Incoming_Effect").GetComponent<ParticleSystem>().Stop();
                    }

                    if (distance == 0 && attackedUnitController != null)
                    {
                        actuallyHit = true;
                        attackedUnitController.Attacked(attackedUnitController.CalculateDamage(activeShotSiegeList[listIndex].Item2));
                    }
                    else if (distance == 1 && attackedUnitController != null)
                    {
                        actuallyHit = true;
                        attackedUnitController.Attacked(attackedUnitController.CalculateDamage(activeShotSiegeList[listIndex].Item2 / 4));
                    }
                }
            }

            // Handle promotions.
            if (activeShotSiegeList[listIndex].Item1 != null && actuallyHit)
            {
                UnitController siegeUC = activeShotSiegeList[listIndex].Item1.GetComponent<UnitController>();

                siegeUC.currentPromotionPoints += 2;
                
                // If unit has enough points to promote, and unit is not at max promotions, then display promotion particle effect.
                if (siegeUC.currentPromotionPoints >= siegeUC.requiredPromotionPoints && siegeUC.totalPromotions <= siegeUC.maxPromotions)
                {
                    ParticleSystem pPS = activeShotSiegeList[listIndex].Item1.transform.Find("Promotion_Effect").GetComponent<ParticleSystem>();
                    pPS.Play();
                }
            }

            yield return new WaitForSeconds(.9f);
            FindObjectOfType<AudioManager>()?.Play("RockFallNoise");
            uPS.Play();

            // Destroy rock instance, manage the list state, and disable the animation lock.
            Destroy(rock);
            if (listIndex == activeShotSiegeList.Count - 1) activeShotSiegeList.Clear();
            GameLogicManager.animationLock = false;
        }
    }
}
