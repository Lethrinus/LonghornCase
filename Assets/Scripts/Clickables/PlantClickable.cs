using UnityEngine;
using DG.Tweening;
using Core;
using Managers;

[RequireComponent(typeof(Collider))]
public class PlantClickable : MonoBehaviour {
    Tween _shake;
    void OnMouseDown(){
        if(GameManager.Instance.State != GameState.ClickPlant){
            transform.DOShakePosition(.2f,new Vector3(.02f,.02f,.02f),8,45).SetEase(Ease.InOutSine);
            return;
        }
        
        transform.DOPunchScale(Vector3.one*0.1f,0.5f,5,1f).SetEase(Ease.InOutSine)
            .OnComplete(()=> EventBus.Publish(new PlantClickedEvent()));
    }
}