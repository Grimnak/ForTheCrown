/* 
 * Author: Will Bartlett
 * Last Edited By: Will Bartlett
 * Date Created: 4-7-2021
 * Description: Code for manipulating the results screen.
 * Filename: ResultsPanelScript.cs
 */

using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Capstone;
using Photon.Pun;

public class ResultsPanelScript : MonoBehaviour
{
    [SerializeField] private Text Wavetxt;
    [SerializeField] private Text Enemiestxt;
    [SerializeField] private Text Turnstxt;
    [SerializeField] private Text Alliestxt;
    [SerializeField] private Text Attackstxt;
    [SerializeField] private Text TryAgaintxt;
    
    public GameObject resultsScreenUI;

    // Start is called before the first frame update
    void Start()
    {
        resultsScreenUI.SetActive(false);               //so we don't see if throughout the GameScene until it's needed
        Wavetxt.text = "Number of waves completed = ";
        Enemiestxt.text = "Number of enemies killed = ";
        Turnstxt.text = "Number of turns completed = ";
        Alliestxt.text = "Number of units lost = ";
        Attackstxt.text = "Number of attacks used = ";
    }

    private void Awake()
    {
       // resultsScreenUI.SetActive(false);
        //Wavetxt.text = "Number of waves completed: ";
        //Enemiestxt.text = "Number of enemies killed: ";
    }

    // Update is called once per frame
    void Update()
    {
        //Wavetxt.text = "Number of waves completed: ";
    }

    public void ShowEndlessPanel()
    {
        int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
        PlayerController pc = GameLogicManager.Instance.controllers[id] as PlayerController;
        Debug.Log("We made it to ShowEndlessPanel().");
        resultsScreenUI.SetActive(true);
        Wavetxt.text = "Waves Completed - " + GameLogicManager.Instance.endlessWaveClearCount;
        Enemiestxt.text = "Enemies Killed - " + pc.enemiesKilledByPlayer;
        Turnstxt.text = "Turns Finished - " + (GameLogicManager.Instance.GetTurnCounter() - 1);
        Alliestxt.text = "Allied Units Lost - " + pc.unitsLostByPlayer;
        Attackstxt.text = "Attacks Delivered - " + pc.AttacksThrown;
        TryAgaintxt.text = "Return to main menu.";
        Time.timeScale = 0f;
        PauseMenu.GameIsPaused = true;
        LevelGenerator.map.SetActive(false);
        Camera.main.gameObject.GetComponent<CinemachineBrain>().enabled = false;
        //Destroy(GameObject.Find("UICanvas"));
    }

    public void ShowTrainingLossPanel()
    {
        int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
        PlayerController pc = GameLogicManager.Instance.controllers[id] as PlayerController;
        Debug.Log("We made it to ShowTrainingLossPanel().");
        resultsScreenUI.SetActive(true);
        Wavetxt.text = "You have been defeated.";
        Enemiestxt.text = "Enemies Killed - " + pc.enemiesKilledByPlayer;
        Turnstxt.text = "Turns Finished - " + (GameLogicManager.Instance.GetTurnCounter() - 1);
        Alliestxt.text = "Allied Units Lost - " + pc.unitsLostByPlayer;
        Attackstxt.text = "Attacks Delivered - " + pc.AttacksThrown;
        TryAgaintxt.text = "Return to main menu.";
        Time.timeScale = 0f;
        PauseMenu.GameIsPaused = true;
        LevelGenerator.map.SetActive(false);
        Camera.main.gameObject.GetComponent<CinemachineBrain>().enabled = false;
        //Destroy(GameObject.Find("UICanvas"));
    }

    public void ShowTrainingWinPanel()
    {
        int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
        PlayerController pc = GameLogicManager.Instance.controllers[id] as PlayerController;
        Debug.Log("We made it to ShowTrainingWinPanel().");
        resultsScreenUI.SetActive(true);
        Wavetxt.text = "You are victorious.";
        Enemiestxt.text = "Enemies Killed - " + pc.enemiesKilledByPlayer;
        Turnstxt.text = "Turns Finished - " + (GameLogicManager.Instance.GetTurnCounter() - 1);
        Alliestxt.text = "Allied Units Lost - " + pc.unitsLostByPlayer;
        Attackstxt.text = "Attacks Delivered - " + pc.AttacksThrown;
        TryAgaintxt.text = "Return to main menu.";
        Time.timeScale = 0f;
        PauseMenu.GameIsPaused = true;
        LevelGenerator.map.SetActive(false);
        Camera.main.gameObject.GetComponent<CinemachineBrain>().enabled = false;
        //Destroy(GameObject.Find("UICanvas"));
    }

    public void ShowMPWinPanel()
    {
        int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
        PlayerController pc = GameLogicManager.Instance.controllers[id] as PlayerController;
        Debug.Log("We made it to ShowMPWinPanel().");
        resultsScreenUI.SetActive(true);
        Wavetxt.text = pc.endResult;
        Enemiestxt.text = "Enemies Killed - " + pc.enemiesKilledByPlayer;
        Turnstxt.text = "Turns Finished - " + (GameLogicManager.Instance.GetTurnCounter() - 1);
        Alliestxt.text = "Allied Units Lost - " + pc.unitsLostByPlayer;
        Attackstxt.text = "Attacks Delivered - " + pc.AttacksThrown;
        TryAgaintxt.text = "Return to main menu.";
        Time.timeScale = 0f;
        PauseMenu.GameIsPaused = true;
        LevelGenerator.map.SetActive(false);
        Camera.main.gameObject.GetComponent<CinemachineBrain>().enabled = false;
        //Destroy(GameObject.Find("UICanvas"));
    }

    public void ShowMPLosePanel()
    {
        int id = PhotonNetwork.IsConnected ? PhotonNetwork.IsMasterClient ? 0 : 1 : 0;
        PlayerController pc = GameLogicManager.Instance.controllers[id] as PlayerController;
        Debug.Log("We made it to ShowMPWinPanel().");
        resultsScreenUI.SetActive(true);
        Wavetxt.text = pc.endResult;
        Enemiestxt.text = "Enemies Killed - " + pc.enemiesKilledByPlayer;
        Turnstxt.text = "Turns Finished - " + (GameLogicManager.Instance.GetTurnCounter() - 1);
        Alliestxt.text = "Allied Units Lost - " + pc.unitsLostByPlayer;
        Attackstxt.text = "Attacks Delivered - " + pc.AttacksThrown;
        TryAgaintxt.text = "Return to main menu.";
        Time.timeScale = 0f;
        PauseMenu.GameIsPaused = true;
        LevelGenerator.map.SetActive(false);
        Camera.main.gameObject.GetComponent<CinemachineBrain>().enabled = false;
        //Destroy(GameObject.Find("UICanvas"));
    }

    public void HidePanel()
    {
        resultsScreenUI.SetActive(false);
    }
}
