using DG.Tweening;
using Managers;
using UnityEngine;

namespace Clickables
{
    [RequireComponent(typeof(Collider))]
    public class BlinkingDot : MonoBehaviour
    {
        [Header("Pulse")]
        [SerializeField] private Vector3 scaleMin   = Vector3.one * 0.6f;
        [SerializeField] private Vector3 scaleMax   = Vector3.one * 1.2f;
        [SerializeField] private float   period     = 1.2f;          
        [SerializeField] private Ease    ease       = Ease.InOutSine;

        private Tween _pulse;

        private void OnEnable()
        {
      
            _pulse = transform
                .DOScale(scaleMax, period * .5f)
                .SetEase(ease)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        private void OnDisable() => _pulse?.Kill();

        void OnMouseDown()
        {
            _pulse?.Kill();
            GameManager.Instance.SetState(GameState.Completed);
            gameObject.SetActive(false);
        }
    }
}