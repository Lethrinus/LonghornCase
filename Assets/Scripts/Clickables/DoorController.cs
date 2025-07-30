using DG.Tweening;
using Infrastructure.Signals;
using UnityEngine;
using Zenject;

namespace Clickables
{
    public class DoorController : MonoBehaviour
    {
        [Header("Door")]
        [SerializeField] Transform door;
        [SerializeField] float     openY    = 35f;
        [SerializeField] float     openDur  = .6f;
        [SerializeField] Ease      openEase = Ease.OutCubic;

        [Header("Blinking Dot")]
        [SerializeField] BlinkingDot dotPrefab;
        [SerializeField] Transform   dotAnchor;
        [SerializeField] float       dotDelay = .35f;

        BlinkingDot _dot;
        bool        _doorOpened;

        [Inject] SignalBus   _bus;
        [Inject] DiContainer _container;   // 🆕

        /* ---------------------------------------------------------------- */
        void OnEnable()  => _bus.Subscribe<TrashThrownSignal>(OnTrashThrown);
        void OnDisable() => _bus.Unsubscribe<TrashThrownSignal>(OnTrashThrown);

        /* ---------------------------------------------------------------- */
        void OnTrashThrown(TrashThrownSignal _)
        {
            if (_doorOpened) return;            
            _doorOpened = true;

            DOTween.Sequence()
                   .Append(door.DORotate(new Vector3(0, openY, 0),
                                         openDur, RotateMode.LocalAxisAdd)
                                 .SetEase(openEase))
                   .AppendInterval(dotDelay)
                   .AppendCallback(ShowDot);
        }

        void ShowDot()
        {
            if (_dot == null)
            {
               
                var go  = _container.InstantiatePrefab(dotPrefab,
                                  dotAnchor.position, dotAnchor.rotation, null);
                _dot = go.GetComponent<BlinkingDot>();
            }
            else
            {
                _dot.transform.SetPositionAndRotation(dotAnchor.position,
                                                       dotAnchor.rotation);
            }

            _dot.gameObject.SetActive(true);
        }
    }
}
