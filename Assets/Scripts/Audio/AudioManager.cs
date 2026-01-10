using UnityEngine.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0, 1)]
    public float volume = 1;
    [Range(-3, 3)]
    public float pitch = 1;
    public bool loop = false;
    public bool playOnAwake = false;
    [HideInInspector]
    public AudioSource source;

    public Sound()
    {
        volume = 1;
        pitch = 1;
        loop = false;
    }
}

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public static AudioManager instance;

    // OPTIMIZATION: Dictionary for O(1) sound lookup instead of O(n) Array.Find
    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Build dictionary for fast lookup
        soundDictionary.Clear();
        
        foreach (Sound s in sounds)
        {
            if (s.source == null)
                s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;
            s.source.playOnAwake = s.playOnAwake;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            // Add to dictionary for fast lookup
            if (s != null && !string.IsNullOrEmpty(s.name))
            {
                soundDictionary[s.name] = s;
            }

            if (s.playOnAwake)
                s.source.Play();
        }
    }

    public void Play(string name)
    {
        // OPTIMIZATION: O(1) dictionary lookup instead of O(n) Array.Find
        if (soundDictionary.TryGetValue(name, out Sound s))
        {
            s.source.Play();
        }
        else
        {
            Debug.LogWarning("Sound: " + name + " not found");
        }
    }

    public void Stop(string name)
    {
        // OPTIMIZATION: O(1) dictionary lookup instead of O(n) Array.Find
        if (soundDictionary.TryGetValue(name, out Sound s))
        {
            s.source.Stop();
        }
    }
}
