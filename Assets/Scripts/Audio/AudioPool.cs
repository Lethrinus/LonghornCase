using UnityEngine;
using Core;
using System.Collections.Generic;
using Audio;

public class AudioPool : MonoBehaviour {
    [SerializeField] int  poolSize = 10;
    [SerializeField] bool spatial  = false;

    readonly List<AudioSource> pool = new();
    int nextIdx;

    void Awake() {
        for (int i = 0; i < poolSize; i++) {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake  = false;
            src.spatialBlend = spatial ? 1f : 0f;
            pool.Add(src);
        }
        DontDestroyOnLoad(gameObject);          // keep across scenes
    }

    void OnEnable()  => EventBus.Subscribe<SfxEvent>(Play);
    void OnDisable() => EventBus.Unsubscribe<SfxEvent>(Play);

    void Play(SfxEvent e) {
        if (!e.Clip) return;
        var src = pool[nextIdx];
        nextIdx = (nextIdx + 1) % pool.Count;

        src.clip   = e.Clip;
        src.volume = e.Volume;
        src.pitch  = e.Pitch;
        src.Play();
    }
}