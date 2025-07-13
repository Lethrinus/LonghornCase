
using System;
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
            if (GameManager.Instance == null) return;
            var state = GameManager.Instance.State;
            if (CanClickNow(state))
                OnValidClick();               
          
        }
        
    }
}