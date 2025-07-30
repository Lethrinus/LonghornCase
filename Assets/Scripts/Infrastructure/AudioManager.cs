
using Infrastructure.Signals;
using UnityEngine;
using Zenject;

namespace Infrastructure.Audio
{
    public sealed class AudioManager : IInitializable, ILateDisposable
    {
        readonly SignalBus   _bus;
        readonly AudioSource _template;     

        public AudioManager(SignalBus bus, AudioSource template)
        {
            _bus      = bus;
            _template = template;
        }

        public void Initialize()  => _bus.Subscribe<PlaySfxSignal>(PlayOneShot);
        public void LateDispose() => _bus.Unsubscribe<PlaySfxSignal>(PlayOneShot);

        
        void PlayOneShot(PlaySfxSignal s)
        {
            if (s.Clip == null) return;

            var go  = new GameObject("SFX‑OneShot");
            var src = go.AddComponent<AudioSource>();

            
            src.outputAudioMixerGroup = _template.outputAudioMixerGroup;
            src.spatialBlend          = _template.spatialBlend;
            src.rolloffMode           = _template.rolloffMode;

            src.clip   = s.Clip;
            src.volume = s.Volume;
            src.pitch  = s.Pitch;
            src.Play();

            Object.Destroy(go, s.Clip.length / Mathf.Abs(s.Pitch));
        }
    }
}