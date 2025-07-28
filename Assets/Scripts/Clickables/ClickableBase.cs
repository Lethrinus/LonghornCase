using UnityEngine;
using Managers;
using Zenject;

namespace Clickables
{
    [RequireComponent(typeof(Collider))]
    public abstract class ClickableBase : MonoBehaviour, IClickable
    {

        [Inject] protected GameManager Game;
        public abstract bool CanClickNow(GameState state);
        protected abstract void OnValidClick();
    
        
    
        private void OnMouseDown()
        {
            if (Game == null) return;
            if(CanClickNow(Game.State))
                OnValidClick();
        }
    }
}