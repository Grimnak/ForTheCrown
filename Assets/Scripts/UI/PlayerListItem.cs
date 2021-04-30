/*
* Author: Garrett Morse
* Last Edited By: ""
* Date Created: 2-7-21
* Description: Manages Player List Item Content in HostLobbyCanvas.
* Filename: PlayerListItem.cs
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

namespace Capstone
{
public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] Text text;
    Player player;
    public void SetUpPlayer(Player _player)
    {
        player = _player;
        text.text = _player.NickName;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
}
}