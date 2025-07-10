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
        private Tween _shakeTween;

        public override bool CanClickNow(GameState gameState)
        {
            return gameState == GameState.ClickDispenser 
                   && (cup.CurrentState == CupController.State.Hovering 
                       ||   cup.CurrentState == CupController.State.AtDispenser) 
                   && cup.CurrentState != CupController.State.Delivered;
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