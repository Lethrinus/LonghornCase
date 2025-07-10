
using UnityEngine;
using DG.Tweening;
using Core;
using Managers;

namespace Clickables {
    public class PenController : ClickableBase
    {
        enum State { Idle, Hovering, Writing }
        
        
        [SerializeField] ParticleSystem chalkDustPrefab;  // drag prefab here
        [SerializeField] Transform dustSpawnOffset;      // empty child at pen tip
        
        
        [Header("Hover")]
        [SerializeField] float liftHeight = .5f, liftDur = .5f;
        [SerializeField] float wobbleY = 15f, wobbleZ = 10f, wobbleSpeed = 1f, fadeIn = .3f;

        [Header("Write Path")]
        [SerializeField] Transform writePathParent;
        [SerializeField] float     writeDur = 1f;

        Tween _lift, _fade, _write;
        Vector3    _origPos;
        Quaternion _origRot;
        float      _angle, _amp;
        Vector3[]  _wps;
        State      _st = State.Idle;

        void Awake()
        {
            _origPos = transform.position;
            _origRot = transform.localRotation;
            int c = writePathParent.childCount;
            _wps = new Vector3[c];
            for (int i = 0; i < c; i++)
                _wps[i] = writePathParent.GetChild(i).position;
        }

        public override bool CanClickNow(GameState state)
            => state == GameState.ClickPen || state == GameState.DrawBoard && _st == State.Hovering;

        protected override void OnValidClick()
        {
            var gm = GameManager.Instance.State;

            // first click brings us into Hovering
            if (gm == GameState.ClickPen)
            {
                StartHover();
            }
            // when in DrawBoard phase...
            else if (gm == GameState.DrawBoard)
            {
                // if we're still hovering and the user clicks the pen again, cancel hover
                if (_st == State.Hovering)
                {
                    ReturnHome();                // send it back to original position
                    EventBus.Publish(new HoverCancelledEvent()); // optional: notify if you need it
                }
                else
                {
                    TriggerWrite();              // normal “draw on board” click
                }
            }
        }

        void Update()
        {
            if (_st != State.Hovering) return;
            _angle += wobbleSpeed * Time.deltaTime;
            if (_angle > Mathf.PI * 2) _angle -= Mathf.PI * 2;
            float y = Mathf.Sin(_angle) * wobbleY * _amp;
            float z = Mathf.Cos(_angle) * wobbleZ * _amp;
            transform.localRotation = _origRot * Quaternion.Euler(0, y, z);
        }

        void StartHover()
        {
            _st = State.Hovering;
            _angle = _amp = 0;
            Kill();
            _lift = transform.DOMoveY(_origPos.y + liftHeight, liftDur).SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _fade = DOTween.To(() => _amp, x => _amp = x, 1f, fadeIn).SetEase(Ease.InOutQuad);
                    EventBus.Publish(new PenClickedEvent());
                });
        }

        public void TriggerWrite()
        {
            _st = State.Writing;
            Kill();

            // --- NEW: spawn dust once ---------------------------------
            var fx = Instantiate(chalkDustPrefab,
                dustSpawnOffset.position,
                Quaternion.identity);
            fx.Play();                         // auto-destroy after lifetime
            // -----------------------------------------------------------

            _write = transform.DOPath(_wps, writeDur, PathType.CatmullRom, PathMode.Full3D)
                .SetLookAt(.05f)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    ReturnHome();
                    EventBus.Publish(new BoardDrawnEvent());
                });
        }


        void ReturnHome()
        {
            _st = State.Idle;
            Kill();
            DOTween.Sequence()
                .Append(transform.DOMove(_origPos, liftDur).SetEase(Ease.InOutQuad))
                .Join(transform.DORotateQuaternion(_origRot, liftDur).SetEase(Ease.InOutQuad));
        }

        void Kill() { _lift?.Kill(); _fade?.Kill(); _write?.Kill(); }
    }
}
