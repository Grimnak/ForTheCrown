/*
* Author: Garrett Morse
* Last Edited By: ""
* Date Created: 2-7-21
* Description: Manages the player's online persona. Controls the input field in the Online canvas.
* Filename: PlayerNameInputField.cs
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

namespace Capstone
{

[RequireComponent(typeof(TMP_InputField))]
public class PlayerNameInputField : MonoBehaviour
{
    const string playerNamePrefKey = "PlayerName";

    // Gets the Player's name, if previously stored
    void Start()
    {
        string defaultName = "";
        InputField _inputField = this.GetComponent<InputField>();
        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName; // player's name over the network
    }

    // Sets player's name and saves it to PlayerPrefs for future sessions
    public void SetPlayerName(string newName)
    {
        if (string.IsNullOrEmpty(newName))
        {
            Debug.LogError("Player name is null or empty.");
            return;
        }
        PhotonNetwork.NickName = newName;
        PlayerPrefs.SetString(playerNamePrefKey, newName);
    }
}
}