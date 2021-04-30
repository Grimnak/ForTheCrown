using UnityEngine.Audio;
using UnityEngine;
using System;

namespace Capstone
{
public class AudioManager : MonoBehaviour
{
    //Using a singleton implemetation to control how how often sounds play
    public static AudioManager instance;

    public Sounds[] sounds;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else                               //Only one instance of a sound played at a time, helpful for scene switching
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sounds s in sounds)        //Creates an audio source for all audio clips being used
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
        }
    }

    //For both play and stop the string 'name' must match the string entered in the AudioManager section on the Loading scene

    public void Play(string name)       //Searches for given audio source and plays if it exists
    {
        Sounds s = Array.Find(sounds, sounds => sounds.name == name);
        if (s == null)
        {
            return;
        }
        s.source.Play();
    }

    public void StopSound(string name)  //Searches for given audio source and stops playing
    {
        Sounds s = Array.Find(sounds, sounds => sounds.name == name);
        if (s == null)
        {
            return;
        }
        s.source.Stop();
    }

    public void StopAll()   //Stops all sounds being played
    {
        foreach (Sounds s in sounds)
        {
            s.source.Stop();
        }
    }
}
}