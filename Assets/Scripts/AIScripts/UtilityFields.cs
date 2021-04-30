using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Capstone
{
public class UtilityFields
{
    private GameObject map = LevelGenerator.map;
    public float team(int numOfTeam)
    {
        float score;
        if (numOfTeam == 0)
        {
            score = 0;
        }
        else
        {
            score = scoreEquation(numOfTeam);
        }
        return score;
    }

    public float enemies(int numOfEnemy)
    {
        float score;
        if (numOfEnemy == 0)
        {
            score = 0;
        }
        else
        {
            score = scoreEquation(numOfEnemy);
        }
        return score;
    }

    public int terrain(float height)
    {
        height *= 10;

        return (int)height;
    }

    public float archerDistance(float totalDist)
    {
        float distFromAvg;
        float maxDist = (map.transform.GetChild(map.transform.childCount - 1).position - map.transform.GetChild(0).position).magnitude;
        float minDist = 0;
        distFromAvg = normalize10(totalDist, maxDist, minDist);
        return distFromAvg;
    }
    public float towardsEnemy(Vector3 avgPos, GameObject tile)
    {
        avgPos /= GameLogicManager.Instance.controllers[0].gameArmy.Count; // human player is always 0
        avgPos -= tile.transform.position;
        float distFromAvg = avgPos.magnitude;
        float maxDist = (map.transform.GetChild(map.transform.childCount - 1).position - map.transform.GetChild(0).position).magnitude;
        float minDist = 0;
        distFromAvg = normalize10(distFromAvg, maxDist, minDist);
        return distFromAvg;
    }
    private float scoreEquation(int x)
    {
        float y;
        y = -((x - 1) / 2) + 1;         //Slope of -1/2 and starting at point (1, 1)
        return y;
    }

    private float normalize10(float raw, float max, float min)
    {
        return 10 * (raw - min) / (max - min);
    }
}
}