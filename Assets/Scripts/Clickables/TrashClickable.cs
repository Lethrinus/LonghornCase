using UnityEngine;
using Core;
using Managers;


namespace Clickables {
    [DisallowMultipleComponent]
    public class TrashClickable : ClickableBase {
        [SerializeField] CupController cup;

        public override bool CanClickNow(GameState gs)
            => gs == GameState.ThrowTrash;
              
             
                
                 

        protected override void OnValidClick() {
            cup?.ThrowToTrash();  
        }
    }
}