using UnityEngine;
using Core;
using Clickables;
using Managers;

namespace Clickables {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class PlantClickable : ClickableBase {
        [SerializeField] private CupController cup;

        public override bool CanClickNow(GameState gs) {
            return gs == GameState.ClickPlant
                   && (cup.CurrentState == CupController.State.Floating
                       || cup.CurrentState == CupController.State.AtPlant);
        }

        protected override void OnValidClick() {
        
            cup.StartPour();
        }
    }
}
