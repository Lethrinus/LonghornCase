using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Button leftBtn;
        [SerializeField] private Button rightBtn;

        void Awake()
        {
            leftBtn.onClick.AddListener(() => Debug.Log("Left button clicked"));
            rightBtn.onClick.AddListener(() => Debug.Log("Right button clicked"));
        }
    }
}