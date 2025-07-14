using UnityEngine;
using Core;
using Managers;

namespace Clickables {
    [RequireComponent(typeof(MeshRenderer))]
    public class BoardClickable : ClickableBase
    {
        [SerializeField] private PenController pen;
        private TextRevealController _reveal;
        

        private void Awake()
        {
            _reveal   = GetComponentInChildren<TextRevealController>();
        }

        public override bool CanClickNow(GameState state)
            => state == GameState.DrawBoard;

        protected override void OnValidClick()
        {
            
            _reveal?.Reveal("hello!");
            pen.TriggerWrite();
        }
    }
}