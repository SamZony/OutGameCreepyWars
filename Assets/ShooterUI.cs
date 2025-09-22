using DG.Tweening;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShooterUI : MonoBehaviour
{
    public static ShooterUI Instance;

    public GameObject reticle;

    [Header("Ammo & Magazine")]
    public Image ammoFill;
    public Image magFill;

    [Header("Health")]
    public Image healthFill;

    [Header("Objective System")]
    public ObjectiveUISystem objectiveUI;

    [Header("Danger Indication")]
    public DangerIndication dangerIndicator;

    [Header("DamagePanel")]
    public DOTweenAnimation damagePanel;

    [Header("Prompts Manager")]
    public PromptsManager promptsManager;

    [Header("Level Fail/Win UI")]
    public GameObject failPanel;
    public GameObject winPanel;

    public static Action<RequirementPromptType> ShowRequirement;
    public static Action HideRequirement;

    public static Action ShowLootPrompt;
    public static Action HideLootPrompt;


    #region EnableDisable
    private void OnEnable()
    {
        ShowRequirement += (type) => promptsManager.ShowRequirementPrompt(type);
        HideRequirement += () => promptsManager.HideRequirementPrompt();

        ShowLootPrompt += () => promptsManager.ShowLootPrompt();
        HideLootPrompt += () => promptsManager.HideLootPrompt();

        DestroyableObject.showDangerIndication += (transform) =>  ToggleDangerIndication(transform, true);
        DestroyableObject.hideDangerIndication += () => ToggleDangerIndication(transform, false);
    }
    private void OnDisable()
    {
        ShowRequirement -= (type) => promptsManager.ShowRequirementPrompt(type);
        HideRequirement -= () => promptsManager.HideRequirementPrompt();

        ShowLootPrompt -= () => promptsManager.ShowLootPrompt();
        HideLootPrompt -= () => promptsManager.HideLootPrompt();

        DestroyableObject.showDangerIndication -= (transform) => ToggleDangerIndication(transform, true);
        DestroyableObject.hideDangerIndication -= () => ToggleDangerIndication(transform, false);
    }
    #endregion

    public void ShowLevelFailedUI(string reason)
    {
        failPanel.SetActive(true);
        failPanel.GetComponent<WinFailPanel>().FailReason(reason);
        SoundManager.Instance.PlayFailSound();
    }

    public void CloseLevelFailedUI()
    {
        failPanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void ToggleDangerIndication(Transform target, bool status)
    {
        dangerIndicator.gameObject.SetActive(status);
        dangerIndicator.target = target;
    }

    private void Awake()
    {
        Instance = this;
    }
}
