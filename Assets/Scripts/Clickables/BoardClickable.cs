// Assets/Scripts/Clickables/BoardClickable.cs
using DG.Tweening;
using Managers;
using UnityEngine;

namespace Clickables
{
    [RequireComponent(typeof(MeshRenderer))]
    public class BoardClickable : ClickableBase
    {
        [SerializeField] private PenController pen;
        private TextRevealController _reveal;

        void Awake() => _reveal = GetComponentInChildren<TextRevealController>();

        public override bool CanClickNow(GameState state) => state == GameState.DrawBoard;

        protected override void OnValidClick()
        {
            _reveal?.Reveal("hello!");
            pen.TriggerWrite();            // let the pen handle its own signals / audio
        }
    }
}