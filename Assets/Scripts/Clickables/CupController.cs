using System;
using System.IO.IsolatedStorage;
using UnityEngine;
using DG.Tweening;
using Core;
using Audio;
using Managers;

namespace Clickables {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class CupController : ClickableBase {
        public enum State {
            Idle, Hovering, AtDispenser, Delivered, Floating, AtPlant, Thrown
        }
        
        [SerializeField] GameEvent cupClickedEvent;
        [SerializeField] GameEvent cupHoverCancelledEvent;
        [SerializeField] GameEvent cupFilledEvent;
        [SerializeField] GameEvent plantClickedEvent;
        [SerializeField] GameEvent trashThrownEvent;
      
        [Header("Audio")]
        [SerializeField] AudioClip clickClip;
        [SerializeField] float     clickVolume = 1f;
        [SerializeField] float     clickPitch  = 1f;

        [Header("Hover & Bob")]
        [SerializeField] float hoverHeight = .3f, hoverDur = .4f;
        [SerializeField] float bobRange    = .05f, bobDur   = 1f;

        [Header("Move Targets")]
        [SerializeField] Transform dispenserTarget, plantTarget, trashTarget;
        [SerializeField] float     moveDur = 1f;

        [Header("Fill / Float")]
        [SerializeField] Color filledColor    = Color.cyan;
        [SerializeField] float floatFwd       = .15f, floatUp = .08f;

        [Header("Pour Path")]
        [SerializeField] Transform pourPathParent;
        [SerializeField] float     pourDur = 1f, waitAfterPour = .25f, fadeBackDur = .5f;

        Vector3      _origPos;
        Quaternion   _origRot;
        MeshRenderer _mr;
        Color        _origCol;
        Vector3[]    _pourWps;
        Tween        _t0, _t1, _liftTween, _bobTween, _followTween;
        State        _st = State.Idle;
        ParticleSystem _activeStream;

        public static event Action OnCupDisposed;

        public Color OrigCol => _origCol;

        public State CurrentState => _st;

        void Awake() {
            _origPos = transform.position;
            _origRot = transform.localRotation;
            _mr      = GetComponent<MeshRenderer>();
            _origCol = _mr.material.color;

            int c = pourPathParent.childCount;
            _pourWps = new Vector3[c];
            for (int i = 0; i < c; ++i)
                _pourWps[i] = pourPathParent.GetChild(i).position;
        }

        public override bool CanClickNow(GameState gs) =>
            (gs == GameState.ClickCup && (_st == State.Idle || _st == State.Hovering)) ||
            (gs == GameState.ClickDispenser && _st == State.Hovering) ||
            (gs == GameState.ClickPlant && (_st == State.Delivered || _st == State.Floating));
          

        protected override void OnValidClick() {
            if (clickClip != null)
                EventBus.Publish(new SfxEvent(clickClip, clickVolume, clickPitch));

            var gs = GameManager.Instance.State;
            switch (gs) {
                case GameState.ClickCup:
                case GameState.ClickDispenser:
                    if (_st == State.Idle) { 
                        if (clickClip != null) 
                            EventBus.Publish(new SfxEvent(clickClip, clickVolume, clickPitch));
                        StartHover();
                    }
                    else if (_st == State.Hovering) {
                      
                        ReturnHome();
                    }
                    break;

                case GameState.ClickPlant:
                    if      (_st == State.Delivered) StartFloat();
                    else if (_st == State.Floating)  ReturnToDispenser();
                    break;

               
            }
        }

        void StartHover() {
            _st = State.Hovering;

            
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

        void ReturnHome() {
            _st = State.Idle;

            DOTween.Kill(transform);
            Kill();

            DOTween.Sequence()
                .Append(transform.DOMove(_origPos, hoverDur).SetEase(Ease.InOutQuad))
                .Join(transform.DORotateQuaternion(_origRot, hoverDur).SetEase(Ease.InOutQuad));
                    cupHoverCancelledEvent.Raise(); 
                }
        

        public void MoveToDispenser() {
            _st = State.AtDispenser;
            Kill();
            _t0 = transform.DOMove(dispenserTarget.position, moveDur)
                           .SetEase(Ease.InOutQuad);
        }

        public void FillWater() {
            _st = State.Delivered;
            Kill();

            _followTween = DOTween.To(() => 0f, _ => { },
                0f, .4f
            ).SetEase(Ease.Linear);

            _t0 = _mr.material.DOColor(filledColor, .4f)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>  cupFilledEvent.Raise());
        }

        void ReturnToDispenser() {
            _st = State.Delivered;
            Kill();
            _t0 = transform.DOMove(dispenserTarget.position, hoverDur)
                           .SetEase(Ease.InOutQuad);
        }

        void StartFloat() {
            _st = State.Floating;
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
            _st = State.AtPlant;
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
        

        void PourFinished() {
            _st = State.Delivered;
            DOTween.Sequence()
                .AppendInterval(waitAfterPour)
                .Append(_mr.material.DOColor(_origCol, fadeBackDur)
                    .SetEase(Ease.InOutQuad))
                .OnComplete(() => {
                    plantClickedEvent.Raise();
                    StartPostPourBob();
                });
        }

        void StartPostPourBob() {
            
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
            
            _st = State.Thrown;
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

        void Kill() 
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
