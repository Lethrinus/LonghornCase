using UnityEngine;
using DG.Tweening;
using Core;
using Managers;

namespace Clickables
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider), typeof(MeshRenderer))]
    public class CupController : MonoBehaviour
    {
        public enum State { Idle, Hovering, AtDispenser, Delivered }

        [Header("Initial Bounce")]
        [SerializeField] float bounceHeight   = 0.3f;
        [SerializeField] float bounceDuration = 0.4f;

        [Header("Hover Bobbing")]
        [SerializeField] float bobRange    = 0.05f;
        [SerializeField] float bobDuration = 1f;

        [Header("Dispense Move")]
        [SerializeField] Transform dispenserTarget;
        [SerializeField] float     moveDuration = 1f;

        [Header("Final Color")]
        [SerializeField] Color     deliveredColor = Color.blue;

        [Header("Deliver → Forward Bob")]
        [SerializeField] float     deliverForwardOffset   = 0.1f;
        [SerializeField] float     deliverForwardDuration = 0.3f;

        State        _state = State.Idle;
        Vector3      _origPos;
        MeshRenderer _rend;
        Tween        _bounceTween, _bobTween, _moveTween, _colorTween, _deliverTween;

        void Awake()
        {
            _origPos = transform.position;
            _rend    = GetComponent<MeshRenderer>();
        }

        void OnMouseDown()
        {
            
            switch (_state)
            {
                case State.Idle:
                    
                    if (GameManager.Instance.State == GameState.ClickCup)
                        StartBounce();
                    else
                        InvalidClick();
                    break;

                case State.Hovering:
                    
                    if (GameManager.Instance.State == GameState.ReturnCup)
                        ReturnToOrigin();
                    else
                        InvalidClick();
                    break;

                case State.Delivered:
                   
                    if (GameManager.Instance.State == GameState.ReturnCup)
                        StartDeliverBob();
                    else
                        InvalidClick();
                    break;

              
            }
        }

        public State CurrentState => _state;

        
        public void Dispense()
        {
           
            if (GameManager.Instance.State != GameState.ClickDispenser ||
                _state != State.Hovering) return;

            _state = State.AtDispenser;
            KillAllTweens();
            _moveTween = transform
                .DOMove(dispenserTarget.position, moveDuration)
                .SetEase(Ease.InOutQuad);
        }

        

        private void StartBounce()
        {
            _state = State.Hovering;
            KillAllTweens();
            _bounceTween = transform
                .DOMoveY(_origPos.y + bounceHeight, bounceDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(StartBobbing);
            EventBus.Publish(new CupClickedEvent());
        }

        private void StartBobbing()
        {
            _bobTween = transform
                .DOMoveY(_origPos.y + bounceHeight - bobRange, bobDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void ReturnToOrigin()
        {
            _state = State.Idle;
            KillAllTweens();
            transform
                .DOMove(_origPos, bounceDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => EventBus.Publish(new CupReturnedEvent()));
        }

     
        public void FillColor()
        {
            if (_state != State.AtDispenser) return;
            _state = State.Delivered;
            KillAllTweens();
            _colorTween = _rend.material
                .DOColor(deliveredColor, 0.5f)
                .SetEase(Ease.InOutQuad);
        }

        private void StartDeliverBob()
        {
            _state = State.Hovering;
            KillAllTweens();
            Vector3 target = _origPos + Vector3.forward * deliverForwardOffset;
            _deliverTween = transform
                .DOMove(target, deliverForwardDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(StartBobbing);
        }

        private void InvalidClick()
        {
            transform
                .DOShakePosition(.2f, new Vector3(.02f, .02f, .02f), 8, 45)
                .SetEase(Ease.InOutSine);
        }

        private void KillAllTweens()
        {
            _bounceTween?.Kill();
            _bobTween?.Kill();
            _moveTween?.Kill();
            _colorTween?.Kill();
            _deliverTween?.Kill();
        }
    }
}
