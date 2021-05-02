using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Capstone
{
public class UtilityCalculator
{
    float totalUtility;
    int moveRange;
    int attackRange;
    UtilityFields uf;

    List<GameObject> tiles = new List<GameObject>();
    List<GameObject> enemyTiles = new List<GameObject>();

    private UnitController unit;
    private GameObject map;

    public float Calculate(GameObject tile, UnitController enemy)
    {
        uf = new UtilityFields();
        map = LevelGenerator.map;
        unit = enemy;
        moveRange = unit.defaultMovementRange;
        attackRange = unit.attackRange;
        totalUtility = 0;

        if (enemy.unitType.CompareTo("Cleric") == 0)
        {
            totalUtility += Teammates(tile) * 3;
        }
        else
        {
            totalUtility += Teammates(tile);
            totalUtility += Enemies(tile);
        }

        totalUtility -= AvoidSiege(tile);
        totalUtility -= MoveTowardsEnemies(tile);

        if (enemy.unitType.CompareTo("Archer") == 0 && enemyTiles.Count > 0)
        {
            totalUtility += (ArcherEnemies(tile, enemyTiles) * 2);
        }

        return totalUtility;
    }
    
    float Teammates(GameObject tile)
    {
        int selectedTileIndex = tile.transform.GetSiblingIndex();

        for (int candidateTileIndex = 0; candidateTileIndex < map.transform.childCount; candidateTileIndex++)
        {
            GameObject candidateTile = map.transform.GetChild(candidateTileIndex).gameObject;

            // Limit the possible range to a square box with the remaining moves as its length and width.
            if (TileManager.MoveDistanceBetweenTiles(candidateTile, tile) <= moveRange)
            {
                // A tile is an acceptable movement location if it is not occupied.
                if (candidateTileIndex == selectedTileIndex || !candidateTile.CompareTag(Global.Tags.occupied))
                {
                    var pathList = TileManager.FindPath(map, tile, candidateTile);
                    if (pathList != null)
                    {
                        // Further limit the possible options by accounting for obstacles noticed during pathing.
                        if (pathList.Count <= moveRange)
                        {
                            tiles.Add(map.transform.GetChild(candidateTileIndex).gameObject);
                        }
                    }
                }
            }
        }
        int numOfTeam = 0;
        foreach (GameObject t in tiles)
        {
            GameObject candidate = TileManager.FindSpecificChildByTag(t, "Unit");
            if (candidate != null && candidate.GetComponent<UnitController>().allegiance == 1)
            {
                numOfTeam++;
            }
        }
        tiles.Clear();
        return uf.team(numOfTeam);

    }

    float Enemies(GameObject tile)
    {
        int selectedTileIndex = tile.transform.GetSiblingIndex();

        for (int candidateTileIndex = 0; candidateTileIndex < map.transform.childCount; candidateTileIndex++)
        {
            // Limit the possible range to a square box with the remaining attacks as its length and width.
            if (Mathf.Abs(candidateTileIndex % LevelGenerator.gridLength - selectedTileIndex % LevelGenerator.gridLength) <= attackRange)
            {
                int horizontalDistanceFromSelectedTile = Mathf.Abs(candidateTileIndex % LevelGenerator.gridLength - selectedTileIndex % LevelGenerator.gridLength);

                // Further limit the possible range based on how many tiles would have to be traveled for an attack to reach any particular point within the square box.
                // A tile is an acceptable attack location if it is vertically located within the box.
                if (candidateTileIndex % LevelGenerator.gridLength >= selectedTileIndex % LevelGenerator.gridLength && Mathf.Abs(candidateTileIndex - (selectedTileIndex + horizontalDistanceFromSelectedTile)) <= attackRange * LevelGenerator.gridLength)
                {
                    tiles.Add(map.transform.GetChild(candidateTileIndex).gameObject);
                }
                else if (candidateTileIndex % LevelGenerator.gridLength < selectedTileIndex % LevelGenerator.gridLength && Mathf.Abs(candidateTileIndex - (selectedTileIndex - horizontalDistanceFromSelectedTile)) <= attackRange * LevelGenerator.gridLength)
                {
                    tiles.Add(map.transform.GetChild(candidateTileIndex).gameObject);
                }
            }
        }

        int numOfEnemy = 0;
        foreach (GameObject t in tiles)
        {
            GameObject candidate = TileManager.FindSpecificChildByTag(t, "Unit");
            if (candidate != null && candidate.GetComponent<UnitController>().allegiance == 0)
            {
                enemyTiles.Add(t);
                numOfEnemy++;
            }
        }
        tiles.Clear();
        return uf.enemies(numOfEnemy);

    }

    float AvoidSiege(GameObject tile)
    {
            if (tile.transform.Find("Incoming_Effect").GetComponent<ParticleSystem>().isPlaying)
            {
                return 100f;
            }
            else
            {
                return 0;
            }
    }

    float ArcherEnemies(GameObject tile, List<GameObject> enemyTiles)
    {
        float totalDist = 0;
        foreach (GameObject enemyTile in enemyTiles)
        {
            float dist = Vector3.Distance(tile.transform.position, enemyTile.transform.position);
            totalDist += dist;
        }
        totalDist /= enemyTiles.Count;
        enemyTiles.Clear();
        return uf.archerDistance(totalDist);
    }

    float MoveTowardsEnemies(GameObject tile)
    {
        Vector3 avgPos = Vector3.zero;

        foreach(GameObject playerUnit in GameLogicManager.Instance.controllers[0].gameArmy) // human player is always 0.
        {
            avgPos += playerUnit.transform.position;
        }
        return uf.towardsEnemy(avgPos, tile);
    }
}
}