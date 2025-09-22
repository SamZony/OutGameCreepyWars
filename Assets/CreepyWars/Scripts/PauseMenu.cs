using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    MainUICanvas mainUICanvas;

    [Header("Buttons")]
    public Button resumeButton;
    public Button optionsButton, exitButton;

    [Header("Panels")]
    public GameObject optionsPanel;
    public GameObject exitPanel;

    [HideInInspector]
    public GameObject currentPanelActive;

    private void Start()
    {
        mainUICanvas = MainUICanvas.Instance;
        mainUICanvas.pauseMenuStep = 0;

        AddListenersToMainButtons();
    }

    private void Update()
    {
        if (!currentPanelActive.activeSelf) currentPanelActive = null;
    }

    void AddListenersToMainButtons()
    {
        resumeButton.onClick.AddListener(() =>
        {
            mainUICanvas.PauseMenuPanel.gameObject.SetActive(false);
        });

        optionsButton.onClick.AddListener(() =>
        {
            optionsPanel.SetActive(true);
            optionsPanel.GetComponent<DOTweenAnimation>().DORestart();
            mainUICanvas.pauseMenuStep++;
        });
    }
}
