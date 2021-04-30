using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Capstone
{
public class QuitManagement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
            if (SceneManager.GetActiveScene().Equals(Global.exitScene) && Input.GetKeyDown(KeyCode.Alpha7))
            {
                Debug.Log("Quitting application");
                Application.Quit();
            }
    }
}
}