using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Capstone
{
public class MainMenu : MonoBehaviour
{
    public string[] sceneName;

    public void StartLevel1()
    {
        SceneManager.LoadScene(sceneName[0]);
    }

    public void StartLevel2()
    {
        SceneManager.LoadScene(sceneName[1]);
    }

    public void QuitGame()
    {
        Application.Quit();
    }


}
}