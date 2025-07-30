// Assets/Scripts/Clickables/BlinkingDot.cs
using DG.Tweening;
using Infrastructure.Signals;
using Managers;
using UnityEngine;
using Zenject;

namespace Clickables
{
    [RequireComponent(typeof(Collider))]
    public class BlinkingDot : MonoBehaviour
    {
        [SerializeField] Vector3 scaleMin = Vector3.one * .6f;
        [SerializeField] Vector3 scaleMax = Vector3.one * 1.2f;
        [SerializeField] float   period   = 1.2f;
        [SerializeField] Ease    ease     = Ease.InOutSine;

        Tween _pulse;
        [Inject] SignalBus _bus;

        void OnEnable()
        {
            _pulse = transform.DOScale(scaleMax, period * .5f)
                .SetEase(ease)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        void OnDisable() => _pulse?.Kill();

        void OnMouseDown()
        {
            _pulse?.Kill();

            if (_bus != null)
                _bus.Fire<DotClickedSignal>();   // Injection geldiyse sinyal gönder
            else
                GameManager.Instance.GotoCompleted(); // çok olağan dışı durumda yedek

            gameObject.SetActive(false);
        }
    }
}