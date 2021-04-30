/*
* Author: Garrett Morse
* Last Edited By: ""
* Date Created: 2-7-21
* Description: Manages online rooms in the Menu scene. Controls players creating, leaving and joining rooms.
* Filename: RoomManager.cs
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
namespace Capstone
{
public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] Text roomNameText;
    
    [Header("Room Cards")]
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [Header("Player Cards")]
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject playerListItemPrefab;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        this.enabled = true;
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(roomNameInputField.text);
    }

    public void JoinRoom(RoomInfo info)
    {
        //Open the host lobby menu. By Longfei
        MenuManager.Instance.OpenMenu("host");

        PhotonNetwork.JoinRoom(info.Name);
        Debug.Log("Joining Room: " + info.Name);
    }

    public override void OnConnectedToMaster()
    {
        
    }
    public override void OnJoinedRoom()
    {
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;
        foreach (Transform el in playerListContent)
            Destroy(el.gameObject);
        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUpPlayer(players[i]);
            if (players[i] != players[0])
                Destroy(GameObject.Find("startGame"));
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to create room: " + returnCode + "\n" + message);
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        for (int i = 0; i < playerListContent.childCount; i++)
            Destroy(playerListContent.transform.GetChild(i).gameObject);
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName);

        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUpPlayer(other);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (SceneManager.GetActiveScene().name.Contains(Global.gameScene)) 
        { 
            SceneManager.LoadScene(Global.menuScene);  
            NetworkManager.Instance.Disconnect(); 
        }
        foreach (Transform el in playerListContent)
        {
            Destroy(el.gameObject);
        }
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUpPlayer(PhotonNetwork.PlayerList[i]);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        List<string> curNames = new List<string>();
        int i = 0;
        GameObject[] allContent = new GameObject[roomListContent.childCount];
        foreach (Transform el in roomListContent)
        {
            allContent[i] = el.gameObject;
            i++;
        }
        foreach (GameObject ch in allContent) Destroy(ch);
        
        for (i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount < 2)
            {
                Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetRoomUp(roomList[i]);
            }
        }
    }

    // for the client to store map ID before game starts.
    [PunRPC]
    public void ReceiveMapData(int mapID)
    {
        Global.activeGameMap = mapID;
    }

    [PunRPC]
    public void LoadGame(int mapID)
    {
        if (PhotonNetwork.PlayerList.Length == 2)
        {
            photonView.RPC("ReceiveMapData", RpcTarget.All, mapID);
            Debug.LogFormat("PhotonNetwork: Loading...Player count: {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.LoadLevel("PopulationSelectScene");
        }
    }
}
}