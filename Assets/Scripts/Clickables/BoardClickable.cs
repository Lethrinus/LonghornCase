using UnityEngine;
using DG.Tweening;
using Core;
using Managers;

namespace Clickables
{
    [RequireComponent(typeof(MeshRenderer))]
    public class BoardClickable : ClickableBase
    {
        [SerializeField] PenController pen;
        MeshRenderer _r;
        TextRevealController _reveal;

        void Awake()
        {
            _r = GetComponent<MeshRenderer>();
            _reveal = GetComponentInChildren<TextRevealController>();
        }

        public override bool CanClickNow(GameState state)
            => state == GameState.DrawBoard;

        protected override void OnValidClick()
        {
            _r.material.DOColor(Color.black, .5f).SetEase(Ease.InOutQuad);
            pen.TriggerWrite();
            _reveal?.Reveal("hello!");
        }
    }
}
