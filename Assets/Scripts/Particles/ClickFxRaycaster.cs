using UnityEngine;
using UnityEngine.EventSystems;

namespace Particles
{
    public class ClickFxRaycaster : MonoBehaviour
    {
        [SerializeField] private CartoonClickFxPool fxPool;
        [SerializeField] private LayerMask clickableMask = ~0;
        [SerializeField] private float maxDistance = 100f;
        [SerializeField] private float surfaceOffset = 0.02f;
        [SerializeField] private float cameraExtraPush = 2f;

        private readonly RaycastHit[] _hits = new RaycastHit[1];
        private Camera _cam;
        private float _lastClickTime;
        private const float MinClickInterval= 0.05f;

        private void Awake()
        {
            if (fxPool == null)
                fxPool = FindObjectOfType<CartoonClickFxPool>(includeInactive: true);
        }

        private void Start()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;


            var currentTime = Time.time;
            if (currentTime - _lastClickTime < MinClickInterval) return;
            _lastClickTime = currentTime;

            if (!_cam || fxPool == null) return;

            PerformRaycast();
        }

        private void PerformRaycast()
        {

            var overUI = EventSystem.current?.IsPointerOverGameObject(-1) ?? false;
            if (overUI) return;

            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            var hitCount = Physics.RaycastNonAlloc(ray, _hits, maxDistance, clickableMask);

            if (hitCount == 0) return;

            var viewDir = -ray.direction;
            var pos = _hits[0].point + viewDir * (surfaceOffset + cameraExtraPush);
            var faceCamera = Quaternion.LookRotation(viewDir);
            fxPool.Play(pos, faceCamera);
        }
    }
}