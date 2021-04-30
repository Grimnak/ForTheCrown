/* 
 * Author:  Eric Henderson
 * Last Edited By:  Eric Henderson
 * Date Created:  1-18-2021
 * Description:  This script generates the level's tile system.
 * Filename:  LevelGenerator.cs
 */

using UnityEngine;
using Photon.Pun;

namespace Capstone
{
    public class LevelGenerator : MonoBehaviourPunCallbacks
    {
        public static LevelGenerator Instance;
        public static GameObject map { get; private set; }
        public static int gridLength; // The length represents the horizontal (east-west) direction.
        public static int gridWidth; // The width represents the vertical (north-south) direction.
        public static int tileDimension;
        public static Vector3 unitScale;
        public static Vector3 healingWellScale;
        public static Vector3 towerScale;
        public static Vector3 obstacleScale;
        public static Vector3 defaultPosition;
        public static Vector3 obstaclePosition;

        [SerializeField] private GameObject playableTilePrefab;
        [SerializeField] private GameObject stagingTilePrefab;
        [SerializeField] private GameObject attackRangeStructurePrefab;
        [SerializeField] private GameObject movementRangeStructurePrefab;
        [SerializeField] private GameObject healingStructurePrefab;
        [SerializeField] private GameObject obstaclePrefab;
        private GameObject stagingAreas;
        private bool done; // are we done drawing obstacles?

        // Complete initializations before generating the world.
        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            Instance = this;
            map = new GameObject("World Map");
            map.tag = Global.Tags.map;
            gridLength = Global.LookUpActiveMapLength();
            gridWidth = Global.LookUpActiveMapWidth();
            tileDimension = Global.LookUpActiveMapTileDimension();
            unitScale = new Vector3(0.35f, tileDimension * 35f, 0.35f);
            healingWellScale = new Vector3(0.3f, tileDimension * 30f, 0.3f);
            towerScale = new Vector3(0.1f, tileDimension * 10f, 0.1f);
            obstacleScale = new Vector3(0.4f, tileDimension * 40f, 0.4f);
            defaultPosition = new Vector3(0.0f, 5.0f, 0.0f);
            obstaclePosition = new Vector3(0.25f, 5.0f, 0.0f);
            done = false;
        }

        // Create the world, which is an empty object in which all generated tiles and associated objects reside.
        private void Start()
        {
            ShowCorrectMap();
            GenerateTiles();
            if (!PhotonNetwork.IsConnected) // GenerateStrategicElements conditionally called in TileHelper OnPhotonInstantiate
                GenerateStrategicElements();
        }

        private void Update()
        {
            if (!done && PhotonNetwork.IsConnected && map.transform.childCount == gridLength * gridWidth)
            { // if the map is done drawing in multiplayer, then spawn the obstacles (once)
                GenerateStrategicElements();
                done = true;
            }
        }

        private void ShowCorrectMap()
        {
            GameObject levelOneMap = GameObject.Find("Level One");
            GameObject levelTwoMap = GameObject.Find("Level Two");
            GameObject levelThreeMap = GameObject.Find("Level Three");

            switch (Global.activeGameMap)
            {
                case Global.ActiveGameMap.map1:
                    levelTwoMap.SetActive(false);
                    levelThreeMap.SetActive(false);
                    break;
                case Global.ActiveGameMap.map2:
                    levelOneMap.SetActive(false);
                    levelThreeMap.SetActive(false);
                    break;
                case Global.ActiveGameMap.map3:
                    levelOneMap.SetActive(false);
                    levelTwoMap.SetActive(false);
                    break;
                default:
                    levelTwoMap.SetActive(false);
                    levelThreeMap.SetActive(false);
                    break;
            }
        }

        private void GenerateTiles()
        {
            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;

            GameObject tile = null;
            int playableTileCount = 1;
            int stagingTileCount = 1;

            // If in endless mode, perform additional setup.
            if (Global.activeGameMode.Equals(Global.ActiveGameMode.endlessMode))
            {
                // Generate staging areas containers.
                stagingAreas = new GameObject("Staging Areas");
                stagingAreas.tag = Global.Tags.staging;

                // Adjust provided grid dimensions to account for staging areas.
                gridLength += 2;
                gridWidth += 2;

                // Inform the game logic manager that we are playing endless mode.
                GameLogicManager.Instance.inEndlessMode = true;
            }

            // Create appropriate board setup.
            for (int rowNumber = 0; rowNumber < gridWidth; rowNumber++)
            {
                for (int columnNumber = 0; columnNumber < gridLength; columnNumber++)
                {
                    // Calculate spawn location.
                    float xPosition = (columnNumber * tileDimension) - (tileDimension * gridLength / 2f) + (tileDimension / 2f);
                    float zPosition = (rowNumber * tileDimension) - (tileDimension * gridWidth / 2f) + (tileDimension / 2f);
                    Vector3 spawnPosition = new Vector3(xPosition, Terrain.activeTerrain.SampleHeight(new Vector3(xPosition, 0f, zPosition)), zPosition);

                    // If in endless mode and tile is an edge tile, place it in the staging area; else, the map.
                    if (Global.activeGameMode.Equals(Global.ActiveGameMode.endlessMode) && (rowNumber == 0 || rowNumber == gridWidth - 1 || columnNumber == 0 || columnNumber == gridLength - 1))
                    {
                        // Instantiate staging tile plane and set scale transform.
                        tile = Instantiate(stagingTilePrefab, spawnPosition, Quaternion.identity);
                        tile.transform.localScale = new Vector3(tileDimension, 0.01f, tileDimension);

                        // Ensure correct object hierarchy.
                        tile.transform.parent = stagingAreas.transform;

                        // Name the tile correctly and uniformly.
                        tile.name = "Staging " + stagingTileCount;
                        stagingTileCount++;
                    }
                    else
                    {
                        if (PhotonNetwork.IsConnected)
                        {
                            object[] _name = { "Tile " + playableTileCount };
                            tile = PhotonNetwork.Instantiate("Tile", spawnPosition, Quaternion.identity, 0, _name);
                            tile.transform.localScale = new Vector3(tileDimension, 0.01f, tileDimension);
                            tile.transform.parent = map.transform;
                            // Tile Name in MP is set in TileHelper Start() 
                            playableTileCount++;
                        }
                        else
                        {
                            tile = Instantiate(playableTilePrefab, spawnPosition, Quaternion.identity);
                            tile.transform.localScale = new Vector3(tileDimension, 0.01f, tileDimension);
                            tile.transform.parent = map.transform;
                            // Tile Name Set Here
                            tile.name = "Tile " + playableTileCount;
                            playableTileCount++;
                        }

                        // Deactivate tiles beneath the water in map 3.
                        if (Global.activeGameMap.Equals(Global.ActiveGameMap.map3) && spawnPosition.y < 11.5f)
                        {
                            tile.tag = Global.Tags.occupied;
                            tile.GetComponent<MeshRenderer>().enabled = false;
                            tile.GetComponent<BoxCollider>().enabled = false;
                            foreach (MeshRenderer renderer in tile.GetComponentsInChildren<MeshRenderer>())
                            {
                                renderer.enabled = false;
                            }
                            foreach (BoxCollider collider in tile.GetComponentsInChildren<BoxCollider>())
                            {
                                collider.enabled = false;
                            }
                        }
                    }
                }
            }

            // If in endless mode, decrement length and width back to expected values.
            if (Global.activeGameMode.Equals(Global.ActiveGameMode.endlessMode))
            {
                gridLength -= 2;
                gridWidth -= 2;
            }
        }

        /// <summary>
        /// Pass in a tile to assign it to the world map and set it's dimensions. It's on you to set it's name, however.
        /// </summary>
        public static void SetUpTile(GameObject tile)
        {
            tile.transform.localScale = new Vector3(tileDimension, 0.01f, tileDimension);
            tile.transform.SetParent(map.transform);
        }

        // Generate the structures present on the gameboard.
        [PunRPC]
        private void GenerateStrategicElements()
        {
            GameObject structure = null;
            Transform tileTransform = null;

            int healingTowerLocation = Global.LookUpActiveMapHealingStructureLocation();
            if (healingTowerLocation != -1)
            {
                tileTransform = map.transform.GetChild(healingTowerLocation);
                structure = Instantiate(healingStructurePrefab);
                structure.transform.parent = tileTransform;
                structure.transform.localScale = healingWellScale;
                structure.transform.localPosition = defaultPosition;
                structure.name = "Healing Structure";
                tileTransform.tag = Global.Tags.occupied;
            }

            int attackRangeTowerLocation = Global.LookUpActiveMapAttackRangeStructureLocation();
            if (attackRangeTowerLocation != -1)
            {
                tileTransform = map.transform.GetChild(attackRangeTowerLocation);
                structure = Instantiate(attackRangeStructurePrefab);
                structure.transform.parent = tileTransform;
                structure.transform.localScale = towerScale;
                structure.transform.localPosition = defaultPosition;
                structure.name = "Attack Range Structure";
                tileTransform.tag = Global.Tags.occupied;
            }

            int movementRangeTowerLocation = Global.LookUpActiveMapMovementRangeStructureLocation();
            if (movementRangeTowerLocation != -1)
            {
                tileTransform = map.transform.GetChild(movementRangeTowerLocation);
                structure = Instantiate(movementRangeStructurePrefab);
                structure.transform.parent = tileTransform;
                structure.transform.localScale = towerScale;
                structure.transform.localPosition = defaultPosition;
                structure.name = "Movement Range Structure";
                tileTransform.tag = Global.Tags.occupied;
            }

            int[] obstacleLocationsArray = Global.LookUpActiveMapObstacleLocationsArray();
            for (int idx = 0; idx < obstacleLocationsArray.Length; idx++)
            {
                tileTransform = map.transform.GetChild(obstacleLocationsArray[idx]);
                structure = Instantiate(obstaclePrefab);
                structure.transform.parent = tileTransform;
                structure.transform.localScale = obstacleScale;
                structure.transform.localPosition = obstaclePosition;
                structure.name = "Obstacle";
                tileTransform.tag = Global.Tags.occupied;
            }
        }
    }
}
