using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Capstone
{
public class MenuBackgroundMusic : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //LoadingBackground is longer than the time the scene is active so must be cut  off

        FindObjectOfType<AudioManager>().StopAll();
        FindObjectOfType<AudioManager>().Play("MainMenuMusic");
    }
}
}