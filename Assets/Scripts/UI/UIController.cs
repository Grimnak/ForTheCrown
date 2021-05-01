/* 
 * Author: Shengyu Jin
 * Last Edited By:  Will Bartlett
 * Date Created:  2-21-2021
 * Description:  This script controls the UI.
 * Filename:  UIController.cs
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

namespace Capstone
{
public class UIController : MonoBehaviour
{
    public static UIController instance;

    [Header("Levels To Load")]
    [SerializeField] private string newGameBtnLevel;    //Scene name for the first level
    [SerializeField] private Texture2D[] backgroundTexs;
    [SerializeField] private RawImage backgroundImg;
    [SerializeField] private string[] levelSceneNames; // this doesn't really serve a purpose right now.

    [SerializeField] private GameObject mainMenuButtonsCanvas;
    [SerializeField] private GameObject hostLobbyCanvas;
    [SerializeField] private GameObject playGameCanvas; //todo
    [SerializeField] private GameObject multiplayerCanvas; //todo
    [SerializeField] private GameObject endlessCanvas; //todo
    [SerializeField] private GameObject levelButtonsCanvas;
    [SerializeField] private GameObject levelNewGameDialog;
    [SerializeField] private GameObject endlessNewGameDialog;
    [SerializeField] private GameObject settingDialog;

    //Audio
    [SerializeField] private AudioManager audioManager;

    [SerializeField] private Text volumeText;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Text brightnessText;
    [SerializeField] private Slider brightnessSlider;

    private int menuNumber;
    private int _levelIdx;
    private int _trainingIdx;
    public int selectedLevelIdx { get => _levelIdx; set { _levelIdx = value; } }
    public int selectedTrainingIdx { get => _trainingIdx; set { _trainingIdx = value; } }
    private float volumeVal;
    private float brightnessVal;
    private Button startMPGame;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        menuNumber = 0;
        if (backgroundTexs.Length > 0)
            backgroundImg.texture = backgroundTexs[Random.Range(0, backgroundTexs.Length)];
        audioManager = FindObjectOfType<AudioManager>();
        if (audioManager)
        {
            audioManager.StopSound("LoadingBackground");
            audioManager.Play("MainMenuMusic");
        }
        volumeVal = 0.5f;   //default
        brightnessVal = 0.5f;   //default
        startMPGame = hostLobbyCanvas.transform.GetChild(1).gameObject.GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
                GoBackToMainMenu();
                ClickSound();
        }
        // if we are not the host, but we're in the host lobby, then do not allow this UI to start the game
        else if (hostLobbyCanvas.activeSelf)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                startMPGame.interactable = false; // child 1 is startGame button
            }
            else startMPGame.interactable = true;
        }
    }

    public void ClickSound()
    {
        GetComponent<AudioSource>().Play();
    }

    public void MouseClick(string buttonType)
    {
        if (buttonType == "PlayGame") // todo
        {
            mainMenuButtonsCanvas.SetActive(false);
            playGameCanvas.SetActive(true);
            menuNumber = 1;
        }
        if (buttonType == "LevelSelect")
        {
            playGameCanvas.SetActive(false);
            mainMenuButtonsCanvas.SetActive(false); //tmp -- remove later
            levelButtonsCanvas.SetActive(true);
        }
        if (buttonType == "Settings")
        {
            mainMenuButtonsCanvas.SetActive(false);
            settingDialog.SetActive(true);
        }
        if (buttonType == "Exit")
        {
            SceneManager.LoadScene(Global.exitScene);
        }
    }

    public void ClickLevelNewGameDialog(string btnType)
    {
        if (btnType == "Yes")
        {
            switch (selectedTrainingIdx)
            {
                case 0:
                    Global.activeGameMode = Global.ActiveGameMode.training1;
                    break;
                case 1:
                    Global.activeGameMode = Global.ActiveGameMode.training2;
                    break;
                case 2:
                    Global.activeGameMode = Global.ActiveGameMode.training3;
                    break;
                case 3:
                    Global.activeGameMode = Global.ActiveGameMode.training4;
                    break;
                default:
                    Global.activeGameMode = Global.ActiveGameMode.training1;
                    break;
            }
            Global.activeGameMap = selectedLevelIdx;
            SceneManager.LoadScene(levelSceneNames[0]); // TODO - SFX on click
        }

        if (btnType == "No")
        {
            levelNewGameDialog.SetActive(false);
            levelButtonsCanvas.SetActive(true);
        }
    }

    public void ClickEndlessNewGameDialog(string btnType)
    {
        if (btnType == "Yes")
        {
            switch (selectedLevelIdx)
            {
                case 1:
                    Global.activeGameMap = Global.ActiveGameMap.map1;
                    break;
                case 2:
                    Global.activeGameMap = Global.ActiveGameMap.map2;
                    break;
                case 3:
                    Global.activeGameMap = Global.ActiveGameMap.map3;
                    break;
                default:
                    Global.activeGameMap = Global.ActiveGameMap.map1;
                    break;
            }
            SceneManager.LoadScene(levelSceneNames[0]); // TODO - SFX on click
        }

        if (btnType == "No")
        {
            endlessNewGameDialog.SetActive(false);
            endlessCanvas.SetActive(true);
        }
    }

    public void StartLevel(int idx)
    {
        if (idx == -1)
            GoBackToMainMenu();
        else
        {
            selectedTrainingIdx = idx;
            selectedLevelIdx = 1;
            levelNewGameDialog.SetActive(true);
            levelButtonsCanvas.SetActive(false);
        }
    }

    public void StartEndlessLevel(int idx)
    {
        if (idx == -1)
            GoBackToMainMenu();
        else
        {
            selectedLevelIdx = idx;
            endlessNewGameDialog.SetActive(true);
            endlessCanvas.SetActive(false);
        }
    }

    private void GoBackToMainMenu()
    {
        Global.ResetGameState();
        settingDialog.SetActive(false);
        levelNewGameDialog.SetActive(false);
        endlessNewGameDialog.SetActive(false);
        playGameCanvas.SetActive(false);
        multiplayerCanvas.SetActive(false);
        endlessCanvas.SetActive(false);
        levelButtonsCanvas.SetActive(false);
        mainMenuButtonsCanvas.SetActive(true);
        menuNumber = 0;
    }

    public void VolumeSlider(float volume)
    {
        AudioListener.volume = volume;
        volumeText.text = volume.ToString("0.0");
    }

    public void BrightnessSlider(float brightness)
    {
        Screen.brightness = brightness;
        brightnessText.text = brightness.ToString("0.0");
    }

    public void ClickSettingsDialog(string btnType)
    {
        if (btnType == "Yes")
        {
            //To Do: save settings in user preference
            brightnessVal = Screen.brightness;
            volumeVal = AudioListener.volume;
        }
        if (btnType == "No")
        {
            Screen.brightness = brightnessVal;
            AudioListener.volume = volumeVal;
            brightnessSlider.value = brightnessVal;
            brightnessText.text = brightnessVal.ToString("0.0");
            volumeSlider.value = volumeVal;
            volumeText.text = volumeVal.ToString("0.0");
        }
        GoBackToMainMenu();
    }

    public void ChangeToWorldMapBg()
    {
        backgroundImg.texture = backgroundTexs[backgroundTexs.Length - 1];
    }
}
}