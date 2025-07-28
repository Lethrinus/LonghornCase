using UnityEngine;

namespace Audio {
    public readonly struct SfxSignal {
        public readonly AudioClip Clip;
        public readonly float    Volume;
        public readonly float    Pitch;

        public SfxSignal(AudioClip clip, float volume = 1f, float pitch = 1f) {
            Clip   = clip;
            Volume = volume;
            Pitch  = pitch;
        }
    }
}


