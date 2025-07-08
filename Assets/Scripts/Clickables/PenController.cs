using UnityEngine;
using DG.Tweening;

namespace Clickables
{
    [DisallowMultipleComponent]
    public class PenController : MonoBehaviour
    {
        enum State { Idle, Lifting, Hovering, Writing, Returning }

        [Header("Path & References")]
        [SerializeField] private Transform writePathParent;
        
        [SerializeField] private Transform penTip;

        [Header("Hover Settings")]
        [SerializeField] private float liftHeight    = 0.5f;
        [SerializeField] private float liftDuration  = 0.5f;
        [SerializeField] private float angleRangeY   = 15f;
        [SerializeField] private float angleRangeZ   = 10f;
        [SerializeField] private float angularSpeed  = 1f;
        [SerializeField] private float fadeDuration  = 0.5f;

        [Header("Write Settings")]
        [SerializeField] private float writeDuration = 2f;
        [SerializeField] private float writeWobbleY  = 3f;
        [SerializeField] private float writeWobbleZ  = 3f;
        [SerializeField] private float writeWobbleDur= 0.3f;

        State      _state = State.Idle;
        Vector3    _origPos;
        Quaternion _origRot;
        float      _angle, _amplitude;
        Vector3[]  _waypoints;
        Tween      _liftTween, _fadeTween, _writeTween, _writeWobbleTween, _returnTween;

        void Awake()
        {
            _origPos = transform.position;
            _origRot = transform.localRotation;

            var count = writePathParent.childCount;
            _waypoints = new Vector3[count];
            for (int i = 0; i < count; i++)
                _waypoints[i] = writePathParent.GetChild(i).position;
        }

        void OnMouseDown()
        {
            // when clicking pen 
            if (_state == State.Idle)
            {
                StartHover();
            }
            else if (_state == State.Hovering)
            {
                // immediately return back 
                StartReturn();
            }
        }

        void Update()
        {
            if (_state != State.Hovering) return;

            // hover wobbling
            _angle += angularSpeed * Time.deltaTime;
            if (_angle > 2 * Mathf.PI) _angle -= 2 * Mathf.PI;
            var yTilt = Mathf.Sin(_angle) * angleRangeY * _amplitude;
            var zTilt = Mathf.Cos(_angle) * angleRangeZ * _amplitude;
            transform.localRotation = _origRot * Quaternion.Euler(0f, yTilt, zTilt);
        }

       // write trigger func
        public void TriggerWrite()
        {
            if (_state == State.Hovering || _state == State.Lifting)
                StartWrite();
        }

        void StartHover()
        {
            _state = State.Lifting;
            _angle = _amplitude = 0f;
            KillAllTweens();

            _liftTween = transform
                .DOMoveY(_origPos.y + liftHeight, liftDuration)
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    _state = State.Hovering;
                    // amplitude fade-in
                    _fadeTween = DOTween
                        .To(() => _amplitude, x => _amplitude = x, 1f, fadeDuration)
                        .SetEase(Ease.InOutQuad);
                });
        }

        void StartWrite()
        {
            _state = State.Writing;
            KillAllTweens();

            // pen moves along path
            _writeTween = transform
                .DOPath(
                  _waypoints,
                  writeDuration,
                  PathType.CatmullRom,
                  PathMode.Full3D,
                  resolution: 30,
                  gizmoColor: Color.clear
                )
                .SetEase(Ease.InOutQuint)   // speed curve
                .SetSpeedBased()            // stable speed
                .SetLookAt(0.01f)           // pentip to forward
                .OnWaypointChange(idx =>
                {
                    if (_writeWobbleTween == null && idx > 0)
                    {
                        // ❸ looser wobble
                        _writeWobbleTween = transform
                            .DOLocalRotate(
                              new Vector3(
                                  Random.Range(-writeWobbleY, writeWobbleY),
                                  0f,
                                  Random.Range(-writeWobbleZ, writeWobbleZ)
                              ),
                              writeWobbleDur
                            )
                            .SetEase(Ease.InOutSine)
                            .SetLoops(-1, LoopType.Yoyo);
                    }
                })
                .OnComplete(StartReturn);
        }

        void StartReturn()
        {
            _state = State.Returning;
            KillAllTweens();

            // position + rotation tweens simultaneously
            _returnTween = DOTween.Sequence()
                .Append(transform
                    .DOMove(_origPos, liftDuration)
                    .SetEase(Ease.OutBack))
                .Join(transform
                    .DORotateQuaternion(_origRot, liftDuration)
                    .SetEase(Ease.OutBack))
                .OnComplete(() =>
                {
                    _state = State.Idle;
                    transform.localRotation = _origRot;
                });
        }

        void KillAllTweens()
        {
            _liftTween?.Kill();
            _fadeTween?.Kill();
            _writeTween?.Kill();
            _writeWobbleTween?.Kill();
            _returnTween?.Kill();
        }
    }
}
