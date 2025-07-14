using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    public class AudioPool : MonoBehaviour
    {
        [SerializeField] int               poolSize = 10;
        [SerializeField] bool              spatial  = false;
        [SerializeField] AudioMixerGroup   sfxGroup;

        readonly List<AudioSource> _pool = new();
        int _nextIdx;

        void Awake()
        {
            for (int i = 0; i < poolSize; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake           = false;
                src.spatialBlend          = spatial ? 1f : 0f;
                src.outputAudioMixerGroup = sfxGroup;
                _pool.Add(src);
            }
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()  => EventBus.Subscribe<SfxEvent>(HandleSfx);
        void OnDisable() => EventBus.Unsubscribe<SfxEvent>(HandleSfx);

        void HandleSfx(SfxEvent e)
        {
            if (e.Clip == null) return;
    
            var src = _pool[_nextIdx];
            _nextIdx = (_nextIdx + 1) % _pool.Count;
        
            if (src.isPlaying) src.Stop();
        
            src.clip = e.Clip;
            src.pitch = e.Pitch;
            src.volume = e.Volume;
            src.Play();
        }
    }
}