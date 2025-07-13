using Clickables;
using DG.Tweening;
using UnityEngine;


public class DoorController : MonoBehaviour
{
    [SerializeField] Transform     door;
    [SerializeField] float         openY  = 80f;
    [SerializeField] float         openDur = .6f;

    [SerializeField] BlinkingDot   dotPrefab;   
    [SerializeField] Transform     dotAnchor;   

    BlinkingDot _dot;                          

    void Awake() => CupController.OnCupDisposed += HandleCupDisposed;
    void OnDestroy() => CupController.OnCupDisposed -= HandleCupDisposed;

    void HandleCupDisposed()
    {
        
        door.DORotate(new Vector3(0, openY, 0), openDur, RotateMode.LocalAxisAdd)
            .SetEase(Ease.OutCubic);

        
        if (_dot == null)
            _dot = Instantiate(dotPrefab, dotAnchor.position, dotAnchor.rotation);
        else
            _dot.transform.SetPositionAndRotation(dotAnchor.position, dotAnchor.rotation);

        _dot.gameObject.SetActive(true);   
    }
}