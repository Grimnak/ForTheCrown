using UnityEngine;

namespace Capstone
{
public class CombatMusic : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AudioManager musicPlayer = FindObjectOfType<AudioManager>();
        if (musicPlayer != null)
        {
            FindObjectOfType<AudioManager>().StopAll();
            FindObjectOfType<AudioManager>().Play("LevelMusic");
        }
    }
}
}