using Audio;
using UnityEngine;
using DG.Tweening;
using Core;
using Managers;
using Zenject;

namespace Clickables {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class PenController : ClickableBase
    {
        [Inject] private SignalBus _bus;
        private enum State { Idle, Hovering, Writing }

        [SerializeField] GameEvent penClickedEvent;
        [SerializeField] GameEvent boardDrawnEvent;
        [SerializeField] GameEvent hoverCancelledEvent;
        

        [Header("Audio")]
        [SerializeField] private AudioSource sfxSrc;      
        [SerializeField] private AudioClip   clickClip;    
        [SerializeField] private AudioClip   scribbleClip; 
        
        [Header("Hover")]
        [SerializeField] private float liftHeight = .5f, liftDur = .5f;
        [SerializeField] private float wobbleY = 15f, wobbleZ = 10f, wobbleSpeed = 1f, fadeIn = .3f;

        [Header("Write Path")]
        [SerializeField]private Transform writePathParent;
        [SerializeField]private float     writeDur = 1f;

        /*──────── Runtime ────────*/
        private Tween        _lift, _fade, _write, _wobble;
        private Vector3      _origPos;
        private  Quaternion   _origRot, _hoverBaseRot, _boardFacing;
        private  float        _angle, _amp;
        private  Vector3[]    _wps;
        private State        _st  = State.Idle;
        private Collider     _col;
         private void Awake()
        {
            _origPos = transform.position;
            _origRot = transform.localRotation;
            _col = GetComponent<Collider>();
            _boardFacing = Quaternion.Euler(0f, 180f, 0f);
    
           
            CacheWaypoints();
            writePathParent = null;
        }

        public override bool CanClickNow(GameState gs)
            => (gs == GameState.ClickPen   && _st == State.Idle)   || 
               (gs == GameState.DrawBoard && _st == State.Hovering);   
        
        protected override void OnValidClick() {
            var gs = GameManager.Instance.State;
         

            if (gs == GameState.ClickPen) {
                penClickedEvent.Raise();
                

                StartHover();
            }
            else if (gs == GameState.DrawBoard)
            {
                if (_st == State.Hovering)
                {
                    ReturnHome();
                    hoverCancelledEvent.Raise();     
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
                    penClickedEvent.Raise();
                    StartWobble();   
            
                  
                    _fade = DOTween.To(() => _amp, v => _amp = v, 1f, fadeIn)
                        .SetEase(Ease.InOutQuad);

                    
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
               _bus.Fire(new SfxSignal(scribbleClip));
        }

        void OnWriteComplete()
        {
            ReturnHome();
            boardDrawnEvent.Raise();
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
        void CacheWaypoints()
        {
            int c = writePathParent.childCount;
            _wps = new Vector3[c];
            for (int i = 0; i < c; ++i)
                _wps[i] = writePathParent.GetChild(i).position;
        }
        void Kill() {
            _lift?.Kill();
            _fade?.Kill();
            _write?.Kill();
            _wobble?.Kill();    
        }
    }
}
