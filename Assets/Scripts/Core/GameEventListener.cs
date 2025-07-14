// Assets/Scripts/Events/GameEventListener.cs

using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    [AddComponentMenu("Events/GameEvent Listener")]
    public class GameEventListener : MonoBehaviour {
        [SerializeField] GameEvent EventAsset;
        [SerializeField] UnityEvent Response;

        void OnEnable() {
            EventAsset.RegisterListener(OnEventRaised);
        }

        void OnDisable() {
            EventAsset.UnregisterListener(OnEventRaised);
        }

        void OnEventRaised() {
            Response.Invoke();
        }
    }
}