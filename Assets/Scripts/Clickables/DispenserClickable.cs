using UnityEngine;
using DG.Tweening;
using Core;
using Managers;
using Clickables;

[RequireComponent(typeof(Collider))]
public class DispenserClickable : MonoBehaviour
{
    [SerializeField] private CupController cup;

    [Header("Dispenser Rotation Shake")]
    [SerializeField] private float shakeAngleZ     = 10f;
    [SerializeField] private float shakeDuration   = 0.5f;
    [SerializeField] private int   shakeVibrato    = 8;
    [SerializeField] private float shakeRandomness = 45f;
    [SerializeField] private Ease  shakeEase       = Ease.InOutSine;

    private Tween _shakeTween;

    void OnMouseDown()
    {
       
        if (GameManager.Instance.State != GameState.ClickDispenser)
        {
            transform
                .DOShakePosition(.2f, new Vector3(.02f, .02f, .02f), 8, 45)
                .SetEase(Ease.InOutSine);
            return;
        }

        
        if (cup.CurrentState == CupController.State.Hovering)
        {
            cup.Dispense();
            _shakeTween?.Kill();
            _shakeTween = transform
                .DOShakeRotation(shakeDuration, new Vector3(0, 0, shakeAngleZ), shakeVibrato, shakeRandomness, fadeOut: true)
                .SetEase(shakeEase);
            return;
        }

       
        if (cup.CurrentState == CupController.State.AtDispenser)
        {
            cup.FillColor();
            EventBus.Publish(new DispenserClickedEvent());

            _shakeTween?.Kill();
            _shakeTween = transform
                .DOShakeRotation(shakeDuration, new Vector3(0, 0, shakeAngleZ), shakeVibrato, shakeRandomness, fadeOut: true)
                .SetEase(shakeEase);
            return;
        }
    }
}