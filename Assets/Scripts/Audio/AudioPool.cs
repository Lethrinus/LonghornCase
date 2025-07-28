using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace Audio
{
    public class AudioPool : MonoBehaviour
    {
        [SerializeField] int             poolSize = 10;
        [SerializeField] bool            spatial  = false;
        [SerializeField] AudioMixerGroup sfxGroup;

        readonly List<AudioSource> _pool = new();
        int _nextIdx;

        [Inject] void Construct(SignalBus bus) 
        {
            bus.Subscribe<SfxSignal>(HandleSignal);
        }

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

        public void HandleSignal(SfxSignal sig)
        {
            if (sig.Clip == null) return;
            var src = _pool[_nextIdx];
            _nextIdx = (_nextIdx + 1) % _pool.Count;
            if (src.isPlaying) src.Stop();
            src.clip   = sig.Clip;
            src.volume = sig.Volume;
            src.pitch  = sig.Pitch;
            src.Play();
        }
    }
}