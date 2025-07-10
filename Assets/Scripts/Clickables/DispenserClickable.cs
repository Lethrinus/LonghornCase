using UnityEngine;
using Core;
using Clickables;
using Managers;

namespace Clickables {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class DispenserClickable : ClickableBase {
        [SerializeField] CupController cup;

        public override bool CanClickNow(GameState gs) =>
            gs == GameState.ClickDispenser &&
            (cup.CurrentState == CupController.State.Hovering ||
             cup.CurrentState == CupController.State.AtDispenser);

        protected override void OnValidClick() {
            if      (cup.CurrentState == CupController.State.Hovering)     cup.MoveToDispenser();
            else if (cup.CurrentState == CupController.State.AtDispenser)  cup.FillWater();
        }
    }
}