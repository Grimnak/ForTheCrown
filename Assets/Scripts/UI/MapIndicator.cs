using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Capstone
{
public class MapIndicator : MonoBehaviour
{
    public static int mapIndex = 1;

    private void Start()
    {
        ObjectManager.DontDestroyOnLoad(this.gameObject);
    }
}
}