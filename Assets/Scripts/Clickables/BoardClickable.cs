using UnityEngine;

namespace Clickables
{
    [RequireComponent(typeof(Collider))]
    public class BoardClickable : MonoBehaviour
    {
        
        [SerializeField] private PenController pen;

        void OnMouseDown()
        {
            pen?.TriggerWrite();
        }
    }
}