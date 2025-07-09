using UnityEngine;
using DG.Tweening;
using Core;
using Managers;

namespace Clickables {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    public class CupController : ClickableBase
    {
        public enum State { Idle, Hovering, AtDispenser, Delivered, AtPlant, Thrown }

        [Header("Bounce/Bob")]
        [SerializeField] float bounceHeight    = .3f;
        [SerializeField] float bounceDuration  = .4f;
        [SerializeField] float bobRange        = .05f;
        [SerializeField] float bobDuration     = 1f;

        [Header("Move Targets")]
        [SerializeField] Transform dispenserTarget;
        [SerializeField] Transform plantTarget;
        [SerializeField] Transform trashTarget;
        [SerializeField] float     moveDuration       = 1f;

        [Header("Color")]
        [SerializeField] Color     deliveredColor     = Color.blue;

        Vector3      _origPos;
        MeshRenderer _renderer;
        State        _state = State.Idle;
        Tween        _tBounce, _tBob, _tDispense, _tColor, _tPlant, _tTrash;

        void Awake()
        {
            _origPos  = transform.position;
            _renderer = GetComponent<MeshRenderer>();
        }

        /// <summary>DispenserClickable vs. diğerlerinin baktığı state</summary>
        public State CurrentState => _state;

        public override bool CanClickNow(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.ClickCup:
                    return _state == State.Idle;
                case GameState.ClickDispenser:
                    return _state == State.Hovering;
                case GameState.ClickPlant:
                    return _state == State.Delivered;
                case GameState.ThrowTrash:
                    return _state == State.AtPlant;
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
                    EventBus.Publish(new CupClickedEvent());
                    break;

                case GameState.ClickDispenser:
                    Dispense();
                    break;

                case GameState.ClickPlant:
                    MoveToPlant();
                    break;

                case GameState.ThrowTrash:
                    ThrowToTrash();
                    break;
            }
        }

        void StartBounce()
        {
            _state = State.Hovering;
            KillAll();
            _tBounce = transform
                .DOMoveY(_origPos.y + bounceHeight, bounceDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _tBob = transform
                        .DOMoveY(_origPos.y + bounceHeight - bobRange, bobDuration)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                });
        }

        /// <summary>1. dispenser tıklaması: sadece hareket</summary>
        public void Dispense()
        {
            _state = State.AtDispenser;
            KillAll();
            _tDispense = transform
                .DOMove(dispenserTarget.position, moveDuration)
                .SetEase(Ease.InOutQuad);
        }

        
        public void FillColor()
        {
            _state = State.Delivered;
            KillAll();
            _tColor = _renderer.material
                .DOColor(deliveredColor, .5f)
                .SetEase(Ease.InOutQuad)
                .OnComplete(()=> EventBus.Publish(new CupFilledEvent()));
        }

        void MoveToPlant()
        {
            _state = State.AtPlant;
            KillAll();
            _tPlant = transform
                .DOMove(plantTarget.position, moveDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => EventBus.Publish(new PlantClickedEvent()));
        }

        public void ThrowToTrash()
        {
            _state = State.Thrown;
            KillAll();
            _tTrash = transform
                .DOMove(trashTarget.position, moveDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => EventBus.Publish(new TrashThrownEvent()));
        }

        void KillAll()
        {
            _tBounce?.Kill();
            _tBob?.Kill();
            _tDispense?.Kill();
            _tColor?.Kill();
            _tPlant?.Kill();
            _tTrash?.Kill();
        }
    }
}