using UnityEngine;
using DG.Tweening;
using Core;
using Managers;

namespace Clickables {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class PlantClickable : ClickableBase
    {
        [SerializeField] CupController cup;

        public override bool CanClickNow(GameState s)
            => s == GameState.ClickPlant && cup.CurrentState == CupController.State.Delivered;

        protected override void OnValidClick()
        {
            cup.SendMessage("MoveToPlant");       
        }
    }
}