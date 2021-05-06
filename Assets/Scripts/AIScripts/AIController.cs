/* 
 * Author: Will Bartlett
 * Last Edited By:  Eric Henderson
 * Date Created:  2-10-2021
 * Description:  Implements functions that allow the AI to play the game. 
 * Filename:  AIController.cs
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

namespace Capstone
{
    public class AIController : GameController
    {
        public static AIController Instance;
        public int controllerID;
        public int allegiance { get; set; }
        public List<GameObject> stagingArmy;
        private GameObject map;
        public GameObject stagingArea;
        private UtilityCalculator uc;
        private int sideToSpawnOn; // Either 0, 1, 2, or 3, corresponding to N, E, S, and W, respectively.

        private int AIEndlessPromPts;

        [SerializeField] private GameObject knightPrefab;
        [SerializeField] private GameObject archerPrefab;
        [SerializeField] private GameObject clericPrefab;
        [SerializeField] private GameObject siegePrefab;
        [SerializeField] private GameObject horsePrefab;

        int i;
        bool movePhase, attackPhase;
        // Start is called before the first frame update
        void Start()
        {
            if (Instance != null) // if there's already an instance
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            gameArmy = new List<GameObject>();
            map = LevelGenerator.map;
            stagingArea = GameObject.FindGameObjectWithTag(Global.Tags.staging);
            uc = new UtilityCalculator();
            allegiance = 1; // this is constant. AI is always the red team.
            controllerID = 1;
            movePhase = true; 
            attackPhase = false;
            sideToSpawnOn = 1;
            i = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if (GameLogicManager.IsMyTurn(allegiance))  //if it's our turn
            {
                if (gameArmy.Count > 0)
                {
                    if (!GameLogicManager.animationLock)
                    {
                        if (movePhase)
                        {
                            if (i < gameArmy.Count)
                            {
                                UnitController uc = gameArmy[i].GetComponent<UnitController>();
                                if (uc.currentPromotionPoints >= uc.requiredPromotionPoints && uc.totalPromotions < uc.maxPromotions)
                                {
                                    if (uc.currentHealth < uc.totalHealth / 3)
                                    {
                                        uc.PromoteSingleHealOption();
                                    }
                                    else
                                    {
                                        uc.PromoteStatsOption();
                                    }
                                }
                                PerformMove(gameArmy[i]);
                                i++;
                            }
                            else
                            {
                                i = 0;
                                movePhase = false;
                                attackPhase = true;
                            }
                        }
                        else if (attackPhase)
                        {
                            if (i < gameArmy.Count)
                            {
                                PerformAction(gameArmy[i]);
                                i++;
                            }
                            else
                            {
                                i = 0;
                                attackPhase = false;
                            }
                        }
                        else 
                        {
                            GameLogicManager.Instance.NextTurn(); // AI Turn is over.
                            movePhase = true;
                        }
                    }
                }
                else if (stagingArmy.Count > 0) // this can only be true if in endless mode
                {
                    // Move all staged units onto the board's closest unoccupied tile.
                    for (int unitNumber = 0; unitNumber < stagingArmy.Count; unitNumber++)
                    {
                        List<(GameObject, GameObject)> pathList = new List<(GameObject, GameObject)>();
                        GameObject closestTile = TileManager.FindClosestUnoccupiedTile(stagingArmy[unitNumber].transform.parent.gameObject);
                        closestTile.tag = Global.Tags.occupied;
                        pathList.Add((stagingArmy[unitNumber].transform.parent.gameObject, closestTile));

                        UnitController stagingArmyUnitController = stagingArmy[unitNumber].GetComponent<UnitController>();
                        stagingArmyUnitController.animator.SetBool("IsSelected", true);
                        StartCoroutine(stagingArmyUnitController.Moving(pathList));
                        stagingArmy[unitNumber].transform.parent = closestTile.transform;

                        gameArmy.Add(stagingArmy[unitNumber]);
                    }

                    // All the staged units have been accounted for, so clear them from the staging army list and generate next wave.
                    stagingArmy.Clear();

                    // Update next wave spawn location.
                    //if (sideToSpawnOn == 3)
                    //{
                    //    sideToSpawnOn = 0;
                    //}
                    //else
                    //{
                    //    sideToSpawnOn++;
                    //}


                    Spawn(true);

                    movePhase = false;
                    attackPhase = true;
                }
                else // this is only reached if we're not in endless mode and the whole AI army is dead
                {
                    GameLogicManager.Instance.NextTurn(); // AI Turn is over.
                }
            }
        }

        //Method to spawn and populate enemyArmy
        public override void Spawn(bool inStagingArea = false)
        {
            // list of units, dict or abstracted list, pass list to new function that generates list of units to place
            List<int> army = new List<int>();
            if (Global.activeGameMode.Equals(Global.ActiveGameMode.endlessMode))
            {
                army = SpawnEndlessCalc(army);

                // Ensure army size never grows beyond what fits inside map staging area.
                while (army.Count > GameObject.FindGameObjectWithTag(Global.Tags.staging).transform.childCount)
                {
                    army.Remove(Random.Range(0, army.Count));
                }
            }
            else
            {
                army = SpawnCalc(army);
            }

            for (int unitNumber = 0; unitNumber < army.Count; unitNumber++)
            {
                GameObject unit;
                GameObject spawnTile;

                if (inStagingArea)
                {
                    //List<int> validTileIndices = DetermineNextWaveLocation(sideToSpawnOn);
                    spawnTile = stagingArea.transform.GetChild(Random.Range(0, stagingArea.transform.childCount)).gameObject;

                    while (TileManager.FindSpecificChildByTag(spawnTile, "Unit") != null)
                    {
                        spawnTile = stagingArea.transform.GetChild(Random.Range(0, stagingArea.transform.childCount)).gameObject;
                    }
                }
                else
                {
                    spawnTile = map.transform.GetChild(Random.Range(map.transform.childCount - 2 * LevelGenerator.gridLength, map.transform.childCount)).gameObject;

                    while (spawnTile.CompareTag(Global.Tags.occupied))
                    {
                        spawnTile = map.transform.GetChild(Random.Range(map.transform.childCount - 2 * LevelGenerator.gridLength, map.transform.childCount)).gameObject;
                    }

                    spawnTile.tag = Global.Tags.occupied;
                }

                switch (army[unitNumber])
                {
                    case 1:
                        unit = Instantiate(knightPrefab, spawnTile.transform);
                        unit.GetComponent<UnitController>().unitType = "Knight";
                        break;
                    case 2:
                        unit = Instantiate(archerPrefab, spawnTile.transform);
                        unit.GetComponent<UnitController>().unitType = "Archer";
                        break;
                    case 3:
                        unit = Instantiate(clericPrefab, spawnTile.transform);
                        unit.GetComponent<UnitController>().unitType = "Cleric";
                        break;
                    case 4:
                        unit = Instantiate(siegePrefab, spawnTile.transform);
                        unit.GetComponent<UnitController>().unitType = "Siege";
                        break;
                    case 5:
                        unit = Instantiate(horsePrefab, spawnTile.transform);
                        unit.GetComponent<UnitController>().unitType = "Horseman";
                        break;
                    default:
                        unit = null; //This should never happen
                        break;
                }
                if (inStagingArea)
                {
                    unit.transform.LookAt(new Vector3(map.transform.position.x, unit.transform.position.y, map.transform.position.z));
                    stagingArmy.Add(unit);
                }
                else
                {
                    unit.transform.eulerAngles = new Vector3(0, 180, 0);
                    gameArmy.Add(unit);
                }

                unit.transform.localScale = LevelGenerator.unitScale;
                unit.transform.parent = spawnTile.transform;
                unit.name = unit.GetComponent<UnitController>().unitType;
                unit.transform.Find("red_team_aura").GetComponent<ParticleSystem>().Play();
            }

            if (GameLogicManager.Instance.endlessWaveClearCount >= 5)
            {
                AIEndlessPromPts = GameLogicManager.Instance.endlessWaveClearCount - 4;

                // Ensure more promotions points are given than can possibly be given to the new wave.
                if (AIEndlessPromPts > 3 * gameArmy.Count)
                {
                    AIEndlessPromPts = 3 * gameArmy.Count;
                }

                while (AIEndlessPromPts > 0)
                {
                    UnitController uc = gameArmy[Random.Range(0, gameArmy.Count)].GetComponent<UnitController>();
                    if (uc.totalPromotions < uc.maxPromotions)
                    {
                        uc.PromoteStatsOption();
                        uc.currentHealth = uc.totalHealth;
                        AIEndlessPromPts--;
                    }
                }
            }
            
        }

        //bools for unit exclusion, checks for training mode
        public List<int> SpawnCalc(List<int> army)
        {
            bool knightBan = false;
            bool archerBan = false;
            bool clericBan = false;
            bool siegeBan = false;
            bool horseBan = false;
            int popCap;

            switch (Global.activeGameMode)           //All values are subject to change 
            {
                case Global.ActiveGameMode.training1:
                    archerBan = true;
                    clericBan = true;
                    siegeBan = true;
                    popCap = 10;
                    break;

                case Global.ActiveGameMode.training2:
                    clericBan = true;
                    siegeBan = true;
                    popCap = 10;
                    break;

                case Global.ActiveGameMode.training3:
                    siegeBan = true;
                    popCap = 10;
                    break;

                default:
                    popCap = 15;
                    break;
            }
            if (!horseBan)
            {
                int horse = Random.Range(10, 20);
                horse /= 10;

                for (int i = 0; i < horse; i++)
                {
                    army.Add(5);
                    popCap -= Global.UnitPopulationCost.horseman;
                }
            }
            if (!siegeBan)
            {
                int siege = Random.Range(0, 15);
                siege /= 10;
            
                for (int i = 0; i < siege; i++)
                {
                    army.Add(4);
                    popCap -= Global.UnitPopulationCost.siege;
                }
            }
            if (!clericBan)
            {
                int cleric = Random.Range(10, 25);
                cleric /= 10;
        
                for (int i = 0; i < cleric; i++)
                {
                    army.Add(3);
                    popCap -= Global.UnitPopulationCost.cleric;
                }
            }
            if (!archerBan)
            {
                int archer = Random.Range(5, 35);
                archer /= 10;

                for (int i = 0; i < archer; i++)
                {
                    army.Add(2);
                    popCap -= Global.UnitPopulationCost.archer;
                }
            }
            if (!knightBan)
            {
                while (popCap > 0)
                {
                    army.Add(1);
                    popCap -= Global.UnitPopulationCost.knight;
                }
                
            }
            return army;
        }
    
        public List<int> SpawnEndlessCalc(List<int> army)
        {
            int waveCount = GameLogicManager.Instance.endlessWaveClearCount;
            int popCap = 2 * (waveCount + 1) + 4;       //Arbitrary addition to make endless feel quicker
            if (waveCount >= 1)
            {
                int archer = Random.Range(15, 35);
                archer /= 10;

                for (int i = 0; i < archer; i++)
                {
                    if (popCap >= Global.UnitPopulationCost.archer) { popCap -= Global.UnitPopulationCost.archer; army.Add(2); }
                    else break;
                }
            }
            if (waveCount >= 3)
            {
                int horse = Random.Range(15, 35);
                horse /= 10;

                for (int i = 0; i < horse; i++)
                {
                    if (popCap >= Global.UnitPopulationCost.horseman) { popCap -= Global.UnitPopulationCost.horseman; army.Add(5); }
                    else break;
                }
            }
            if (waveCount >= 2)
            {
                int cleric = Random.Range(15, 25);
                cleric /= 10;

                for (int i = 0; i < cleric; i++)
                {
                    if (popCap >= Global.UnitPopulationCost.cleric) { popCap -= Global.UnitPopulationCost.cleric; army.Add(3); }
                    else break;
                }
            }
            if (waveCount >= 4)
            {
                int siege = Random.Range(5, 15);
                siege /= 10;

                for (int i = 0; i < siege; i++)
                {
                    if (popCap >= Global.UnitPopulationCost.siege) { popCap -= Global.UnitPopulationCost.siege; army.Add(4); }
                    else break;
                }
            }
            while (popCap >= Global.UnitPopulationCost.knight)      
            {
                army.Add(1);
                popCap -= Global.UnitPopulationCost.knight;
            }
            return army;
        }

        // Determine which side of the map the next endless wave will spawn.
        private List<int> DetermineNextWaveLocation(int sideToSpawnOn)
        {
            int minNumOfSiege = 0;
            int maxNumOfSiege = 15;
            int siege = Random.Range(minNumOfSiege, maxNumOfSiege) / 10;
            List<int> eligibleTileIndices = new List<int>();

            for (int tileIndex = 0; tileIndex < stagingArea.transform.childCount; tileIndex++)
            {
                // North.
                if (sideToSpawnOn == 0 && tileIndex >= stagingArea.transform.childCount - (LevelGenerator.gridLength + 2))
                {
                    eligibleTileIndices.Add(tileIndex);
                }
                // East.
                else if (sideToSpawnOn == 1 && ((tileIndex >= LevelGenerator.gridLength + 1 && tileIndex < stagingArea.transform.childCount - (LevelGenerator.gridLength + 2) && tileIndex % 2 != 0) || tileIndex == stagingArea.transform.childCount - 1))
                {
                    eligibleTileIndices.Add(tileIndex);
                }
                // South.
                else if (sideToSpawnOn == 2 && tileIndex < LevelGenerator.gridLength + 2)
                {
                    eligibleTileIndices.Add(tileIndex);
                }
                // West.
                else if (sideToSpawnOn == 3 && ((tileIndex > LevelGenerator.gridLength + 1 && tileIndex <= stagingArea.transform.childCount - (LevelGenerator.gridLength + 2) && tileIndex % 2 == 0) || tileIndex == 0))
                {
                    eligibleTileIndices.Add(tileIndex);
                }
            }
            return eligibleTileIndices;
        }

        public override void Attack(string _me, string _target)
        {
            throw new System.NotImplementedException();
        }

        public override void Move(string _me, string _target)
        {
            throw new System.NotImplementedException();
        }
        public override void Initialize()
        {
            GameLogicManager.Instance.controllers[1] = this;
        }

        public override void Heal(string _me, string _target)
        {
            throw new System.NotImplementedException();
        }
    
        public void PerformMove(GameObject unit)
        {
            GameObject selectedTile = CalculateMoveUtility(unit);      //calculate best possible move, that move becomes selectedTile

            if (selectedTile != unit.transform.parent.gameObject)               //if best tile is the current tile we are on, don't move anywhere
            {
                UnitController unitController = unit.GetComponent<UnitController>();
                unitController.target = selectedTile;         //set target of the enemy

                unitController.Move();               //move the enemy

                unit.transform.parent.tag = Global.Tags.inactive;
                unit.transform.parent = selectedTile.transform;            //me attempting to reset where the enemy thinks it is
                selectedTile.tag = Global.Tags.occupied;
            }
        }

        public void PerformAction(GameObject unit)
        {
            switch (unit.name)
            {
                case "Knight":
                    PerformKnightAction(unit);
                    break;
                case "Archer":
                    PerformArcherAction(unit);
                    break;
                case "Cleric":
                    PerformClericAction(unit);
                    break;
                case "Horseman":
                    PerformHorsemanAction(unit);
                    break;
                case "Siege":
                    PerformSiegeAction(unit);
                    break;
                default:
                    break;
            }     
        }

        private void PerformKnightAction(GameObject knight)
        {
            UnitController knightController = knight.GetComponent<UnitController>();
            GameObject selectedTile = CalculateAttackUtility(knight);

            if (knightController.remainingSpecialAbilityCooldown == 0 && selectedTile == null)
            {
                int nearbyEnemyUnits = 0;

                for (int enemyUnit = 0; enemyUnit < GameLogicManager.Instance.controllers[GameLogicManager.Instance.myID].gameArmy.Count; enemyUnit++)
                {
                    if (TileManager.CombatDistanceBetweenTiles(knight.transform.parent.gameObject, GameLogicManager.Instance.controllers[GameLogicManager.Instance.myID].gameArmy[enemyUnit].transform.parent.gameObject) <= 3)
                    {
                        nearbyEnemyUnits++;
                    }
                }

                if (nearbyEnemyUnits > 0)
                {
                    knightController.UseSpecialAbility();
                }
            }
            else if (selectedTile != null)
            {
                knightController.target = TileManager.FindSpecificChildByTag(selectedTile, "Unit");
                knightController.Attack();
            }
        }

        private void PerformArcherAction(GameObject archer)
        {
            UnitController archerController = archer.GetComponent<UnitController>();
            GameObject selectedTile = CalculateAttackUtility(archer);

            if (archerController.remainingSpecialAbilityCooldown == 0 && selectedTile != null)
            {
                int adjacentEnemyUnits = 0;

                for (int enemyUnit = 0; enemyUnit < GameLogicManager.Instance.controllers[GameLogicManager.Instance.myID].gameArmy.Count; enemyUnit++)
                {
                    if (TileManager.CombatDistanceBetweenTiles(selectedTile, GameLogicManager.Instance.controllers[GameLogicManager.Instance.myID].gameArmy[enemyUnit].transform.parent.gameObject) == 1)
                    {
                        adjacentEnemyUnits++;
                    }
                }

                if (adjacentEnemyUnits > 0)
                {
                    archerController.UseSpecialAbility();
                }

                archerController.target = TileManager.FindSpecificChildByTag(selectedTile, "Unit");
                archerController.Attack();
            }
            else if (selectedTile != null)
            {
                archerController.target = TileManager.FindSpecificChildByTag(selectedTile, "Unit");
                archerController.Attack();
            }
        }

        private void PerformClericAction(GameObject cleric)
        {
            UnitController clericController = cleric.GetComponent<UnitController>();

            if (clericController.remainingSpecialAbilityCooldown == 0)
            {
                int adjacentFriendlyInjuredUnits = 0;

                for (int friendlyUnitIndex = 0; friendlyUnitIndex < gameArmy.Count; friendlyUnitIndex++)
                {
                    GameObject friendlyUnit = gameArmy[friendlyUnitIndex];
                    UnitController friendlyUC = friendlyUnit.GetComponent<UnitController>();
                    if (TileManager.CombatDistanceBetweenTiles(cleric, friendlyUnit.transform.parent.gameObject) == 1 && friendlyUC.currentHealth < friendlyUC.totalHealth)
                    {
                        adjacentFriendlyInjuredUnits++;
                    }
                }

                if (adjacentFriendlyInjuredUnits > 1)
                {
                    clericController.UseSpecialAbility();
                }
            }
            
            if (!clericController.hasActed)
            {
                GameObject selectedTile = CalculateHealUtility(cleric);

                if (selectedTile != null)
                {
                    clericController.target = TileManager.FindSpecificChildByTag(selectedTile, "Unit");
                    clericController.Heal();
                }
                else
                {
                    selectedTile = CalculateAttackUtility(cleric);

                    if (selectedTile != null)
                    {
                        clericController.target = TileManager.FindSpecificChildByTag(selectedTile, "Unit");
                        clericController.Attack();
                    }
                }
            }
        }

        private void PerformHorsemanAction(GameObject horseman)
        {
            UnitController horsemanController = horseman.GetComponent<UnitController>();
            GameObject selectedTile = CalculateAttackUtility(horseman);

            if (horsemanController.remainingSpecialAbilityCooldown == 0 && selectedTile == null)
            {
                horsemanController.attackRange++;
                selectedTile = CalculateAttackUtility(horseman);
                horsemanController.attackRange--;
                if (selectedTile != null)
                {
                    horsemanController.UseSpecialAbility();
                    horsemanController.target = TileManager.FindSpecificChildByTag(selectedTile, "Unit");
                    horsemanController.Attack();
                }
            }
            else if (selectedTile != null)
            {
                horsemanController.target = TileManager.FindSpecificChildByTag(selectedTile, "Unit");
                horsemanController.Attack();
            }
        }

        private void PerformSiegeAction(GameObject siege)
        {
            UnitController siegeController = siege.GetComponent<UnitController>();
            GameObject selectedTile = CalculateSiegeAttackUtility(siege);

            if (siegeController.remainingSpecialAbilityCooldown == 0 && selectedTile == null)
            {
                siegeController.UseSpecialAbility();
                selectedTile = CalculateSiegeAttackUtility(siege);
            }
            
            if (selectedTile != null)
            {
                siegeController.target = TileManager.FindSpecificChildByTag(selectedTile, "Unit");
                siegeController.Attack();
            }
        }

        private GameObject CalculateMoveUtility(GameObject unit)        
        {
            UnitController unitController = unit.GetComponent<UnitController>();
            GameObject finalTile;
            GameObject tile = unit.transform.parent.gameObject;
            finalTile = tile;
            float maxUtility = Mathf.NegativeInfinity;

            foreach(GameObject currentTile in FindPossibleMoveTiles(tile, unitController.currentMovementRange))
            {
                float currentUtility = uc.Calculate(currentTile, unitController);
                if (currentUtility > maxUtility)
                {
                    maxUtility = currentUtility;
                    finalTile = currentTile;
                }
            }

            return finalTile;
        }

        private GameObject CalculateAttackUtility(GameObject unit)
        {
            GameObject finalTile;
            GameObject tile = unit.transform.parent.gameObject;    
            finalTile = null;
            float minHealth = Mathf.Infinity;

            foreach (GameObject currentTile in FindPossibleActionTiles(tile, unit.GetComponent<UnitController>().attackRange, false))
            {
                GameObject target = TileManager.FindSpecificChildByTag(currentTile, "Unit");
                if (target != null)
                {
                    UnitController tUC = target.GetComponent<UnitController>();
                    if (tUC.currentHealth < minHealth && tUC.currentHealth > 0)
                    {
                        minHealth = tUC.currentHealth;
                        finalTile = currentTile;
                    }
                }
            }

            return finalTile;
        }

        private GameObject CalculateSiegeAttackUtility(GameObject siege)
        {
            GameObject finalTile;
            GameObject tile = siege.transform.parent.gameObject;
            finalTile = null;
            float maxEnemyUnits = Mathf.NegativeInfinity;

            foreach (GameObject currentTile in FindPossibleActionTiles(tile, siege.GetComponent<UnitController>().attackRange, false))
            {
                GameObject target = TileManager.FindSpecificChildByTag(currentTile, "Unit");
                int nearbyFriendlyUnits = 0;
                int nearbyEnemyUnits = 1;  // starts at 1 because primary target is always an enemy

                if (target != null)
                {
                    for (int neighboringTileIndex = 0; neighboringTileIndex < LevelGenerator.map.transform.childCount; neighboringTileIndex++)
                    {
                        GameObject neighboringTile = LevelGenerator.map.transform.GetChild(neighboringTileIndex).gameObject;
                        if (TileManager.CombatDistanceBetweenTiles(neighboringTile, currentTile) == 1)
                        {
                            GameObject neighboringUnit = TileManager.FindSpecificChildByTag(neighboringTile, "Unit");
                            if (neighboringUnit != null)
                            {
                                if (gameArmy.Contains(neighboringUnit))
                                {
                                    nearbyFriendlyUnits++;
                                }
                                else if (GameLogicManager.Instance.controllers[0].gameArmy.Contains(neighboringUnit))
                                {
                                    nearbyEnemyUnits++;
                                }
                            }
                        }
                    }

                    if (nearbyEnemyUnits > nearbyFriendlyUnits && nearbyEnemyUnits > maxEnemyUnits)
                    {
                        maxEnemyUnits = nearbyEnemyUnits;
                        finalTile = currentTile;
                    }
                }
            }

            return finalTile;
        }

        private GameObject CalculateHealUtility(GameObject unit)
        {
            GameObject finalTile;
            GameObject tile = unit.transform.parent.gameObject;
            finalTile = null;
            float minRatio = 1;

            foreach (GameObject currentTile in FindPossibleActionTiles(tile, unit.GetComponent<UnitController>().attackRange, true))
            {
                GameObject target = TileManager.FindSpecificChildByTag(currentTile, "Unit");
                if (target != null && currentTile != tile)
                {
                    UnitController tUC = target.GetComponent<UnitController>();
                    if (((float)tUC.currentHealth / tUC.totalHealth) < minRatio && tUC.currentHealth > 0)
                    {
                        minRatio = (float)tUC.currentHealth / tUC.totalHealth;
                        finalTile = currentTile;
                    }
                }
            }

            return finalTile;
        }

        private List<GameObject> FindPossibleMoveTiles(GameObject tile, int moveRange)
        {
            List<GameObject> tiles = new List<GameObject>();

            int selectedTileIndex = tile.transform.GetSiblingIndex();
            for (int candidateTileIndex = 0; candidateTileIndex < map.transform.childCount; candidateTileIndex++)
            {
                GameObject candidateTile = map.transform.GetChild(candidateTileIndex).gameObject;

                // Limit the possible range to a square box with the remaining moves as its length and width.
                if (TileManager.MoveDistanceBetweenTiles(tile, candidateTile) <= moveRange)
                {
                    // A tile is an acceptable movement location if it is not occupied.
                    if (!candidateTile.CompareTag(Global.Tags.occupied)) //candidateTileIndex == selectedTileIndex rare bug that causes find path error
                    {
                        var route = TileManager.FindPath(map, tile, candidateTile);

                        // Further limit the possible options by accounting for obstacles noticed during pathing.
                        if (route != null && route.Count <= moveRange)
                        {
                                tiles.Add(map.transform.GetChild(candidateTileIndex).gameObject);
                        }
                    }
                }
            }

            return tiles;
        }

        private List<GameObject> FindPossibleActionTiles(GameObject tile, int attackRange, bool healing)
        {
            List<GameObject> tiles = new List<GameObject>();

            int selectedTileIndex = tile.transform.GetSiblingIndex();
            for (int candidateTileIndex = 0; candidateTileIndex < map.transform.childCount; candidateTileIndex++)
            {
                // Limit the possible range to a square box with the remaining attacks as its length and width.
                if (Mathf.Abs(candidateTileIndex % LevelGenerator.gridLength - selectedTileIndex % LevelGenerator.gridLength) <= attackRange)
                {
                    int horizontalDistanceFromSelectedTile = Mathf.Abs(candidateTileIndex % LevelGenerator.gridLength - selectedTileIndex % LevelGenerator.gridLength);
                    GameObject candidate = TileManager.FindSpecificChildByTag(map.transform.GetChild(candidateTileIndex).gameObject, "Unit");
                    // Further limit the possible range based on how many tiles would have to be traveled for an action to reach any particular point within the square box.
                    // A tile is an acceptable action location if it is vertically located within the box.
                    if (candidateTileIndex % LevelGenerator.gridLength >= selectedTileIndex % LevelGenerator.gridLength && Mathf.Abs(candidateTileIndex - (selectedTileIndex + horizontalDistanceFromSelectedTile)) <= attackRange * LevelGenerator.gridLength)
                    {
                        if (candidate != null && ((!healing && candidate.GetComponent<UnitController>().allegiance == 0) || (healing && candidate.GetComponent<UnitController>().allegiance == 1)))
                        {
                            tiles.Add(map.transform.GetChild(candidateTileIndex).gameObject);
                        }
                    }
                    else if (candidateTileIndex % LevelGenerator.gridLength < selectedTileIndex % LevelGenerator.gridLength && Mathf.Abs(candidateTileIndex - (selectedTileIndex - horizontalDistanceFromSelectedTile)) <= attackRange * LevelGenerator.gridLength)
                    {
                        if (candidate != null && ((!healing && candidate.GetComponent<UnitController>().allegiance == 0) || (healing && candidate.GetComponent<UnitController>().allegiance == 1)))
                        {
                            tiles.Add(map.transform.GetChild(candidateTileIndex).gameObject);
                        }
                    }
                }
            }

            return tiles;
        }
    }
}
