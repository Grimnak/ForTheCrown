using UnityEngine.Audio;
using UnityEngine;

namespace Capstone
{
[System.Serializable]
public class Sounds
{
    //All sounds can be added on the sounds drop down menu on the AudioManager gameobject in the Loading scene

    public string name;     //Important as this is how each clip is referenced
    public AudioClip clip;  
    [Range(0f, 1f)]
    public float volume;
    [HideInInspector]       //This is set at runtime so no need to show in inspector
    public AudioSource source;
    public bool loop;
}
}