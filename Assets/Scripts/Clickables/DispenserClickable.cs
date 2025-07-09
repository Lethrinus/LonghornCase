// Assets/Scripts/Clickables/DispenserClickable.cs
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

        [Header("Rotation-Shake Feedback")]
        [SerializeField] float rotShakeAngle      = 10f;
        [SerializeField] float rotShakeDuration   = .5f;
        [SerializeField] int   rotShakeVibrato    = 8;
        [SerializeField] float rotShakeRandomness = 45f;
        [SerializeField] Ease  rotShakeEase       = Ease.InOutSine;

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
               
            }

            
            _shakeTween?.Kill();
           
        }
    }
}