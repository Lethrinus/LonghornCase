using Audio;
using UnityEngine;
using DG.Tweening;
using Core;
using Managers;

namespace Clickables {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class PenController : ClickableBase
    {
        enum State { Idle, Hovering, Writing }

     
        static readonly PenClickedEvent     CachedPenEvent   = new();
        static readonly BoardDrawnEvent     CachedBoardEvent = new();
        static readonly HoverCancelledEvent CachedHoverEvent = new();
        

        [Header("Audio")]
        [SerializeField] AudioSource sfxSrc;      
        [SerializeField] AudioClip   clickClip;    
        [SerializeField] AudioClip   scribbleClip; 
        
        [Header("Hover")]
        [SerializeField] float liftHeight = .5f, liftDur = .5f;
        [SerializeField] float wobbleY = 15f, wobbleZ = 10f, wobbleSpeed = 1f, fadeIn = .3f;

        [Header("Write Path")]
        [SerializeField] Transform writePathParent;
        [SerializeField] float     writeDur = 1f;

        /*──────── Runtime ────────*/
        Tween        _lift, _fade, _write, _wobble;
        Vector3      _origPos;
        Quaternion   _origRot, _hoverBaseRot, _boardFacing;
        float        _angle, _amp;
        Vector3[]    _wps;
        State        _st  = State.Idle;
        Collider     _col;
        void Awake()
        {
            _origPos = transform.position;
            _origRot = transform.localRotation;
            _col     = GetComponent<Collider>();
            _boardFacing = Quaternion.Euler(0f, 180f, 0f);
            int c = writePathParent.childCount;
            _wps = new Vector3[c];
            for (int i = 0; i < c; ++i)
                _wps[i] = writePathParent.GetChild(i).position;

            writePathParent = null;         
        }

        public override bool CanClickNow(GameState gs)
            => (gs == GameState.ClickPen   && _st == State.Idle)   || 
               (gs == GameState.DrawBoard && _st == State.Hovering);   
        
        protected override void OnValidClick()
        {
            var gs = GameManager.Instance.State;

            if (gs == GameState.ClickPen)
            {
                StartHover();
            }
            else if (gs == GameState.DrawBoard)
            {
                if (_st == State.Hovering)
                {
                    ReturnHome();
                    EventBus.Publish(CachedHoverEvent);
                }
                else
                {
                    TriggerWrite();
                }
            }
        }
        void StartWobble()
                    {
                        const float twoPI = Mathf.PI * 2f;
            
                           _angle        = 0f;
                        _hoverBaseRot = transform.localRotation;
            
                          
                                _wobble = DOTween.To(() => 0f, a =>
                            {
                                
                                    _angle = a;
                                float y = Mathf.Sin(_angle) * wobbleY * _amp;
                                float z = Mathf.Cos(_angle) * wobbleZ * _amp;
                                transform.localRotation =
                                        _hoverBaseRot * Quaternion.Euler(0, y, z);
                
                               }, twoPI, twoPI / wobbleSpeed)       
                       .SetEase(Ease.Linear).
                       SetLoops(-1, LoopType.Incremental).
                       SetLink(gameObject, LinkBehaviour.KillOnDestroy); }
        
        
        void StartHover()
        {
            _st  = State.Hovering;
            _amp = 0f;               
            _angle = 0f;             
            Kill();                  

            if (clickClip)          
                sfxSrc.PlayOneShot(clickClip);

            _col.Lock(liftDur);      

            _lift = transform.DOMoveY(_origPos.y + liftHeight, liftDur)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    StartWobble();   
            
                  
                    _fade = DOTween.To(() => _amp, v => _amp = v, 1f, fadeIn)
                        .SetEase(Ease.InOutQuad);

                    EventBus.Publish(CachedPenEvent);   
                });
        }

        public void TriggerWrite()
        {
            _st = State.Writing;
            Kill();
            
            if (scribbleClip) sfxSrc.PlayOneShot(scribbleClip);
            transform.rotation = _boardFacing;
            
            _write = transform.DOPath(_wps, writeDur, PathType.CatmullRom)
                .SetLookAt(.05f)
                .SetEase(Ease.InOutQuad)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
                .OnWaypointChange(OnWriteWaypointChanged)
                .OnComplete(OnWriteComplete);
        }


        void OnWriteWaypointChanged(int idx)
        {
            if (idx == 5)
                EventBus.Publish(new SfxEvent(scribbleClip, 1f));
        }

        void OnWriteComplete()
        {
            ReturnHome();
            EventBus.Publish(CachedBoardEvent);
        }


        void ReturnHome()
        {
            if (clickClip)          
                sfxSrc.PlayOneShot(clickClip);
            _st = State.Idle;
            Kill();
            DOTween.Sequence()
                .Append(transform.DOMove(_origPos, liftDur).SetEase(Ease.InOutQuad))
                .Join(transform.DORotateQuaternion(_origRot, liftDur).SetEase(Ease.InOutQuad));
        }

        void Kill() {
            _lift?.Kill();
            _fade?.Kill();
            _write?.Kill();
            _wobble?.Kill();    
        }
    }
}
