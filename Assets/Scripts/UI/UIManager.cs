using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Managers;  

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject pauseMenuPanel;
    [SerializeField] GameObject levelCompletedPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] Button playButton;
    [SerializeField] Button mainMuteButton;
    [SerializeField] Button mainExitButton;

    [Header("Pause Menu Buttons")]
    [SerializeField] Button resumeButton;
    [SerializeField] Button pauseMuteButton;
    [SerializeField] Button pauseExitButton;

    [Header("Level Completed Buttons")]
    [SerializeField] Button replayButton;
    [SerializeField] Button levelMainMenuButton;

    [Header("Sound Icons")]
    [SerializeField] Sprite soundOnIcon;
    [SerializeField] Sprite soundOffIcon;

    bool isMuted = false;
    Image mainMuteImage, pauseMuteImage;

    void Start()
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

        
        mainMuteImage  = mainMuteButton.GetComponent<Image>();
        pauseMuteImage = pauseMuteButton.GetComponent<Image>();
        UpdateMuteIcons();
    }

    void Update()
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

  

    void OnPlay()
    {
        mainMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void OnResume()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void ShowPauseMenu()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void OnMuteToggle()
    {
        isMuted = !isMuted;
        AudioListener.pause = isMuted;
        UpdateMuteIcons();
    }

    void UpdateMuteIcons()
    {
        mainMuteImage.sprite  = isMuted ? soundOffIcon : soundOnIcon;
        pauseMuteImage.sprite = isMuted ? soundOffIcon : soundOnIcon;
    }

    void OnExit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    void OnReplay()
    {
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnLevelMainMenu()
    {
       
        levelCompletedPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        
        GameManager.Instance.GotoClickPen();
    }

    

    void ShowLevelCompletedMenu()
    {
        levelCompletedPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
