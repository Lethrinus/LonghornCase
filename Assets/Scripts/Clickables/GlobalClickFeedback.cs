using UnityEngine;
using DG.Tweening;
using Managers;
using Clickables;

[DisallowMultipleComponent]
public class GlobalClickFeedback : MonoBehaviour
{
    [Header("Shake Settings")]
    [Tooltip("Pozisyon titremesi için güç (XYZ)")]
    public Vector3 shakePositionStrength = new Vector3(0.02f, 0.02f, 0.02f);
    [Tooltip("Süre (sn)")]
    public float   shakeDuration         = 0.2f;
    [Tooltip("Titreşim sayısı")]
    public int     shakeVibrato          = 8;
    [Tooltip("Rastgelelik (°)")]
    public float   shakeRandomness       = 45f;
    [Tooltip("Ease eğrisi")]
    public Ease    shakeEase             = Ease.InOutSine;

    Camera _cam;

    void Awake()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        var ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit)) return;

        var go = hit.collider.gameObject;
        var state = GameManager.Instance.State;

    
        if (go.TryGetComponent<IClickable>(out var clickable))
        {
            if (!clickable.CanClickNow(state))
            {
                
                hit.collider.transform
                    .DOShakePosition(shakeDuration, shakePositionStrength, shakeVibrato, shakeRandomness)
                    .SetEase(shakeEase);
            }
            
        }
        else
        {
            
        }
    }
}