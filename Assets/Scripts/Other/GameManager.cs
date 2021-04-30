using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    bool gameHasEnded = false;
    public Camera mainCam, endCam;
    private bool camSwitch = false;

    public void EndGame()
    {
        if(!gameHasEnded)
        {
            gameHasEnded = true;
            camSwitch = !camSwitch;
            mainCam.enabled = false;
            endCam.enabled = true;
            //endCam.gameObject.SetActive(camSwitch);
            //GameObject.Find("Text").SetActive(true);
            Debug.Log("Game Over!");
            //Application.Quit();     //do this if we just want to quit immediately
        }
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Quitting from ExitScene");
            Application.Quit();
        }
    }
}
