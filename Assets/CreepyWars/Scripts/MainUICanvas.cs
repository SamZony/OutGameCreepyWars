using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainUICanvas : MonoBehaviour
{
    public static MainUICanvas Instance;
    [Header("Main Menu Buttons")]
    public MainMenuButtons mainMenuButtons;


    [Header("Pause Menu Buttons")]
    public PauseMenuButtons pauseMenuButtons;

    [Header("Pause Menu Panels")]
    public PauseMenu PauseMenuPanel;

    [Header("Common")]

    public GameObject optionsPanel;

    InputSystem_Actions inputActions;
    InputAction togglePause;

    [HideInInspector]
    public int pauseMenuStep;
    [HideInInspector]
    public int mainMenuStep;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        inputActions = new InputSystem_Actions();
        togglePause = inputActions.UI.Cancel;
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;

        togglePause.Enable();
        togglePause.performed += TogglePauseMenu;
    }

    private void TogglePauseMenu(InputAction.CallbackContext context)
    {
        if (PauseMenuPanel.gameObject.activeSelf == true)
        {
            if (pauseMenuStep == 0)
            {
                PauseMenuPanel.gameObject.SetActive(false);
            }
            else
            {
                if (pauseMenuStep > 0 && PauseMenuPanel.currentPanelActive != null)
                {
                    PauseMenuPanel.currentPanelActive.SetActive(false);
                    pauseMenuStep--;
                }
            }
        }
        else
        {
            PauseMenuPanel.gameObject.SetActive(true);
            PauseMenuPanel.GetComponent<DOTweenAnimation>().DORestart();
        }
    }

    private void OnSceneChanged(Scene arg0, Scene arg1)
    {
        if (arg1 != SceneManager.GetSceneByName("MainMenu"))
        {
            mainMenuButtons.playButton.transform.parent.gameObject.SetActive(false);
        }
        else mainMenuButtons.playButton.transform.parent.gameObject.SetActive(true);
    }

    private void Start()
    {
        if (mainMenuButtons.optionsButton) mainMenuButtons.optionsButton.onClick.AddListener(() => optionsPanel.SetActive(true));
        if (pauseMenuButtons.optionsButton) pauseMenuButtons.optionsButton.onClick.AddListener(() => optionsPanel.SetActive(true));
        if (mainMenuButtons.exitButton) mainMenuButtons.exitButton.onClick.AddListener(ExitApp);
        if (pauseMenuButtons.exitButton) pauseMenuButtons.exitButton.onClick.AddListener(ExitApp);
    }
    public void ExitApp()
    {
        Application.Quit();
    }

    public void ChangeSceneAsync(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OnDisable()
    {
        togglePause.performed += TogglePauseMenu;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////

    [Serializable]
    public struct MainMenuButtons
    {
        public Button playButton, optionsButton, exitButton;

    }

    [Serializable]
    public struct PauseMenuButtons
    {
        public Button resumeButton, optionsButton, mainMenuButton, exitButton;
    }
}
