// Assets/Scripts/Infrastructure/Signals.cs
using UnityEngine;

namespace Infrastructure.Signals
{
    /* ---------- audio --------------------------------------------------- */
    public readonly struct PlaySfxSignal
    {
        public readonly AudioClip Clip;
        public readonly float    Volume;
        public readonly float    Pitch;

        public PlaySfxSignal(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            Clip   = clip;
            Volume = volume;
            Pitch  = pitch;
        }
    }

    /* ---------- cup ----------------------------------------------------- */
    public struct CupClickedSignal          {}
    public struct CupHoverCanceledSignal    {}
    public struct CupFilledSignal           {}
    public struct CupStartedFloatingSignal  {}
    public struct PlantClickedSignal        {}
    public struct TrashThrownSignal         {}

    /* ---------- pen / board -------------------------------------------- */
    public struct PenClickedSignal          {}
    public struct PenHoverCanceledSignal    {}
    public struct BoardDrawnSignal          {}
    
    public struct DotClickedSignal          {} 
}