
using UnityEngine;
using Managers;

namespace Clickables
{
    public abstract class ClickableBase : MonoBehaviour, IClickable
    {
        public abstract bool   CanClickNow(GameState state);
        protected abstract void OnValidClick();

        void OnMouseDown()
        {
            var state = GameManager.Instance.State;
            if (CanClickNow(state))
                OnValidClick();               
          
        }
    }
}