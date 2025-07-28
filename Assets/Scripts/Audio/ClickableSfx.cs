using Audio;
using UnityEngine;
using Zenject;

namespace Audio
{
    [RequireComponent(typeof(Collider))]
    public class ClickableSfx : MonoBehaviour
    {
        [SerializeField] AudioClip clip;
        [SerializeField] float volume = 1f, pitch = 1f;

        [Inject] SignalBus _bus;

        void OnMouseDown()
        {
            if (!enabled || clip == null) return;
            _bus.Fire(new SfxSignal(clip, volume, pitch));
        }
    }
}