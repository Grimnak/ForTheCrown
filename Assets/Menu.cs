using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Author: Longfei Yu
 * Last Edited By: ""
 * Date Created: 2-11-21
 * Description: Menu class. Assigned to JoinLobbyMenu and HostLobbyMenu
 * Filename: Menu.cs
 */
public class Menu : MonoBehaviour
{
    public string menuName;
    public bool open;

    public void Open()
    {
        open = gameObject.activeSelf;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        open = gameObject.activeSelf;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        open = gameObject.activeSelf;
    }
}
