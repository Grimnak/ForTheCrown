/*
* Author: Garrett Morse
* Last Edited By: ""
* Date Created: 2-7-21
* Description: Manages List Item content in JoinLobby Canvas. Joins a master client's room on click.
* Filename: RoomListItem.cs
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

namespace Capstone
{
public class RoomListItem : MonoBehaviour
{
    [SerializeField] 
    public Text text;
    RoomInfo info;
    public void SetRoomUp(RoomInfo _info)
    {
         
        info = _info;
        text.text = _info.Name;
    }

    public void OnClick()
    {
        RoomManager.Instance.JoinRoom(info);
        Debug.Log("Attempting to Join Room: " + info.Name);
    }
}
}