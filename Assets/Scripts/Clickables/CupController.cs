
using UnityEngine;
using DG.Tweening;
using Core;
using Managers;

namespace Clickables {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    public class CupController : ClickableBase
    {
        public enum State { Idle, Hovering, AtDispenser, Delivered }

        [Header("Bounce")]
        [SerializeField] float bounceHeight   = .3f;
        [SerializeField] float bounceDuration = .4f;
        [Header("Bob")]
        [SerializeField] float bobRange       = .05f;
        [SerializeField] float bobDuration    = 1f;
        [Header("Dispense")]
        [SerializeField] Transform dispenserTarget;
        [SerializeField] float     moveDuration = 1f;
        [Header("Color")]
        [SerializeField] Color     deliveredColor = Color.blue;
        [Header("Deliver Bob")]
        [SerializeField] float     forwardOffset   = .1f;
        [SerializeField] float     forwardDuration = .3f;

        MeshRenderer _renderer;
        Vector3      _origPos;
        State        _state = State.Idle;
        Tween        _t1, _t2, _t3, _t4, _t5;

        void Awake()
        {
            _origPos  = transform.position;
            _renderer = GetComponent<MeshRenderer>();
        }

        public State CurrentState => _state;

       
        public override bool CanClickNow(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.ClickCup:
                    return _state == State.Idle;
                case GameState.ClickDispenser:
                    return _state == State.Hovering
                        || _state == State.AtDispenser;
                case GameState.ReturnCup:
                    return _state == State.Hovering
                        || _state == State.Delivered;
                default:
                    return false;
            }
        }

       
        protected override void OnValidClick()
        {
            var gs = GameManager.Instance.State;
            switch (gs)
            {
                case GameState.ClickCup:
                    StartBounce();
                    break;
                case GameState.ClickDispenser:
                    if (_state == State.Hovering)
                        Dispense();
                    else /* AtDispenser */
                    {
                        FillColor();
                        EventBus.Publish(new DispenserClickedEvent());
                    }
                    break;
                case GameState.ReturnCup:
                    if (_state == State.Hovering)
                        ReturnHome();
                    else /* Delivered */
                        StartDeliverBob();
                    break;
            }
        }

        void StartBounce()
        {
            _state = State.Hovering;
            KillTweens();
            _t1 = transform
                .DOMoveY(_origPos.y + bounceHeight, bounceDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _t2 = transform
                        .DOMoveY(_origPos.y + bounceHeight - bobRange, bobDuration)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                    EventBus.Publish(new CupClickedEvent());
                });
        }

        public void Dispense()
        {
            _state = State.AtDispenser;
            KillTweens();
            _t3 = transform
                .DOMove(dispenserTarget.position, moveDuration)
                .SetEase(Ease.InOutQuad);
        }

        public void FillColor()
        {
            _state = State.Delivered;
            KillTweens();
            _t4 = _renderer.material
                .DOColor(deliveredColor, .5f)
                .SetEase(Ease.InOutQuad);
        }

        void ReturnHome()
        {
            _state = State.Idle;
            KillTweens();
            transform
                .DOMove(_origPos, bounceDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => EventBus.Publish(new CupReturnedEvent()));
        }

        void StartDeliverBob()
        {
            _state = State.Hovering;
            KillTweens();
            Vector3 target = _origPos + Vector3.forward * forwardOffset;
            _t5 = transform
                .DOMove(target, forwardDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(StartBounce);
        }

        
        void KillTweens()
        {
            _t1?.Kill();
            _t2?.Kill();
            _t3?.Kill();
            _t4?.Kill();
            _t5?.Kill();
        }
    }
}
