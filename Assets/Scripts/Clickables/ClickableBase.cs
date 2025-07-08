
using UnityEngine;
using DG.Tweening;
using Managers;

namespace Clickables {
    [DisallowMultipleComponent]
    public abstract class ClickableBase : MonoBehaviour, IClickable
    {
        [Header("Invalid Click Shake")]
        [SerializeField] Vector3 shakeStrength   = new Vector3(0.02f, 0.02f, 0.02f);
        [SerializeField] float   shakeDuration   = 0.2f;
        [SerializeField] int     shakeVibrato    = 8;
        [SerializeField] float   shakeRandomness = 45f;
        [SerializeField] public Ease shakeEase = DG.Tweening.Ease.InOutSine;

        
        public abstract bool CanClickNow(GameState state);

   
        protected abstract void OnValidClick();

        void OnMouseDown()
        {
            var state = GameManager.Instance.State;
            if (CanClickNow(state))
            {
                OnValidClick();
            }
            else
            {
                
                transform
                    .DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness)
                    .SetEase(shakeEase);
            }
        }
    }
}