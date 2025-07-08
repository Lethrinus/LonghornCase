using UnityEngine;
using DG.Tweening;
using Core;
using Managers;

namespace Clickables {
    [DisallowMultipleComponent]
    public class PenController : MonoBehaviour {
        enum State { Idle, Hovering, Writing }

        [Header("Hover")]
        [SerializeField] float liftHeight   = .5f;
        [SerializeField] float liftDur      = .5f;
        [SerializeField] float wobbleY      = 15f;
        [SerializeField] float wobbleZ      = 10f;
        [SerializeField] float wobbleSpeed  = 1f;
        [SerializeField] float fadeIn       = .3f;

        [Header("Write Path")]
        [SerializeField] Transform writePathParent;
        [SerializeField] float     writeDur    = 1f;

        Tween      _lift, _fade, _write;
        Vector3    _origPos;
        Quaternion _origRot;
        float      _angle, _amp;
        Vector3[]  _wps;
        State      _st = State.Idle;

        void Awake(){
            _origPos = transform.position;
            _origRot = transform.localRotation;
            int c = writePathParent.childCount;
            _wps = new Vector3[c];
            for(int i=0;i<c;i++) _wps[i] = writePathParent.GetChild(i).position;
        }

        void OnMouseDown(){
            if(GameManager.Instance.State != GameState.ClickPen) {
                transform.DOShakePosition(.2f,new Vector3(.02f,.02f,.02f),8,45).SetEase(Ease.InOutSine);
                return;
            }
            
            StartHover();
        }

        void Update(){
            if(_st!=State.Hovering) return;
            _angle += wobbleSpeed*Time.deltaTime;
            if(_angle>Mathf.PI*2) _angle-=Mathf.PI*2;
            float y = Mathf.Sin(_angle)*wobbleY*_amp;
            float z = Mathf.Cos(_angle)*wobbleZ*_amp;
            transform.localRotation = _origRot * Quaternion.Euler(0,y,z);
        }

        void StartHover(){
            _st = State.Hovering;
            _angle=_amp=0;
            Kill();
            _lift = transform.DOMoveY(_origPos.y+liftHeight,liftDur).SetEase(Ease.OutQuad)
                .OnComplete(()=>{
                    _fade = DOTween.To(()=>_amp,x=>_amp=x,1f,fadeIn).SetEase(Ease.InOutQuad);
                    EventBus.Publish(new PenClickedEvent());
                });
        }

        public void TriggerWrite(){
            if(GameManager.Instance.State!=GameState.DrawBoard || _st!=State.Hovering){
                transform.DOShakePosition(.2f,new Vector3(.02f,.02f,.02f),8,45).SetEase(Ease.InOutSine);
                return;
            }
            _st=State.Writing;
            Kill();
            _write = transform.DOPath(_wps,writeDur,PathType.CatmullRom)
                .SetEase(Ease.InOutSine)
                .OnComplete(()=>{
                    ReturnHome();
                    EventBus.Publish(new BoardDrawnEvent());
                });
        }

        void ReturnHome(){
            _st=State.Idle;
            Kill();
            DOTween.Sequence()
                .Append(transform.DOMove(_origPos,liftDur).SetEase(Ease.InOutQuad))
                .Join(transform.DORotateQuaternion(_origRot,liftDur).SetEase(Ease.InOutQuad));
        }

        void Kill(){ _lift?.Kill(); _fade?.Kill(); _write?.Kill(); }

    }
}
