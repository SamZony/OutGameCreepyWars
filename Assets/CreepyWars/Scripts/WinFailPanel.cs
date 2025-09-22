using DG.Tweening;
using TMPro;
using UnityEngine;
using System.Collections;

public class WinFailPanel : MonoBehaviour
{
    public DOTweenAnimation title;
    public string descriptionText;
    public TMP_Text description;

    public bool restartTextAnims;

    private void OnEnable()
    {
       Invoke(nameof(DoDescribe), 0.1f);
    }

    IEnumerator TypeText(string fullText, float totalDuration)
    {
        float delay = totalDuration / Mathf.Max(fullText.Length, 0.5f);

        description.text = "";
        for (int i = 0; i < fullText.Length; i++)
        {
            description.text += fullText[i];
            yield return new WaitForSeconds(delay);
        }
    }

    void DoDescribe()
    {
        DOTweenAnimation descAnim = description.GetComponent<DOTweenAnimation>();
        if (restartTextAnims)
        {
            title.DORestart();
            descAnim.DORestart();
        }
        // Wait for tween to finish, then start typing
        descAnim.tween.OnComplete(() =>
        {
            StartCoroutine(TypeText(descriptionText, descAnim.duration));
        });
    }

    public void FailReason(string reason)
    {
        descriptionText = reason;
        description.text = descriptionText;
    }

    public void RestartGame()
    {

    }
}
