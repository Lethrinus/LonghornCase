using UnityEngine;
using DG.Tweening;
using Managers;

namespace Clickables
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class DecorativeBounce : MonoBehaviour
    {
        [Header("Jump + Punch")]
        [SerializeField] float   jumpPower     = 0.08f;
        [SerializeField] float   jumpDuration  = 0.35f;
        [SerializeField] Ease    jumpEase      = Ease.OutCubic;

        [SerializeField] Vector3 posShake      = new (.01f, .01f, .01f);
        [SerializeField] int     posVibrato    = 10;
        [SerializeField] float   posRandomness = 60f;

        [SerializeField] Vector3 rotShake      = new (4f, 2f, 4f);
        [SerializeField] int     rotVibrato    = 12;
        [SerializeField] float   rotRandomness = 45f;
        [SerializeField] Ease    commonEase    = Ease.InOutSine;

        Sequence _seq;

        void OnMouseDown()
        {
        
            bool validClick = false;
            if (TryGetComponent<IClickable>(out var ic))
                validClick = ic.CanClickNow(GameManager.Instance.State);

     
            if (validClick) return;

          
            if (_seq != null && _seq.IsActive()) return; 

            _seq?.Kill();
            _seq = DOTween.Sequence()
                          .SetLink(gameObject, LinkBehaviour.KillOnDisable);

            _seq.Append(
                transform.DOJump(transform.position, jumpPower, 1, jumpDuration)
                        .SetEase(jumpEase));

            _seq.Join(
                transform.DOShakePosition(
                    jumpDuration * .9f, posShake,
                    posVibrato, posRandomness, fadeOut:true)
                        .SetEase(commonEase));

            _seq.Insert(
                jumpDuration * .45f,
                transform.DOShakeRotation(
                    jumpDuration * .5f, rotShake,
                    rotVibrato, rotRandomness, fadeOut:true)
                        .SetEase(commonEase));
        }
    }
}
