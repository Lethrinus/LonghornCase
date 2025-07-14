using Configs;
using UnityEngine;
using DG.Tweening;
using Managers;

namespace Clickables {
    [RequireComponent(typeof(Collider))]
    [DefaultExecutionOrder(-100)]
    public class DecorativeBounce : MonoBehaviour {
        [SerializeField] BounceConfig cfg;
        [SerializeField] AudioSource  src;

        Sequence       _seq;
        IClickable     _clickable;
        GameManager    _gameManager;
        bool           _hasClickable;

        void Awake() {
            _clickable = GetComponent<IClickable>();
            _hasClickable = _clickable != null;
        }

        void Start() {
            _gameManager = GameManager.Instance;
        }

        void OnMouseDown() {
        
            if (_seq?.IsActive() == true) return;

            
            if (_hasClickable && _gameManager != null && 
                _clickable.CanClickNow(_gameManager.State))
                return;

           
            if (cfg.clip && src) {
                src.pitch = cfg.pitch;
                src.volume = cfg.volume;
                src.PlayOneShot(cfg.clip);
            }

          
            float jumpDur = cfg.jumpDuration;
            float posShakeDur = jumpDur * 0.9f;
            float rotShakeStart = jumpDur * 0.45f;
            float rotShakeDur = jumpDur * 0.5f;

            _seq = DOTween.Sequence()
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .Append(transform.DOJump(transform.position, cfg.jumpPower, 1, jumpDur)
                    .SetEase(cfg.jumpEase))
                .Join(transform.DOShakePosition(posShakeDur, cfg.posShake, 
                    cfg.posVibrato, cfg.posRandomness, fadeOut: true)
                    .SetEase(cfg.commonEase))
                .Insert(rotShakeStart, transform.DOShakeRotation(rotShakeDur, 
                    cfg.rotShake, cfg.rotVibrato, cfg.rotRandomness, fadeOut: true)
                    .SetEase(cfg.commonEase));
        }
    }
}