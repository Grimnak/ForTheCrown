/* 
 * Author: Garrett Morse
 * Last Edited By:
 * Date Created: 3-14-2021
 * Description: Interface for all players of the game to implement.
 * Filename: IPlayer.cs
 */
using UnityEngine;
using System.Collections.Generic;
using Photon.Realtime;
public abstract class GameController : MonoBehaviour
{
    int allegiance { get; set; }
    int controllerID;
    public List<GameObject> gameArmy { get; set; }
    // human players will also have sth. like popArmyList and placedArmyStack to handle Population Select/Place logic.
    public abstract void Attack(string _me, string _target);
    public abstract void Move(string _me, string _target);
    public abstract void Heal(string _me, string _target);
    public abstract void Spawn(bool inStagingArea = false); // spawn your units from an army collection. AI will just use its gameArmy. humans will use population at first.

    public abstract void Initialize();
}