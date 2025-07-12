using UnityEngine;
using Core;
using System.Collections.Generic;
using Audio;
using UnityEngine.Audio;

public class AudioPool : MonoBehaviour
{
    [SerializeField] int  poolSize = 10;
    [SerializeField] bool spatial  = false;

    [Header("Routing")]
    [SerializeField] AudioMixerGroup sfxGroup;  

    readonly List<AudioSource> pool = new();
    int nextIdx;

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake            = false;
            src.spatialBlend           = spatial ? 1f : 0f;
            src.outputAudioMixerGroup  = sfxGroup;  
            pool.Add(src);
        }
        DontDestroyOnLoad(gameObject);
    }
    
}