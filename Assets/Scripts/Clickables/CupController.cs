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
        
        [Header("Pour Path")]
        [SerializeField] Transform pourPathParent;   
        [SerializeField] float pourDuration = 1f;
        
        Quaternion _origRot; 
        Vector3      _origPos;
        MeshRenderer _renderer;
        State        _state = State.Idle;
        Tween        _tBounce, _tBob, _tDispense, _tColor, _tPlant, _tTrash;
        Vector3[] _pourWps;
        Color     _origColor;
        Tween _tCancel;
        
        void Awake()
        {
            _origPos   = transform.position;
            _renderer  = GetComponent<MeshRenderer>();
            _origRot  = transform.localRotation; 
            _origColor = _renderer.material.color;

         
            int pc = pourPathParent.childCount;
            _pourWps = new Vector3[pc];
            for (int i = 0; i < pc; i++)
                _pourWps[i] = pourPathParent.GetChild(i).position;
        }

      
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
                    if (_state == State.Hovering)
                        ReturnHome();
                    else
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
                .OnComplete(() => {
                    EventBus.Publish(new CupFilledEvent());
                    StartPour();     
                });
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
        void StartPour()
        {
            
            transform
                .DOPath(_pourWps, pourDuration, PathType.CatmullRom, PathMode.Full3D)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                   
                    _renderer.material.color = _origColor;
                    transform.position       = _origPos; 
                    _state = State.Idle;  
                });
        }
        void ReturnHome()
        {
           
            _state = State.Idle;
            KillAll();
            transform.DOKill();
            _renderer.material.DOKill();

           
            DOTween.Sequence()
                .Append(transform.DOMove(_origPos, bounceDuration)     
                    .SetEase(Ease.InOutQuad))
                .Join(transform.DORotateQuaternion(_origRot, bounceDuration)
                    .SetEase(Ease.InOutQuad))
                .OnComplete(() => {
                    EventBus.Publish(new CupHoverCancelledEvent());
                });
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