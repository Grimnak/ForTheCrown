/* 
 * Author:  Eric Henderson
 * Last Edited By:  Eric Henderson
 * Date Created:  2-20-2021
 * Description:  This script manages the army population select logic.
 * Filename:  PopulationSelectLogicManager.cs
 */

using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Linq;

namespace Capstone
{
public class PopulationSelectLogicManager : MonoBehaviour
{
    public int allegiance;
    public int populationPointsToSpend;
    public Dictionary<string, int> unitDict = new Dictionary<string, int>() { { "Knight", 0 }, { "Archer", 0 }, { "Cleric", 0 }, {"Siege", 0 }, { "Horseman", 0 } };
    public PopulationUI popUI;
    private UnityAction<string> AddUnitAction;
    private UnityAction<string> RemoveUnitAction;
    void Start()
    {
        allegiance = PhotonNetwork.IsConnected ? (PhotonNetwork.IsMasterClient ? 0 : 1) : 0;
        populationPointsToSpend = 25;  // we'll eventually attach this to a slider menu option
        ObjectManager.DontDestroyOnLoad(gameObject);

        AddUnitAction = new UnityAction<string>(OnClick);
        RemoveUnitAction = new UnityAction<string>(OnRemove);
        PopulationUI.instance.AddUnitEvent.AddListener(AddUnitAction);
        PopulationUI.instance.RemoveUnitEvent.AddListener(RemoveUnitAction);
        PopulationUI.instance.SetPopulationPoints(populationPointsToSpend);
    }

    // Add units to the list of player units.
    public void OnClick(string unitType)
    {
        int unitCost = Global.LookUpPopulationCost(unitType);
        if (populationPointsToSpend >= unitCost || !PopulationUI.instance.selectionPhase)
        {
            unitDict[unitType]++;
            populationPointsToSpend -= unitCost;
            var dictLines = unitDict.Select(kvp => kvp.Key + ": " + kvp.Value.ToString());
            PopulationUI.instance.SetPopulationPoints(populationPointsToSpend);
            Debug.Log(string.Join("\n", dictLines));
        }
    }

    // Remove units from the list of player units.
    public void OnRemove(string unitType)
    {
        unitDict[unitType] = unitDict[unitType] <= 0 ? 0 : unitDict[unitType] - 1;
        if (SceneManager.GetActiveScene().name.Equals("PopulationSelectScene")) populationPointsToSpend += Global.LookUpPopulationCost(unitType);
        PopulationUI.instance.SetPopulationPoints(populationPointsToSpend);
    }
}
}