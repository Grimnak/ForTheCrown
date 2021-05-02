/* 
 * Author: Will Bartlett
 * Last Edited By:  Eric Henderson
 * Date Created:  2-7-2021
 * Description:  Implements functions that allow user to use the pause menu. 
 * Filename:  PauseMenu.cs
 */

using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections.Generic;

namespace Capstone
{
public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;


    // Pause the game only when escape is pressed and no units are currently selected.
    void Update()
    {
        int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
        PlayerController pc = GameLogicManager.Instance.controllers[id] as PlayerController;
        if (pc != null && TileManager.currentlySelectedTile == null && Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
        else if (pc == null && Input.GetKeyDown(KeyCode.Escape))
        {
            LoadMenu();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    private void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void LoadMenu()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (!PhotonNetwork.IsMasterClient) PhotonNetwork.LeaveRoom();
            NetworkManager.Instance.Disconnect();
        }
        Cursor.lockState = CursorLockMode.None;
        UnlockAnimationLock();
        ObjectManager.ClearDDOLs();
        Global.ResetGameState();
        SceneManager.LoadScene(Global.menuScene);
    }

    public void QuitGame()
    {
        Cursor.lockState = CursorLockMode.None;
        UnlockAnimationLock();
        SceneManager.LoadScene(Global.exitScene);
    }

    /*
        * Author: Longfei Yu
        * Last Edited By: Longfei Yu
        * Date Created: 1-30-2021
        * Description: Reset the current level as the user clicked on the restart button
        */
    public void RestartLevel()
    {
        UnlockAnimationLock();
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    // keeps the music player
    

    /*
        * Author: Longfei Yu
        * Last Edited By: Longfei Yu
        * Date Created: 1-30-2021
        * Description: Resume the game and unlock the animation lock in UnitManager. 
        * If there is any unit in moving state, and player restarts the level or 
        * reenters the game through main menu, the animationLock will still remains in 
        * true state which prevents player from selecting units. This function is designed
        * to solve the issue.
        */
    public void UnlockAnimationLock()
    {
        Resume();
        GameLogicManager.animationLock = false;
    }
}
}