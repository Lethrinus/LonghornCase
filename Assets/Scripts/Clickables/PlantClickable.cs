// Assets/Scripts/Clickables/PlantClickable.cs
using UnityEngine;
using DG.Tweening;
using Core;
using Managers;

namespace Clickables {
    [DisallowMultipleComponent]
    public class PlantClickable : ClickableBase
    {
        public override bool CanClickNow(GameState state)
            => state == GameState.ClickPlant;

        protected override void OnValidClick()
        {
          
            transform.DOPunchScale(Vector3.one * .1f, .5f, 5, 1f)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => EventBus.Publish(new PlantClickedEvent()));
        }
    }
}