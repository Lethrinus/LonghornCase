using Core;
using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(Collider))]
    public class ClickableSfx : MonoBehaviour {
        [SerializeField] AudioClip clip;
        [SerializeField] float     volume = 1f;
        [SerializeField] float     pitch  = 1f;

        void OnMouseDown() {
            if (!enabled || clip == null) return;

            EventBus.Publish(new SfxEvent(clip, volume, pitch));
        }
    }
}


