// Assets/Scripts/Clickables/TrashClickable.cs
using UnityEngine;
using Core;
using Managers;

namespace Clickables
{
    [DisallowMultipleComponent]
    public class TrashClickable : ClickableBase
    {
        [SerializeField] CupController cup;

        public override bool CanClickNow(GameState gameState)
            => gameState == GameState.ThrowTrash
               && cup.CurrentState == CupController.State.AtPlant;

        protected override void OnValidClick()
        {
            cup.ThrowToTrash();
        }
    }
}