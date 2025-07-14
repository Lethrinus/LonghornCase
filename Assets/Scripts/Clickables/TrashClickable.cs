using UnityEngine;
using Core;
using Managers;


namespace Clickables {
    [DisallowMultipleComponent]
    public class TrashClickable : ClickableBase {
        [SerializeField] private CupController cup;

        public override bool CanClickNow(GameState gs)
            => gs == GameState.ThrowTrash;
              
             
                
                 

        protected override void OnValidClick() {
            cup?.ThrowToTrash();  
        }
    }
}