using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class ClickableBounce : MonoBehaviour
{
    [Header("Jump (Bounce) Settings")]
    [SerializeField] private float jumpPower     = 0.05f;
    [SerializeField] private float jumpDuration  = 0.3f;
    [SerializeField] private Ease  jumpEase      = Ease.OutQuad;

    [Header("Shake (Tremble) Settings")]
    [SerializeField] private Vector3 shakeStrength   = new Vector3(0.02f, 0.02f, 0.02f);
    [SerializeField] private int     shakeVibrato    = 8;
    [SerializeField] private float   shakeRandomness = 45f;
    [SerializeField] private Ease    shakeEase       = Ease.InOutSine;

    bool _busy;

    void OnMouseDown()
    {
        if (_busy) return;
        _busy = true;

        // append + plus join
        var seq = DOTween.Sequence();

        // bounce
        seq.Append(transform
            .DOJump(transform.position, jumpPower, 1, jumpDuration)
            .SetEase(jumpEase)
        );

        //synchron start bounce and shake 
        seq.Join(transform
            .DOShakePosition(
                jumpDuration,       
                shakeStrength,
                shakeVibrato,
                shakeRandomness
            )
            .SetEase(shakeEase)
        );

        seq.OnComplete(() => _busy = false);
    }
}