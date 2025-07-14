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

        private void Start()
        {
            _cam = Camera.main;
        }

        void Awake()
        {
            if (fxPool == null)
                fxPool = FindObjectOfType<CartoonClickFxPool>(includeInactive: true);
        }

        void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            if (!_cam || fxPool is null) return;

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            int hitCount = Physics.RaycastNonAlloc(ray, hits, maxDistance, clickableMask);

            bool overUI = EventSystem.current is not null &&
                          EventSystem.current.IsPointerOverGameObject(-1);

            if (overUI && hitCount == 0) return;
            if (hitCount == 0) return;
        
        
        
            Vector3 viewDir = -ray.direction.normalized;
            Vector3 pos = hits[0].point + viewDir * (surfaceOffset + cameraExtraPush);
            Quaternion faceCamera = Quaternion.LookRotation(viewDir);
            fxPool.Play(pos, faceCamera);
        }
    }
}