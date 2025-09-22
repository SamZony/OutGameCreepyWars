using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace CreepyWars
{
    public class MissionSelectMenu : MonoBehaviour
    {
        public List<Button> missionButtons = new List<Button>();
        public Button startButton;

        public string gameplaySceneName;

        private void Start()
        {
            for (int i = 0; i < missionButtons.Count; i++)
            {
                int index = i; // capture the index for closure
                missionButtons[i].onClick.AddListener(() =>
                {
                    PlayerPrefs.SetInt("SelectedMission", index);
                });
            }


            startButton.onClick.AddListener(() => {
                SceneManager.LoadScene(gameplaySceneName);
                startButton.transform.parent.parent.gameObject.SetActive(false);
            });
        }
    }

    public static class SaveKeys
    {

    }
}

