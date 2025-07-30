
using Configs;
using Infrastructure.Signals;
using Managers;
using UnityEngine;
using Zenject;

namespace Clickables
{
    [RequireComponent(typeof(Collider))]
    public class ClickableSfx : ClickableBase
    {
        [SerializeField] BounceConfig cfg;
        
        [Inject] SignalBus _bus;

        public override bool CanClickNow(GameState state) => true;

        protected override void OnValidClick()
        {
            if (cfg != null && cfg.clip != null && _bus != null)
                _bus.Fire(new PlaySfxSignal(cfg.clip, cfg.volume, cfg.pitch));
        }
    }
}