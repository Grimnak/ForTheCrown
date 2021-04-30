using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Capstone
{
public class ObjectManager : MonoBehaviour
{
    public static List<GameObject> _ddolGOs {get; private set; } = new List<GameObject>();

    private static GameObject LP;

    public static void DontDestroyOnLoad(GameObject go)
    {
        MonoBehaviour.DontDestroyOnLoad(go);
        _ddolGOs.Add(go);
    }

    /// <summary>Destroy all DontDestroyOnLoad GameObjects</summary>
    /// <param name="ex">The DDOL GameObjects you don't want to destroy</param>
    public static void DestroyAll(List<GameObject> ex=null)
    {
        foreach (GameObject go in _ddolGOs)
        {
            if (ex != null && ex.Contains(go)) continue;
            else Destroy(go);
        }
    }

    public static void ClearDDOLs()
    {
        List<GameObject> keepMe = new List<GameObject>();
        var LP = GameObject.Find("LevelPersistence");
        keepMe.Add(LP);
        foreach (Transform child in LP.transform)
        {
            keepMe.Add(child.gameObject);
        }
        ObjectManager.DestroyAll(keepMe);
    }
}
}