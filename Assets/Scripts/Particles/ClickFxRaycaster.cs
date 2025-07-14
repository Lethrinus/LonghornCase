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

        readonly RaycastHit[] hits = new RaycastHit[1];
        private Camera _cam;
        private float _lastClickTime;
        private const float MIN_CLICK_INTERVAL = 0.05f;

        void Awake()
        {
            if (fxPool == null)
                fxPool = FindObjectOfType<CartoonClickFxPool>(includeInactive: true);
        }

        private void Start()
        {
            _cam = Camera.main;
        }

        void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;


            float currentTime = Time.time;
            if (currentTime - _lastClickTime < MIN_CLICK_INTERVAL) return;
            _lastClickTime = currentTime;

            if (!_cam || fxPool == null) return;

            PerformRaycast();
        }

        void PerformRaycast()
        {

            bool overUI = EventSystem.current?.IsPointerOverGameObject(-1) ?? false;
            if (overUI) return;

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            int hitCount = Physics.RaycastNonAlloc(ray, hits, maxDistance, clickableMask);

            if (hitCount == 0) return;

            Vector3 viewDir = -ray.direction;
            Vector3 pos = hits[0].point + viewDir * (surfaceOffset + cameraExtraPush);
            Quaternion faceCamera = Quaternion.LookRotation(viewDir);
            fxPool.Play(pos, faceCamera);
        }
    }
}