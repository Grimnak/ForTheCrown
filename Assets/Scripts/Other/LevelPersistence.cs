using UnityEngine.SceneManagement;
using UnityEngine;

namespace Capstone
{
public class LevelPersistence : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        ObjectManager.DontDestroyOnLoad(gameObject);
        // DontDestroyOnLoad(gameObject);      //Persistent throughout scenes
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ScreenCapture.CaptureScreenshot("Captures/screenshot");
            FindObjectOfType<AudioManager>().Play("ScreenshotIndicator");
        }
    }
}
}