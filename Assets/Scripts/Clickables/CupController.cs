using UnityEngine;
using DG.Tweening;
using Core;
using Managers;

namespace Clickables {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    public class CupController : ClickableBase {
        public enum State {
            Idle, Hovering, AtDispenser, Delivered,
            Floating, AtPlant, Thrown
        }

        /*──────── Inspector ayarları ────────*/
        [Header("Hover / Bob")]
        [SerializeField] float hoverHeight = .3f;
        [SerializeField] float hoverDur    = .4f;
        [SerializeField] float bobRange    = .05f;
        [SerializeField] float bobDur      = 1f;

        [Header("Move Targets")]
        [SerializeField] Transform dispenserTarget;
        [SerializeField] Transform plantTarget;
        [SerializeField] Transform trashTarget;
        [SerializeField] float     moveDur = 1f;

        [Header("Fill Colour")]
        [SerializeField] Color filledColor = Color.blue;

        [Header("Float-out (mavi kupa)")]
        [SerializeField] float floatFwd = .15f;
        [SerializeField] float floatUp  = .08f;

        [Header("Pour Path & Fade-back")]
        [SerializeField] Transform pourPathParent;
        [SerializeField] float     pourDur       = 1f;
        [SerializeField] float     waitAfterPour = .25f;
        [SerializeField] float     fadeBackDur   = .5f;

        /*──────── Alanlar ────────*/
        Vector3      _origPos;
        Quaternion   _origRot;
        MeshRenderer _mr;
        Color        _origCol;
        Vector3[]    _pourWps;
        Tween        _t0,_t1;
        State        _st = State.Idle;

        public State CurrentState => _st;

        /*──────── Kurulum ────────*/
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

        /*──────── Click izinleri ────────*/
        public override bool CanClickNow(GameState gs) => gs switch {
            GameState.ClickCup       => _st is State.Idle or State.Hovering,          
            GameState.ClickDispenser => _st == State.Hovering,                        
            GameState.ClickPlant     => _st is State.Delivered or State.Floating,
            GameState.ThrowTrash     => _st == State.AtPlant,
            _                        => false
        };

        /*──────── Click davranışı ────────*/
        protected override void OnValidClick() {
            var gs = GameManager.Instance.State;

            switch (gs) {
                case GameState.ClickCup:
                    if      (_st == State.Idle)     StartHover();
                    else if (_st == State.Hovering) ReturnHome();
                    break;

              case GameState.ClickDispenser:
            if (_st == State.Hovering) ReturnHome();   // yalnızca Hovering’e dön
            // _st == AtDispenser yolu artık buraya hiç gelmez
            break;
                
                case GameState.ClickPlant:
                    if      (_st == State.Delivered)     StartFloat();
                    else if (_st == State.Floating)      ReturnToDispenser();
                    break;

                case GameState.ThrowTrash:
                    if (_st == State.AtPlant) ThrowToTrash();
                    break;
            }
        }

        void StartHover() {
            _st = State.Hovering;
            Kill();
            _t0 = transform.DOMoveY(_origPos.y + hoverHeight, hoverDur)
                           .SetEase(Ease.OutQuad)
                           .OnComplete(() => {
                               EventBus.Publish(new CupClickedEvent());  
                               _t1 = transform.DOMoveY(_origPos.y + hoverHeight - bobRange, bobDur)
                                              .SetEase(Ease.InOutSine)
                                              .SetLoops(-1, LoopType.Yoyo);
                           });
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
            _t0 = _mr.material.DOColor(filledColor, .4f)
                               .SetEase(Ease.InOutQuad)
                               .OnComplete(() => EventBus.Publish(new CupFilledEvent()));
        }

        void StartFloat() {
            _st = State.Floating;
            Kill();
            Vector3 tgt = dispenserTarget.position +
                          dispenserTarget.forward * floatFwd +
                          Vector3.up              * floatUp;

            _t0 = transform.DOMove(tgt, hoverDur).SetEase(Ease.OutQuad)
                           .OnComplete(() => {
                               _t1 = transform.DOMoveY(tgt.y - bobRange, bobDur)
                                              .SetEase(Ease.InOutSine)
                                              .SetLoops(-1, LoopType.Yoyo);
                           });
        }

        void ReturnToDispenser() {
            _st = State.Delivered;         
            Kill();
            _t0 = transform.DOMove(dispenserTarget.position, hoverDur)
                           .SetEase(Ease.InOutQuad);
        }

        void ReturnHome() {
            _st = State.Idle;
            Kill();
            DOTween.Sequence()
                   .Append(transform.DOMove(_origPos, hoverDur)
                                     .SetEase(Ease.InOutQuad))
                   .Join (transform.DORotateQuaternion(_origRot, hoverDur)
                                     .SetEase(Ease.InOutQuad))
                   .OnComplete(() => {
                       transform.position      = _origPos;   
                       transform.localRotation = _origRot;
                       EventBus.Publish(new CupHoverCancelledEvent());
                   });
        }

        public void StartPour() {
            _st = State.AtPlant;
            Kill();
            transform.DOPath(_pourWps, pourDur, PathType.CatmullRom, PathMode.Full3D)
                     .SetEase(Ease.InOutQuad)
                     .OnComplete(PourFinished);         
        }

       
        void PourFinished() {
            EventBus.Publish(new PlantClickedEvent());    

            Quaternion downRot = _origRot * Quaternion.Euler(90, 0, 0);

            DOTween.Sequence()
                .AppendInterval(waitAfterPour)
                .Append(_mr.material.DOColor(_origCol, fadeBackDur)
                    .SetEase(Ease.InOutQuad))
                .Join (transform.DORotateQuaternion(downRot,    fadeBackDur)
                    .SetEase(Ease.InOutQuad))
                .OnComplete(StartPostPourBob);
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
                   .Join (transform.DOScale(Vector3.zero, moveDur)                 
                                     .SetEase(Ease.InOutQuad))
                   .OnComplete(() => {
                       EventBus.Publish(new TrashThrownEvent());
                       gameObject.SetActive(false);                              
                   });
        }

        void Kill() { _t0?.Kill(); _t1?.Kill(); }
    }
}
