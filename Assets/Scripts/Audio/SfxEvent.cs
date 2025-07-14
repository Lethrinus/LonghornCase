using UnityEngine;

namespace Audio {
    public readonly struct SfxEvent {
        public readonly AudioClip Clip;
        public readonly float    Volume;
        public readonly float    Pitch;

        public SfxEvent(AudioClip clip, float volume = 1f, float pitch = 1f) {
            Clip   = clip;
            Volume = volume;
            Pitch  = pitch;
        }
    }
}


