using System;
using UnityEngine;
using DG.Tweening;
using Core;
using Audio;
using Managers;
using Zenject;

namespace Clickables {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class CupController : ClickableBase
    {

        [Inject] private SignalBus _bus;
        
        public enum State {
            Idle, Hovering, AtDispenser, Delivered, Floating, AtPlant, Thrown
        }
        
        [SerializeField] private GameEvent cupClickedEvent;
        [SerializeField] private GameEvent cupHoverCancelledEvent;
        [SerializeField] private GameEvent cupFilledEvent;
        [SerializeField] private GameEvent plantClickedEvent;
        [SerializeField] private GameEvent trashThrownEvent;
      
        [Header("Audio")]
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private float     clickVolume = 1f;
        [SerializeField] private float     clickPitch  = 1f;

        [Header("Hover & Bob")]
        [SerializeField] private float hoverHeight = .3f, hoverDur = .4f;
        [SerializeField] private float bobRange    = .05f, bobDur   = 1f;

        [Header("Move Targets")]
        [SerializeField] private Transform dispenserTarget, plantTarget, trashTarget;
        [SerializeField] private float     moveDur = 1f;

        [Header("Fill / Float")]
        [SerializeField] private Color filledColor    = Color.cyan;
        [SerializeField] private float floatFwd       = .15f, floatUp = .08f;

        [Header("Pour Path")]
        [SerializeField] private Transform pourPathParent;
        [SerializeField] private float     pourDur = 1f, waitAfterPour = .25f, fadeBackDur = .5f;

        private Vector3      _origPos;
        private  Quaternion   _origRot;
        private  MeshRenderer _mr;
        private   Color        _origCol;
        private   Vector3[]    _pourWps;
        private   Tween        _t0, _t1, _liftTween, _bobTween, _followTween;
        private  ParticleSystem _activeStream;

        public static event Action OnCupDisposed;

        public State CurrentState { get; private set; } = State.Idle;

        private void Awake() {
            _origPos = transform.position;
            _origRot = transform.localRotation;
            _mr      = GetComponent<MeshRenderer>();
            _origCol = _mr.material.color;

            var c = pourPathParent.childCount;
            _pourWps = new Vector3[c];
            for (var i = 0; i < c; ++i)
                _pourWps[i] = pourPathParent.GetChild(i).position;
        }

        public override bool CanClickNow(GameState gs) =>
            (gs == GameState.ClickCup && (CurrentState == State.Idle || CurrentState == State.Hovering)) ||
            (gs == GameState.ClickDispenser && CurrentState == State.Hovering) ||
            (gs == GameState.ClickPlant && (CurrentState == State.Delivered || CurrentState == State.Floating));
          

        protected override void OnValidClick() {
            if (clickClip != null) 
                _bus.Fire(new SfxSignal(clickClip, clickVolume, clickPitch));

            var gs = GameManager.Instance.State;
            switch (gs) {
                case GameState.ClickCup:
                case GameState.ClickDispenser:
                    if (CurrentState == State.Idle) { 
                        if (clickClip != null) 
                            _bus.Fire(new SfxSignal(clickClip, clickVolume, clickPitch));

                        StartHover();
                    }
                    else if (CurrentState == State.Hovering) {
                      
                        ReturnHome();
                    }
                    break;

                case GameState.ClickPlant:
                    if      (CurrentState == State.Delivered) StartFloat();
                    else if (CurrentState == State.Floating)  ReturnToDispenser();
                    break;

               
            }
        }

        private void StartHover() {
            CurrentState = State.Hovering;

            
            DOTween.Kill(transform);
            Kill();

         
            _liftTween = transform
                .DOMoveY(_origPos.y + hoverHeight, hoverDur)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    cupClickedEvent.Raise(); 
                    _bobTween = transform
                        .DOMoveY(_origPos.y + hoverHeight - bobRange, bobDur)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                });
        }

        private void ReturnHome() {
            CurrentState = State.Idle;

            DOTween.Kill(transform);
            Kill();

            DOTween.Sequence()
                .Append(transform.DOMove(_origPos, hoverDur).SetEase(Ease.InOutQuad))
                .Join(transform.DORotateQuaternion(_origRot, hoverDur).SetEase(Ease.InOutQuad));
                    cupHoverCancelledEvent.Raise(); 
                }
        

        public void MoveToDispenser() {
            CurrentState = State.AtDispenser;
            Kill();
            _t0 = transform.DOMove(dispenserTarget.position, moveDur)
                           .SetEase(Ease.InOutQuad);
        }

        public void FillWater() {
            CurrentState = State.Delivered;
            Kill();

            _followTween = DOTween.To(() => 0f, _ => { },
                0f, .4f
            ).SetEase(Ease.Linear);

            _t0 = _mr.material.DOColor(filledColor, .4f)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>  cupFilledEvent.Raise());
        }

        private void ReturnToDispenser() {
            CurrentState = State.Delivered;
            Kill();
            _t0 = transform.DOMove(dispenserTarget.position, hoverDur)
                           .SetEase(Ease.InOutQuad);
        }

        private  void StartFloat() {
            CurrentState = State.Floating;
            Kill();

            Vector3 tgt = dispenserTarget.position
                        + dispenserTarget.forward * floatFwd
                        + Vector3.up              * floatUp;

            _t0 = transform.DOMove(tgt, hoverDur).SetEase(Ease.OutQuad)
                           .OnComplete(() => {
                               _t1 = transform.DOMoveY(tgt.y - bobRange, bobDur)
                                              .SetEase(Ease.InOutSine)
                                              .SetLoops(-1, LoopType.Yoyo);
                           });
        }

        public void StartPour() {
            CurrentState = State.AtPlant;
            Kill();

            int  n   = _pourWps.Length;
            var wps = new Vector3[n + 1];
            wps[0] = transform.position;
            Array.Copy(_pourWps, 0, wps, 1, n);

            Vector3 endEuler = _origRot.eulerAngles;
            endEuler.x = 110f;

            var seq = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            
            seq.Append(
                transform.DOPath(wps, pourDur, PathType.CatmullRom,PathMode.Ignore).SetEase(Ease.InOutQuad));

            seq.Append(transform.DOLocalRotate(endEuler, .55f).SetEase(Ease.OutQuad));
            
            seq.OnComplete(PourFinished);
        }
        

        private void PourFinished() {
            CurrentState = State.Delivered;
            DOTween.Sequence()
                .AppendInterval(waitAfterPour)
                .Append(_mr.material.DOColor(_origCol, fadeBackDur)
                    .SetEase(Ease.InOutQuad))
                .OnComplete(() => {
                    plantClickedEvent.Raise();
                    StartPostPourBob();
                });
        }

        private  void StartPostPourBob() {
            
            DOTween.Sequence()
                .Append(transform.DORotateQuaternion(_origRot, .35f)
                    .SetEase(Ease.InOutQuad))
                .AppendCallback(() => {
                    _t1 = transform.DOMoveY(transform.position.y - bobRange, bobDur)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                    
                });
        }

        public void ThrowToTrash() {
            
            CurrentState = State.Thrown;
            Kill();
            
             DOTween.Sequence()
                .Append(transform.DOMove(trashTarget.position, moveDur)
                    .SetEase(Ease.InOutQuad))
                .Join(transform.DOScale(Vector3.zero, moveDur)
                    .SetEase(Ease.InOutQuad))
                .OnComplete(() =>
                {
                    trashThrownEvent.Raise();         
                    OnCupDisposed?.Invoke();
                    gameObject.SetActive(false);
                });
        }

        private   void Kill() 
        {
            var tweens = new[] { _t0, _t1, _liftTween, _bobTween, _followTween };
            for (int i = 0; i < tweens.Length; i++)
            {
                tweens[i]?.Kill();
            }
            _t0 = _t1 = _liftTween = _bobTween = _followTween = null;
        }
    }
}
