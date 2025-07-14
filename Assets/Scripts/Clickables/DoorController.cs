using DG.Tweening;
using UnityEngine;

namespace Clickables
{
    public class DoorController : MonoBehaviour
    {
        [SerializeField] private Transform     door;
        [SerializeField] private float         openY   = 80f;
        [SerializeField] private  float         openDur = .6f;

        [SerializeField] private  BlinkingDot   dotPrefab;   
        [SerializeField] private Transform     dotAnchor;   

        private  BlinkingDot _dot;  

        private  void Awake() => CupController.OnCupDisposed += HandleCupDisposed;
        private  void OnDestroy() => CupController.OnCupDisposed -= HandleCupDisposed;

        private void HandleCupDisposed()
        {
        
            door
                .DORotate(new Vector3(0, openY, 0), openDur, RotateMode.LocalAxisAdd)
                .SetEase(Ease.OutCubic)
                .OnComplete(SpawnDot);
        }

        private  void SpawnDot()
        {
        
            if (_dot == null)
            {
                _dot = Instantiate(
                    dotPrefab,
                    dotAnchor.position,
                    dotAnchor.rotation,
                    parent: null      
                );
            }
            else
            {
           
                _dot.transform.SetParent(null, worldPositionStays: true);
                _dot.transform.SetPositionAndRotation(
                    dotAnchor.position,
                    dotAnchor.rotation
                );
            }

            _dot.gameObject.SetActive(true);
        }
    }
}