using DG.Tweening;
using Managers;         
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BlinkingDot : MonoBehaviour
{
    [Header("Pulse")]
    [SerializeField] Vector3 scaleMin   = Vector3.one * 0.6f;
    [SerializeField] Vector3 scaleMax   = Vector3.one * 1.2f;
    [SerializeField] float   period     = 1.2f;          
    [SerializeField] Ease    ease       = Ease.InOutSine;

    private Tween _pulse;

    void OnEnable()
    {
      
        _pulse = transform
                      .DOScale(scaleMax, period * .5f)
                 .SetEase(ease)
                 .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    void OnDisable() => _pulse?.Kill();

    void OnMouseDown()                     
    {
        _pulse?.Kill();                      
       // GameManager.Instance.SetState(GameState.Completed);
        gameObject.SetActive(false);    
    }
}