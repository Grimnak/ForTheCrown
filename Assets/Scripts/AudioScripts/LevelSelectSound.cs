using UnityEngine.UI;
using UnityEngine;

namespace Capstone
{
public class LevelSelectSound : MonoBehaviour
{
    public Button btn;

    //This sound can be applied to any button by adding the script to the button and then dragging the button into the reference
    void Start()
    {
        btn.onClick.AddListener(ButtonSound);
    }

    void ButtonSound()
    {
        FindObjectOfType<AudioManager>().Play("LevelSelectNoise");
    }
}
}