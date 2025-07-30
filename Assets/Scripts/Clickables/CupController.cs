// Assets/Scripts/Clickables/CupController.cs
using System;
using DG.Tweening;
using Infrastructure.Signals;
using Managers;
using UnityEngine;
using Zenject;

namespace Clickables
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class CupController : ClickableBase
    {
        /* ------------ public enum -------------------------------------- */
        public enum State { Idle, Hovering, AtDispenser, Delivered, Floating, AtPlant, Thrown }
        
        
    
        /* ------------ inspector: events replaced by signals ------------- */
        [Header("Audio")]
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private float     clickVolume = 1f;
        [SerializeField] private float     clickPitch  = 1f;

        [Header("Hover & Bob")]
        [SerializeField] private float hoverHeight = .3f;
        [SerializeField] private float hoverDur    = .4f;
        [SerializeField] private float bobRange    = .05f;
        [SerializeField] private float bobDur      = 1f;

        [Header("Move Targets")]
        [SerializeField] private Transform dispenserTarget;
        [SerializeField] private Transform plantTarget;
        [SerializeField] private Transform trashTarget;
        [SerializeField] private float     moveDur = 1f;

        [Header("Fill / Float")]
        [SerializeField] private Color filledColor = Color.cyan;
        [SerializeField] private float floatFwd    = .15f;
        [SerializeField] private float floatUp     = .08f;

        [Header("Pour Path")]
        [SerializeField] private Transform pourPathParent;
        [SerializeField] private float     pourDur       = 1f;
        [SerializeField] private float     waitAfterPour = .25f;
        [SerializeField] private float     fadeBackDur   = .5f;

        /* ------------ SIGNAL BUS --------------------------------------- */
        [Inject] SignalBus _bus;

        /* ------------ runtime ------------------------------------------ */
        Vector3      _origPos;
        Quaternion   _origRot;
        MeshRenderer _mr;
        Color        _origCol;
        Transform _origParent;
        Vector3[] _pourWps;

        Tween _t0, _t1, _liftTween, _bobTween, _followTween;

        public State CurrentState { get; private set; } = State.Idle;

        /* ---------------------------------------------------------------- */
        void Awake()
        {
            _origPos = transform.position;
            _origRot = transform.localRotation;
            _mr      = GetComponent<MeshRenderer>();
            _origCol = _mr.material.color;
            _origParent = transform.parent;
            int c = pourPathParent.childCount;
            _pourWps = new Vector3[c];
            for (int i = 0; i < c; ++i)
                _pourWps[i] = pourPathParent.GetChild(i).position;
        }

        /* ---------------- ClickableBase --------------------------------- */
        public override bool CanClickNow(GameState gs) =>
            (gs == GameState.ClickCup && (CurrentState == State.Idle || CurrentState == State.Hovering)) ||
            (gs == GameState.ClickDispenser && CurrentState == State.Hovering) ||
            (gs == GameState.ClickPlant && (CurrentState == State.Delivered || CurrentState == State.Floating));

        protected override void OnValidClick()
        {
            if (clickClip) _bus.Fire(new PlaySfxSignal(clickClip, clickVolume, clickPitch));

            var gs = GameManager.Instance.State;
            switch (gs)
            {
                case GameState.ClickCup:
                case GameState.ClickDispenser:
                    if (CurrentState == State.Idle)
                    {
                        StartHover();
                        _bus.Fire<CupClickedSignal>();
                    }
                    else if (CurrentState == State.Hovering)
                    {
                        ReturnHome();
                        _bus.Fire<CupHoverCanceledSignal>();
                    }
                    break;

                case GameState.ClickPlant:
                    if      (CurrentState == State.Delivered) StartFloat();
                    else if (CurrentState == State.Floating)  ReturnToDispenser();
                    break;
            }
        }

        /* ---------------- Idle → Hover ---------------------------------- */
        void StartHover()
        {
            transform.SetParent(null,true);
            CurrentState = State.Hovering;
            KillTweens();

            _liftTween = transform.DOMoveY(_origPos.y + hoverHeight, hoverDur)
                                  .SetEase(Ease.OutQuad)
                                  .OnComplete(StartBob);
        }

        void StartBob()
        {
            _bobTween = transform.DOMoveY(_origPos.y + hoverHeight - bobRange, bobDur)
                                .SetEase(Ease.InOutSine)
                                .SetLoops(-1, LoopType.Yoyo);
        }

        void ReturnHome()
        {
            CurrentState = State.Idle;
            KillTweens();
            transform.SetParent(_origParent, true);
            DOTween.Sequence()
                   .Append(transform.DOMove(_origPos, hoverDur).SetEase(Ease.InOutQuad))
                   .Join(transform.DORotateQuaternion(_origRot, hoverDur).SetEase(Ease.InOutQuad));
        }

        /* ---------------- Dispenser flow -------------------------------- */
        public void MoveToDispenser()
        {
            CurrentState = State.AtDispenser;
            KillTweens();

            _t0 = transform.DOMove(dispenserTarget.position, moveDur)
                           .SetEase(Ease.InOutQuad);
        }

        public void FillWater()
        {
            CurrentState = State.Delivered;
            KillTweens();

            _followTween = DOTween.To(() => 0f, _ => { }, 0f, .4f)
                                  .SetEase(Ease.Linear);

            _t0 = _mr.material.DOColor(filledColor, .4f)
                               .SetEase(Ease.InOutQuad)
                               .OnComplete(() => _bus.Fire<CupFilledSignal>());
        }

        void ReturnToDispenser()
        {
            CurrentState = State.Delivered;
            KillTweens();

            _t0 = transform.DOMove(dispenserTarget.position, hoverDur)
                           .SetEase(Ease.InOutQuad);
        }

        /* ---------------- Float around ---------------------------------- */
        void StartFloat()
        {
            CurrentState = State.Floating;
            KillTweens();

            Vector3 tgt = dispenserTarget.position
                        + dispenserTarget.forward * floatFwd
                        + Vector3.up              * floatUp;

            _t0 = transform.DOMove(tgt, hoverDur).SetEase(Ease.OutQuad)
                           .OnComplete(() =>
                           {
                               _t1 = transform.DOMoveY(tgt.y - bobRange, bobDur)
                                              .SetEase(Ease.InOutSine)
                                              .SetLoops(-1, LoopType.Yoyo);
                               _bus.Fire<CupStartedFloatingSignal>();
                           });
        }

        /* ---------------- Pour at plant --------------------------------- */
        public void StartPour()
        {
            CurrentState = State.AtPlant;
            KillTweens();

            int n = _pourWps.Length;
            var wps = new Vector3[n + 1];
            wps[0] = transform.position;               
            Array.Copy(_pourWps, 0, wps, 1, n);       

            Vector3 endEuler = _origRot.eulerAngles;
            endEuler.x = 110f;

            var seq = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDestroy);

          
            seq.Append(transform.DOPath(wps, pourDur, PathType.CatmullRom, PathMode.Ignore)
                .SetEase(Ease.InOutQuad));

            seq.Append(transform.DOLocalRotate(endEuler, .55f).SetEase(Ease.OutQuad));

            seq.OnComplete(PourFinished);
        }

        void PourFinished()
        {
            CurrentState = State.Delivered;

            DOTween.Sequence()
                   .AppendInterval(waitAfterPour)
                   .Append(_mr.material.DOColor(_origCol, fadeBackDur)
                                       .SetEase(Ease.InOutQuad))
                   .OnComplete(() =>
                   {
                       _bus.Fire<PlantClickedSignal>();
                       StartPostPourBob();
                   });
        }

        void StartPostPourBob()
        {
            DOTween.Sequence()
                   .Append(transform.DORotateQuaternion(_origRot, .35f)
                                    .SetEase(Ease.InOutQuad))
                   .AppendCallback(() =>
                   {
                       _t1 = transform.DOMoveY(transform.position.y - bobRange, bobDur)
                                      .SetEase(Ease.InOutSine)
                                      .SetLoops(-1, LoopType.Yoyo);
                   });
        }

        /* ---------------- Trash flow ------------------------------------ */
        public void ThrowToTrash()
        {
            CurrentState = State.Thrown;
            KillTweens();

            DOTween.Sequence()
                   .Append(transform.DOMove(trashTarget.position, moveDur)
                                     .SetEase(Ease.InOutQuad))
                   .Join(transform.DOScale(Vector3.zero, moveDur)
                                     .SetEase(Ease.InOutQuad))
                   .OnComplete(() =>
                   {
                       _bus.Fire<TrashThrownSignal>();
                       gameObject.SetActive(false);
                   });
        }

        /* ---------------- Utilities ------------------------------------- */
        void KillTweens()
        {
            _t0?.Kill();
            _t1?.Kill();
            _liftTween?.Kill();
            _bobTween?.Kill();
            _followTween?.Kill();
            _t0 = _t1 = _liftTween = _bobTween = _followTween = null;
        }
    }
}
