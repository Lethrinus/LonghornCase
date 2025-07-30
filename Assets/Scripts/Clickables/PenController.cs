
using DG.Tweening;
using Infrastructure.Signals;
using Managers;
using UnityEngine;
using Zenject;

namespace Clickables
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class PenController : ClickableBase
    {
        enum State { Idle, Hovering, Writing }

        /* ---------- inspector ----------------------------------------- */
        [SerializeField] AudioSource sfxSrc;
        [SerializeField] AudioClip   clickClip;
        [SerializeField] AudioClip   scribbleClip;

        [Header("Hover")]
        [SerializeField] float liftHeight  = .5f;
        [SerializeField] float liftDur     = .5f;
        [SerializeField] float wobbleY     = 15f;
        [SerializeField] float wobbleZ     = 10f;
        [SerializeField] float wobbleSpeed = 1f;
        [SerializeField] float fadeIn      = .3f;

        [Header("Write Path")]
        [SerializeField] Transform writePathParent;
        [SerializeField] float     writeDur = 1f;

        [Header("Write Rotation")]
        [SerializeField] float rotateDur  = .35f;
        [SerializeField] Ease  rotateEase = Ease.OutQuad;

        /* ---------- DI -------------------------------------------------- */
        [Inject] SignalBus _bus;

        /* ---------- runtime -------------------------------------------- */
        State      _state = State.Idle;
        Vector3    _origPos;
        Quaternion _origLocalRot;
        Quaternion _localBoardFacing;
        Vector3[]  _wps;

        Tween _lift, _fade, _write, _wobble, _ampTween;
        float _amp;

        /* =============================================================== */
        void Awake()
        {
            _origPos        = transform.position;
            _origLocalRot   = transform.localRotation;          // local!!
            _localBoardFacing = _origLocalRot * Quaternion.Euler(0f, 180f, 0f);

            CacheWaypoints();
        }

        /* ---------- ClickableBase ------------------------------------- */
        public override bool CanClickNow(GameState gs) =>
            (gs == GameState.ClickPen   && _state == State.Idle)   ||
            (gs == GameState.DrawBoard && _state == State.Hovering);

        protected override void OnValidClick()
        {
            var gs = GameManager.Instance.State;

            if (gs == GameState.ClickPen)
            {
                if (clickClip) _bus.Fire(new PlaySfxSignal(clickClip));
                _bus.Fire<PenClickedSignal>();
                StartHover();
            }
            else if (gs == GameState.DrawBoard)
            {
                if (_state == State.Hovering)
                {
                    ReturnHome();
                    _bus.Fire<PenHoverCanceledSignal>();
                }
                else
                {
                    TriggerWrite();
                }
            }
        }

        /* ---------- Hover --------------------------------------------- */
        void StartHover()
        {
            _state = State.Hovering;
            KillTweens();

            _amp = 0f;                               

            float startY  = transform.position.y;
            float targetY = startY + liftHeight;

            _lift = transform.DOMoveY(targetY, liftDur)
                             .SetEase(Ease.OutQuad)
                             .OnComplete(StartWobble);
        }

        void StartWobble()
        {
            const float twoPi = Mathf.PI * 2f;
            float angle = 0f;
            Quaternion baseRot = transform.localRotation;

            _wobble = DOTween.To(() => 0f, a =>
                         {
                             angle = a;
                             float y = Mathf.Sin(angle) * wobbleY * _amp;
                             float z = Mathf.Cos(angle) * wobbleZ * _amp;
                             transform.localRotation = baseRot * Quaternion.Euler(0f, y, z);
                         },
                         twoPi, twoPi / wobbleSpeed)
                        .SetEase(Ease.Linear)
                        .SetLoops(-1, LoopType.Incremental)
                        .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

            _ampTween = DOTween.To(() => _amp, v => _amp = v, 1f, fadeIn)
                               .SetEase(Ease.InOutQuad);

            sfxSrc.DOFade(1f, fadeIn);
        }

        /* ---------- Writing ------------------------------------------- */
        public void TriggerWrite()
        {
            _state = State.Writing;
            KillTweens();

            if (scribbleClip) _bus.Fire(new PlaySfxSignal(scribbleClip));

            var seq = DOTween.Sequence()
                             .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        
            seq.Append(transform.DOLocalRotateQuaternion(_localBoardFacing, rotateDur)
                                .SetEase(rotateEase));

           
            seq.Append(transform.DOPath(_wps, writeDur, PathType.CatmullRom)
                                .SetLookAt(.05f)
                                .SetEase(Ease.InOutQuad)
                                .OnWaypointChange(OnWriteWaypointChanged));

            seq.OnComplete(OnWriteComplete);
            _write = seq;
        }

        void OnWriteWaypointChanged(int idx)
        {
            if (idx == 5 && scribbleClip)
                _bus.Fire(new PlaySfxSignal(scribbleClip));
        }

        void OnWriteComplete()
        {
            ReturnHome();
            _bus.Fire<BoardDrawnSignal>();
        }

        /* ---------- Return -------------------------------------------- */
        void ReturnHome()
        {
            if (clickClip) _bus.Fire(new PlaySfxSignal(clickClip));
            _state = State.Idle;
            KillTweens();

            DOTween.Sequence()
                   .Append(transform.DOMove(_origPos, liftDur).SetEase(Ease.InOutQuad))
                   .Join(transform.DOLocalRotateQuaternion(_origLocalRot, liftDur)
                                   .SetEase(Ease.InOutQuad));
        }

        /* ---------- helpers ------------------------------------------- */
        void CacheWaypoints()
        {
            int c = writePathParent.childCount;
            _wps = new Vector3[c];
            for (int i = 0; i < c; ++i)
                _wps[i] = writePathParent.GetChild(i).position;
        }

        void KillTweens()
        {
            _lift?.Kill();
            _fade?.Kill();
            _write?.Kill();
            _wobble?.Kill();
            _ampTween?.Kill();
        }
    }
}
