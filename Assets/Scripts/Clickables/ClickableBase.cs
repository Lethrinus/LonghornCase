
using System;
using UnityEngine;
using Managers;

namespace Clickables
{
    public abstract class ClickableBase : MonoBehaviour, IClickable
    {
        public abstract bool CanClickNow(GameState state);
        protected abstract void OnValidClick();
    
        private GameManager _gameManager;
    
        protected virtual void Start()
        {
            _gameManager = GameManager.Instance;
        }
    
        void OnMouseDown()
        {
            if (_gameManager == null) return;
            if (CanClickNow(_gameManager.State))
                OnValidClick();
        }
    }
}