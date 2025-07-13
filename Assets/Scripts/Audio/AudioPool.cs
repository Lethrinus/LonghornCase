
using UnityEngine;
using Audio;
using Core;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioPool : MonoBehaviour
{
    [SerializeField] int               poolSize = 10;
    [SerializeField] bool              spatial  = false;
    [SerializeField] AudioMixerGroup   sfxGroup;

    readonly List<AudioSource> pool = new();
    int nextIdx;

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake           = false;
            src.spatialBlend          = spatial ? 1f : 0f;
            src.outputAudioMixerGroup = sfxGroup;
            pool.Add(src);
        }
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()  => EventBus.Subscribe<SfxEvent>(HandleSfx);
    void OnDisable() => EventBus.Unsubscribe<SfxEvent>(HandleSfx);

    void HandleSfx(SfxEvent e)
    {
        if (e.Clip == null) return;       
        var src = pool[nextIdx];
        nextIdx = (nextIdx + 1) % pool.Count;
        src.Stop();    
        src.clip   = e.Clip;   
        src.pitch  = e.Pitch;          
        src.volume = e.Volume;
        src.Play();         
    }
}