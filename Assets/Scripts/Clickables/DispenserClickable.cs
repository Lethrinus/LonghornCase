
using UnityEngine;
using DG.Tweening;
using Core;
using Managers;

namespace Clickables
{
    [DisallowMultipleComponent]
    public class DispenserClickable : ClickableBase
    {
        [SerializeField] private CupController cup;

        [Header("Shake Feedback")]
        [SerializeField] float shakeAngleZ   = 10f;
        [SerializeField] float shakeDuration = .5f;
        [SerializeField] int   shakeVibrato  = 8;
        [SerializeField] float shakeRandomness = 45f;
        [SerializeField] Ease  shakeEase     = Ease.InOutSine;

        private Tween _shakeTween;

        public override bool CanClickNow(GameState gameState)
        {
            
            return gameState == GameState.ClickDispenser
                   && (cup.CurrentState == CupController.State.Hovering
                       || cup.CurrentState == CupController.State.AtDispenser);
        }

        protected override void OnValidClick()
        {
            if (cup.CurrentState == CupController.State.Hovering)
            {
               
                cup.Dispense();
            }
            else 
            {
               
                cup.FillColor();
                EventBus.Publish(new DispenserClickedEvent());
            }

          
            _shakeTween?.Kill();
            _shakeTween = transform
                .DOShakeRotation(
                    shakeDuration,
                    new Vector3(0f, 0f, shakeAngleZ),
                    shakeVibrato,
                    shakeRandomness,
                    fadeOut: true
                )
                .SetEase(shakeEase);
        }
    }
}