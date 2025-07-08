using UnityEngine;
using DG.Tweening;
using Core;
using Managers;
using Clickables;

[RequireComponent(typeof(Collider),typeof(MeshRenderer))]
public class BoardClickable : MonoBehaviour {
    [SerializeField] PenController pen;
    MeshRenderer _r;
    Color        _o;
    void Awake(){
        _r = GetComponent<MeshRenderer>();
        _o = _r.material.color;
    }
    void OnMouseDown(){
        if(GameManager.Instance.State != GameState.DrawBoard){
            transform.DOShakePosition(.2f,new Vector3(.02f,.02f,.02f),8,45).SetEase(Ease.InOutSine);
            return;
        }
        
        _r.material.DOColor(Color.black, .5f).SetEase(Ease.InOutQuad);
        pen.TriggerWrite();
    }
}