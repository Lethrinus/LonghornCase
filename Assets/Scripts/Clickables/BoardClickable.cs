using UnityEngine;
using Core;
using Managers;

namespace Clickables {
    [RequireComponent(typeof(MeshRenderer))]
    public class BoardClickable : ClickableBase
    {
        [SerializeField] PenController pen;
        TextRevealController _reveal;
        MeshRenderer _renderer;

        void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _reveal   = GetComponentInChildren<TextRevealController>();
        }

        public override bool CanClickNow(GameState state)
            => state == GameState.DrawBoard;

        protected override void OnValidClick()
        {
            
            _reveal?.Reveal("hello world!");
            pen.TriggerWrite();
        }
    }
}