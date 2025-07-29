using Configs;
using UnityEngine;
using DG.Tweening;
using Managers;
using Zenject;

namespace Clickables {
    [RequireComponent(typeof(Collider))]
    [DefaultExecutionOrder(-100)]
    public class DecorativeBounce : MonoBehaviour {
        
        
        [SerializeField]  private BounceConfig cfg;
        [SerializeField] AudioSource  src;

        private Sequence       _seq;
        private IClickable     _clickable;
        private GameManager    _gameManager;
        private  bool           _hasClickable;

        private  void Awake() {
            _clickable = GetComponent<IClickable>();
            _hasClickable = _clickable != null;
        }

        private  void Start() {
            _gameManager = GameManager.Instance;
        }

        private void OnMouseDown() {
        
            if (_seq?.IsActive() == true) return;

            
            if (_hasClickable && _gameManager != null && 
                _clickable.CanClickNow(_gameManager.State))
                return;

           
            if (cfg.clip && src) {
                src.pitch = cfg.pitch;
                src.volume = cfg.volume;
                src.PlayOneShot(cfg.clip);
            }

          
            var jumpDur = cfg.jumpDuration;
            var posShakeDur = jumpDur * 0.9f;
            var rotShakeStart = jumpDur * 0.45f;
            var rotShakeDur = jumpDur * 0.5f;

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