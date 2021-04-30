/*
* Author: Garrett Morse
* Last Edited By: ""
* Date Created: 2-7-21
* Description: When the main menu is loaded, this script connects the client to the photon network.
* It also manages the client's online nickname and informs the player on whether they're connected.
* Filename: MPLauncher.cs
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

namespace Capstone
{
public class NetworkManager : MonoBehaviourPunCallbacks
{   
    public static NetworkManager Instance;
    string version = "0.1";
    [SerializeField] private byte maxPlayers = 2;
    
    [SerializeField]
    private GameObject playerNameInputField;

    [SerializeField]
    private GameObject progressLabel;
    [Header("Online Screen")]
    public Button createRoomButton;
    public Button joinRoomButton;
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // ObjectManager.DontDestroyOnLoad(gameObject);
        this.enabled = true;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            UIDisconnected();
            PhotonNetwork.ConnectUsingSettings(); // settings are configured in Assets/Photon/PUN/Resources/
            PhotonNetwork.GameVersion = version;
        } else {
            UIConnected();
        }
    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(); // this allows us to join and create "rooms"
        // we aren't actually joining a game lobby
        UIConnected();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.JoinLobby();
        Debug.Log("joined lobby again");
    }
    
    private void UIDisconnected()
    {
        progressLabel.SetActive(true);
        playerNameInputField.SetActive(false);
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
    }

    private void UIConnected()
    {
        progressLabel.SetActive(false);
        playerNameInputField.SetActive(true);
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }
}
}