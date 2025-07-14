using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject levelCompletedPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button mainMuteButton;
        [SerializeField] private Button mainExitButton;

        [Header("Pause Menu Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseMuteButton;
        [SerializeField] private Button pauseExitButton;

        [Header("Level Completed Buttons")]
        [SerializeField] private Button replayButton;
        [SerializeField] private Button levelMainMenuButton;

        [Header("Sound Icons")]
        [SerializeField] private Sprite soundOnIcon;
        [SerializeField] private Sprite soundOffIcon;

        private bool _isMuted;
        private Image _mainMuteImage, _pauseMuteImage;

        private void Start()
        {
      
            mainMenuPanel.SetActive(true);
            pauseMenuPanel.SetActive(false);
            levelCompletedPanel.SetActive(false);
            Time.timeScale = 0f;

     
            playButton.onClick.AddListener(OnPlay);
            mainMuteButton.onClick.AddListener(OnMuteToggle);
            mainExitButton.onClick.AddListener(OnExit);

            resumeButton.onClick.AddListener(OnResume);
            pauseMuteButton.onClick.AddListener(OnMuteToggle);
            pauseExitButton.onClick.AddListener(OnExit);

            replayButton.onClick.AddListener(OnReplay);
            levelMainMenuButton.onClick.AddListener(OnLevelMainMenu);

        
            _mainMuteImage  = mainMuteButton.GetComponent<Image>();
            _pauseMuteImage = pauseMuteButton.GetComponent<Image>();
            UpdateMuteIcons();
        }

        private void Update()
        {
        
            if (!mainMenuPanel.activeSelf
                && !levelCompletedPanel.activeSelf
                && Input.GetKeyDown(KeyCode.Escape))
            {
                if (pauseMenuPanel.activeSelf) OnResume();
                else ShowPauseMenu();
            }

        
            if (!mainMenuPanel.activeSelf
                && !pauseMenuPanel.activeSelf
                && !levelCompletedPanel.activeSelf
                && GameManager.Instance.State == GameState.Completed)
            {
                ShowLevelCompletedMenu();
            }
        }


        private void OnPlay()
        {
            mainMenuPanel.SetActive(false);
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }

        private void OnResume()
        {
            pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f;
            
            AudioListener.pause = false;
        }

        private void ShowPauseMenu()
        {
            pauseMenuPanel.SetActive(true);
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }

        private void OnMuteToggle()
        {
            _isMuted = !_isMuted;
            AudioListener.pause = _isMuted;
            UpdateMuteIcons();
        }

        private void UpdateMuteIcons()
        {
            _mainMuteImage.sprite  = _isMuted ? soundOffIcon : soundOnIcon;
            _pauseMuteImage.sprite = _isMuted ? soundOffIcon : soundOnIcon;
        }

        private void OnExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        private static void OnReplay()
        {
            AudioListener.pause = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnLevelMainMenu()
        {
       
            levelCompletedPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            AudioListener.pause = false;
            GameManager.Instance.GotoClickPen();
        }


        private void ShowLevelCompletedMenu()
        {
            levelCompletedPanel.SetActive(true);
            Time.timeScale = 0f;

            AudioListener.pause = true;
        }
    }
}
