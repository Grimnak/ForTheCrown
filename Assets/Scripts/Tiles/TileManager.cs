/* 
 * Author:  Eric Henderson
 * Last Edited By:  Eric Henderson
 * Date Created:  1-20-2021
 * Description:  Manages tile interactions that concern the entire map, including pathing, distance b/w tiles and deselection across all tiles.
 * Filename:  TileManager.cs
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

namespace Capstone
{
    public class TileManager : MonoBehaviourPun
    {
        public static TileManager Instance;
        public UnityEvent<string, string, int> placeUnitEvent;
        public static GameObject currentlySelectedTile;
        public static GameObject currentlyTargetedTile;

        private static GameObject map;

        // Initialize relevant variables.

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
    
        private void Start()
        {
            map = LevelGenerator.map;
            placeUnitEvent = new UnityEvent<string, string, int>();
            placeUnitEvent.AddListener(GameLogicManager.Instance.placeUnitAction);
        }

        // Perform background tasks.
        private void LateUpdate()
        {
            if (!PauseMenu.GameIsPaused)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    DeselectCurrentSelection();
            }
        }

        /// <summary>
        /// Find a specific child of a tile by its tag and return null if the requested child cannot be located.
        /// </summary>
        /// <param name="tile">The tile whose children will be searched.</param>
        /// <param name="tagName">The tag of the child that we are looking for.</param>
        /// <returns>The requested child as a GameObject if found; else, return null.</returns>
        public static GameObject FindSpecificChildByTag(GameObject tile, string tagName)
        {
            if (tile != null)
            {
                for (int childNumber = 0; childNumber < tile.transform.childCount; childNumber++)
                {
                    Transform ch = tile.transform.GetChild(childNumber);
                    if (ch.CompareTag(tagName))
                    {
                        return ch.gameObject;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Calculate the distance between two tiles for the purposes of movement (no diagonal movement).
        /// </summary>
        /// <param name="tileOne">The first of two tiles that we are calculating the distance between.</param>
        /// <param name="tileTwo">The second of two tiles that we are calculating the distance between.</param>
        /// <returns>The move distance between the provided tiles.</returns>
        public static int MoveDistanceBetweenTiles(GameObject tileOne, GameObject tileTwo)
        {
            int tileOneIndex = tileOne.transform.GetSiblingIndex();
            int tileTwoIndex = tileTwo.transform.GetSiblingIndex();
            int horizontalDistance = Mathf.Abs(tileOneIndex % LevelGenerator.gridLength - tileTwoIndex % LevelGenerator.gridLength);
            int verticalDistance = Mathf.Abs(tileOneIndex / LevelGenerator.gridLength - tileTwoIndex / LevelGenerator.gridLength);

            return horizontalDistance + verticalDistance;
        }

        /// <summary>
        /// Calculate the distance between two tiles for the purposes of combat (diagonals included).
        /// </summary>
        /// <param name="tileOne">The first of two tiles that we are calculating the distance between.</param>
        /// <param name="tileTwo">The second of two tiles that we are calculating the distance between.</param>
        /// <returns>The combat distance between the provided tiles.</returns>
        public static int CombatDistanceBetweenTiles(GameObject tileOne, GameObject tileTwo)
        {
            int tileOneIndex = tileOne.transform.GetSiblingIndex();
            int tileTwoIndex = tileTwo.transform.GetSiblingIndex();
            int horizontalDistance = Mathf.Abs(tileOneIndex % LevelGenerator.gridLength - tileTwoIndex % LevelGenerator.gridLength);
            int verticalDistance = Mathf.Abs(tileOneIndex / LevelGenerator.gridLength - tileTwoIndex / LevelGenerator.gridLength);

            return Mathf.Max(horizontalDistance, verticalDistance);
        }

        /// <summary>
        /// Find the closest unoccupied tile to a provided tile in terms of transform distance.
        /// </summary>
        /// <param name="tile">The tile from which we are comparing distances.</param>
        /// <returns>The tile closest distance-wise to the provided tile.</returns>
        public static GameObject FindClosestUnoccupiedTile(GameObject tile)
        {
            GameObject closestTile = map.transform.GetChild(0).gameObject;
            float closestSquaredDistance = Mathf.Infinity;
            Vector3 currentPosition = new Vector3(tile.transform.position.x, 0, tile.transform.position.z);

            for (int childIndex = 0; childIndex < map.transform.childCount; childIndex++)
            {
                Transform childTransform = map.transform.GetChild(childIndex);
                Vector3 directionToCandidate = new Vector3(childTransform.position.x, 0, childTransform.position.z) - currentPosition;
                float squaredDistance = directionToCandidate.sqrMagnitude;
                if (squaredDistance < closestSquaredDistance && !childTransform.gameObject.CompareTag(Global.Tags.occupied))
                {
                    closestSquaredDistance = squaredDistance;
                    closestTile = map.transform.GetChild(childIndex).gameObject;
                }
            }

            return closestTile;
        }

        // Calculate the four neighbor indices of a tile for a given game map.
        public static int[] GetNeighborIndices(int idx)
        {
            int gridLength = LevelGenerator.gridLength;
            int gridWidth = LevelGenerator.gridWidth;
            int[] neighbors = new int[4];
            neighbors[0] = idx / gridLength == gridWidth - 1 ? idx : idx + gridLength; // if self not north border, neighbor is north by 1, else north is self
            neighbors[1] = idx / gridLength == 0 ? idx : idx - gridLength; // if self not south border, neighbor is south by 1, else south is self
            neighbors[2] = idx % gridLength == gridLength - 1 ? idx : idx + 1; // if self not east border, neighbor is east by 1, else east is self
            neighbors[3] = idx % gridLength == 0 ? idx : idx - 1; // if self not west border, neighbor is west by 1, else west is self
            return neighbors;
            // Assuming Z Pos is North, array order is as such: [N, S, E, W]
        }

        /// <summary>
        /// Determine the path to travel from one tile to another using BFS.
        /// </summary>
        /// <param name="gameMap">The object that holds all the individual tile GameObjects.</param>
        /// <param name="startState">The first tile in the path.</param>
        /// <param name="goalState">The final tile in the path.</param>
        /// <returns>A list of tuples indicating the final path where each tuple reveals the current tile GameObject in the path and the next tile GameObject that is along the path.</returns>
        public static List<(GameObject, GameObject)> FindPath(GameObject gameMap, GameObject startState, GameObject goalState)
        {
            HashSet<GameObject> closedSet = new HashSet<GameObject>(); // for visited tiles
            List<GameObject> openList = new List<GameObject>(); // for tiles to-be-visited
            Dictionary<GameObject, (GameObject, GameObject)[]> parents = new Dictionary<GameObject, (GameObject, GameObject)[]>();
            // parents is a next_tile -> [...(parent tile, next_tile)]
            openList.Add(startState);
            while (openList.Count > 0)
            {
                GameObject state = openList[0]; // maintaining this list like a queue. get first element
                openList.RemoveAt(0); // then pop it off
                if (closedSet.Contains(state)) // don't "re-visit" a visited tile
                    continue;

                if (state.Equals(goalState))
                { // goal found, time to walk back from goal to start.
                    List<(GameObject, GameObject)> solution = new List<(GameObject, GameObject)>();
                    (GameObject, GameObject) action;
                    while (!state.Equals(startState))
                    {
                        action = parents[state].First(); // parents tells us how we got to a state based on the previous state.
                        (state, _) = action; // we update the state variable here to walk back 1 more tile to check the condition in the while.
                        solution.Insert(0, action);
                    }
                    return solution;
                }
                else
                {
                    int[] neighborsIdxs = GetNeighborIndices(state.transform.GetSiblingIndex());
                    for (int i = 0; i < neighborsIdxs.Length; i++)
                    {
                        GameObject neighbor = gameMap.transform.GetChild(neighborsIdxs[i]).gameObject;
                        if (neighbor.CompareTag(Global.Tags.occupied) || neighbor.Equals(state))
                            continue; // don't consider a neighbor for expansion if it's occupied or the same tile as the current one 
                        
                        if (parents.ContainsKey(neighbor)) 
                        {
                            (GameObject, GameObject)[] values = new (GameObject, GameObject)[parents[neighbor].Count() + 1];
                            int j = 0;
                            foreach ((GameObject, GameObject) val in parents[neighbor])
                            { // rebuild parents mapping of GameObject -> [...(GameObject, GameObject)]
                                values[j] = val;
                                j++;
                            }
                            values[j] = (state, neighbor);
                            parents[neighbor] = values;
                        }
                        else parents[neighbor] = new[] { (state, neighbor) };
                        openList.Add(neighbor);
                    }
                    closedSet.Add(state); // done visiting this tile. add to closed set.
                }
            }

            // This code should only run if no path is found
            Debug.LogWarning("No path found. Dumping all pathing data! (not necessarily an error)");
            Debug.Log("Start State: " + startState.name);
            Debug.Log("Goal State: " + goalState.name);
            Debug.Log("Open List: ");
            foreach (GameObject el in openList)
            {
                Debug.Log(el.name);
            }
            foreach (KeyValuePair<GameObject, (GameObject, GameObject)[]> kvp in parents)
                foreach ((GameObject, GameObject) tup in kvp.Value)
                    Debug.LogFormat("Key = {0}, Value = {1}", kvp.Key.name, (tup.Item1.name, tup.Item2.name));

            return null; // if no path is found, returns null
        }

        /// Ensure that tiles update their status upon deselection.
        /// <param name="mouseDown">Only true if we're inside OnMouseDown, hence the default value.</param>
        public static void DeselectCurrentSelection(bool mouseDown = false)
        {
            if (currentlySelectedTile != null)
            {
                for (int tileNumber = 0; tileNumber < map.transform.childCount; tileNumber++)
                {
                    GameObject tile = map.transform.GetChild(tileNumber).gameObject;
                    GameObject reticleChild = FindSpecificChildByTag(tile, "Reticle");
                    GameObject selectIndicatorChild = FindSpecificChildByTag(tile, "Select Indicator");

                    if (tile.CompareTag(Global.Tags.selected))
                    {
                        reticleChild.SetActive(false);

                        if (FindSpecificChildByTag(tile, "Unit") == null && FindSpecificChildByTag(tile, "Structure") == null)
                        {
                            tile.tag = Global.Tags.inactive;
                        }
                        else
                        {
                            tile.tag = Global.Tags.occupied;
                        }
                    }

                    if (selectIndicatorChild.activeSelf)
                    {
                        selectIndicatorChild.SetActive(false);
                    }
                }

                currentlySelectedTile = null;
                currentlyTargetedTile = null;
                return;
            }
        }

        // Ensure the proper tile select indicators are activated upon making a selection.
        public static void ActivateProperSelectIndicators(GameObject tile)
        {
            GameObject unit = FindSpecificChildByTag(tile, "Unit");
            UnitController uc = unit?.GetComponent<UnitController>();
            int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
            PlayerController pc = GameLogicManager.Instance.controllers[id] as PlayerController;
            // If there is no unit present on the tile, only highlight the selected tile.
            if (unit != null && uc.allegiance == pc.allegiance)
            {
                // The current phase of the turn affects which tiles are affected.
                if (pc.movePhase)
                {
                    ShowMoveOptions(tile, uc.currentMovementRange);
                }
                else
                {
                    if (uc.hasActed)
                    {
                        FindSpecificChildByTag(tile, "Select Indicator").SetActive(true);
                    }
                    else
                    {
                        ShowCombatOptions(tile, uc.attackRange);
                    }
                }
            }
            else
            {
                FindSpecificChildByTag(tile, "Select Indicator").SetActive(true);
            }
        }

        // Determine the range of acceptable movement locations for the selected unit.
        public static void ShowMoveOptions(GameObject selectedTile, int remainingMoves)
        {
            int selectedTileIndex = selectedTile.transform.GetSiblingIndex();

            for (int candidateTileIndex = 0; candidateTileIndex < map.transform.childCount; candidateTileIndex++)
            {
                GameObject candidateTile = map.transform.GetChild(candidateTileIndex).gameObject;

                // Limit the possible range to a square box with the remaining moves as its length and width to reduce number of searches taking place.
                if (MoveDistanceBetweenTiles(selectedTile, candidateTile) <= remainingMoves)
                {
                    // A tile is an acceptable movement location if it is not occupied.
                    if (!candidateTile.CompareTag(Global.Tags.occupied))
                    {
                        List<(GameObject, GameObject)> route = FindPath(map, selectedTile, candidateTile);
                        // Further limit the possible options by accounting for obstacles noticed during pathing.
                        if (route != null && route.Count <= remainingMoves)
                        {
                            FindSpecificChildByTag(candidateTile, "Select Indicator").SetActive(true);
                        }
                    }
                }
            }
        }

        // Determine the range of acceptable action locations for the selected unit.
        public static void ShowCombatOptions(GameObject selectedTile, int attackRange)
        {
            for (int candidateTileIndex = 0; candidateTileIndex < map.transform.childCount; candidateTileIndex++)
            {
                GameObject candidateTile = map.transform.GetChild(candidateTileIndex).gameObject;

                if (CombatDistanceBetweenTiles(selectedTile, candidateTile) <= attackRange)
                {
                    FindSpecificChildByTag(candidateTile, "Select Indicator").SetActive(true);
                }
            }
        }
    }
}
