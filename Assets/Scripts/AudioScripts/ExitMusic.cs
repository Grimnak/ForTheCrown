using UnityEngine;

namespace Capstone
{
public class ExitMusic : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<AudioManager>().StopAll();
        FindObjectOfType<AudioManager>().Play("ExitScreenMusic");
    }
}
}