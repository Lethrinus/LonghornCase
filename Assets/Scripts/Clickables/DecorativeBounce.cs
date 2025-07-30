// Assets/Scripts/Clickables/DecorativeBounce.cs
using Configs;
using DG.Tweening;
using Infrastructure.Signals;
using Managers;
using UnityEngine;
using Zenject;

namespace Clickables
{
    [RequireComponent(typeof(Collider))]
    [DefaultExecutionOrder(-100)]
    public class DecorativeBounce : MonoBehaviour, IClickable
    {
        [SerializeField] private BounceConfig cfg;

        [Inject] private SignalBus _bus;

        IClickable _flowClickable;   // aynı GO’daki gerçek akış click’ı
        Sequence   _seq;

        private static bool sGlobalBounceLock;
        /* ---------------------------------------------------------------- */
        void Awake()
        {
            // Kendi bileşenimizi hariç tutarak mevcut IClickable’ı bul
            foreach (var c in GetComponents<IClickable>())
                if (c != (IClickable)this)
                {
                    _flowClickable = c;
                    break;
                }
        }

        /* ---------------------------------------------------------------- */
        // Dekor objeleri oyunun her anında tıklanabilir
        public bool CanClickNow(GameState _) => true;

        void OnMouseDown()
        {
            if (sGlobalBounceLock) return;
            if (_seq?.IsActive() == true) return;          // spam koruması

            /* 1️⃣  Eğer bu obje akışta aktifse veya HoverCancel durumundaysa
                   bounce iptal edilir                                        */
            if (_flowClickable != null &&
                _flowClickable.CanClickNow(GameManager.Instance.State))
                return;

            /* 2️⃣  Ses efekti */
            if (cfg.clip)
                _bus.Fire(new PlaySfxSignal(cfg.clip, cfg.volume, cfg.pitch));
            sGlobalBounceLock = true;
            /* 3️⃣  Animasyon: dünya zıplaması + relatif shake */
            Vector3 worldAnchor = transform.position;

            float jumpDur     = cfg.jumpDuration;
            float posShakeDur = jumpDur * .9f;
            float rotStart    = jumpDur * .45f;
            float rotDur      = jumpDur * .5f;

            _seq = DOTween.Sequence()
                          .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                          // Küçük dünya zıplaması (parent scale etkisiz)
                          .Append(transform.DOJump(worldAnchor, cfg.jumpPower, 1, jumpDur)
                                           .SetEase(cfg.jumpEase))
                          // localPosition etrafında shake
                          .Join(transform.DOShakePosition(posShakeDur,
                                   cfg.posShake, cfg.posVibrato, cfg.posRandomness,
                                   snapping:false, fadeOut:true)
                                   .SetRelative()
                                   .SetEase(cfg.commonEase))
                          // localRotation shake
                          .Insert(rotStart, transform.DOShakeRotation(rotDur,
                                   cfg.rotShake, cfg.rotVibrato, cfg.rotRandomness, true)
                                   .SetEase(cfg.commonEase))
                          .OnComplete(() => sGlobalBounceLock = false)  
                          .OnKill    (() => sGlobalBounceLock = false);
            
        }
    }
}
