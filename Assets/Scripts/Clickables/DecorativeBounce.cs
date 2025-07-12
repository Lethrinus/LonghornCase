using UnityEngine;
using DG.Tweening;
using Managers;

namespace Clickables
{
   
    [RequireComponent(typeof(Collider))]
    public class DecorativeBounce : MonoBehaviour
    {
        [SerializeField] BounceConfig cfg;
        [SerializeField] AudioSource  src; 

        Sequence _seq;

        void OnMouseDown()
        {
            
            if (TryGetComponent<IClickable>(out var ic) &&
                ic.CanClickNow(GameManager.Instance.State)) return;
            
            if (_seq != null && _seq.IsActive()) return;
            
            if (cfg.clip)
            {
                src.pitch  = cfg.pitch;
                src.volume = cfg.volume;
                src.PlayOneShot(cfg.clip);
            }
            
            _seq?.Kill();
            _seq = DOTween.Sequence()
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);

            _seq.Append(
                transform.DOJump(transform.position,
                        cfg.jumpPower, 1, cfg.jumpDuration)
                    .SetEase(cfg.jumpEase));

            _seq.Join(
                transform.DOShakePosition(cfg.jumpDuration * .9f,
                        cfg.posShake,
                        cfg.posVibrato,
                        cfg.posRandomness,
                        fadeOut: true)
                    .SetEase(cfg.commonEase));

            _seq.Insert(
                cfg.jumpDuration * .45f,
                transform.DOShakeRotation(cfg.jumpDuration * .5f,
                        cfg.rotShake,
                        cfg.rotVibrato,
                        cfg.rotRandomness,
                        fadeOut: true)
                    .SetEase(cfg.commonEase));
            
        }
        
        
    }
}
