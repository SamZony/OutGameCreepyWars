using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum RequirementPromptType {
    Key,
    Card
}
public class PromptsManager : MonoBehaviour
{
    [Header("Requirement Prompt")]
    public GameObject requirementPrompt;
    public Image requirement;

    [Space]
    public Sprite key;
    public Sprite card;


    [Header("Loot Prompt")]
    public GameObject lootPrompt;
    public TextMeshProUGUI controlKey;

    public string lootControlKey = "E";

    private void Start()
    {
        lootControlKey = PlayerPrefs.GetString("InteractKey", "E");
    }

    public void ShowRequirementPrompt(RequirementPromptType promptType)
    {
        requirementPrompt.SetActive(true);
        switch (promptType)
        {
            case RequirementPromptType.Key:
                requirement.sprite = key;

                break;
            case RequirementPromptType.Card:
                requirement.sprite = card;
                break;
        }
    }

    public void HideRequirementPrompt()
    {
        requirementPrompt.SetActive(false);
    }

    public void ShowLootPrompt()
    {
        lootPrompt.SetActive(true);
        controlKey.text = lootControlKey;
    }

    public void HideLootPrompt()
    {
        lootPrompt.SetActive(false);
    }
}
