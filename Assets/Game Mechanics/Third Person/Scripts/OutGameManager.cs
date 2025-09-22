using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using static OutHolsterManager;

public enum CurrentPlayer
{
    Player1, Player2
}
public class OutGameManager : MonoBehaviour
{
    public static OutGameManager Instance;

    [Header("Missions Config")]
    public List<Mission> missionsList;
    public Mission currentMission;

    public CurrentPlayer currentPlayer = CurrentPlayer.Player1;

    [Header("Players")]
    public GameObject p1;
    public GameObject p2;
    public Transform currentP;

    [Header("Impacts Config")]
    public List<BulletImpact> bulletImpacts;


    InputSystem_Actions action;
    InputAction switchPlayer;

    [Header("CurrentWeapon")]

    public GameObject currentWeapon;
    public OutVehicleController currentVehicle;
    public ThrowableObject currentPickableObject, ownedThrowableObject;


    [Space]
    public LayerMask enemyLayer;
    void Awake()
    {
        Instance = this;

        action = new InputSystem_Actions();
        switchPlayer = action.Player.SwitchPlayer;

        if (p1 && p2)
            if (currentPlayer == CurrentPlayer.Player1)
            {
                p1.tag = "Player";
                p2.tag = "AI";

            }
            else
            {
                p2.tag = "Player";
                p1.tag = "AI";
            }
        currentP = GameObject.FindWithTag("Player").transform;

        
    }

    private void OnEnable()
    {
        switchPlayer.Enable();
        switchPlayer.performed += SwitchPlayer;
        switchPlayer.canceled += SwitchPlayer;
    }

    private void Start()
    {
        for (int i = 0; i < missionsList.Count; i++)
        {
            if (missionsList.IndexOf(missionsList[i]) == PlayerPrefs.GetInt("MissionNo"))
            {
                currentMission = missionsList[i];
                break;
            }
        }
        for (int i = 0; i < currentMission.objectives.Count; i++)
        {
            if (currentMission.objectives.IndexOf(currentMission.objectives[i]) == PlayerPrefs.GetInt("ObjectiveNo"))
            {
                UpdateObjective(currentMission.objectives[i].title, currentMission.objectives[i].description, 0.5f, currentMission.objectives[i].location,
                    currentMission.objectives[i].type);

            }
        }

        Time.timeScale = 1;
    }

    private void OnDisable()
    {
        switchPlayer.Disable();
        switchPlayer.performed -= SwitchPlayer;
        switchPlayer.canceled -= SwitchPlayer;
    }

    private void SwitchPlayer(InputAction.CallbackContext context)
    {
        if (currentPlayer == CurrentPlayer.Player1)
        {
            currentPlayer = CurrentPlayer.Player2;
            p2.tag = "Player";
            p1.tag = "AI";
        }
        else
        {
            currentPlayer = CurrentPlayer.Player1;
            p1.tag = "Player";
            p2.tag = "AI";
        }
        currentP = GameObject.FindWithTag("Player").transform;
    }

    public void UpdateObjective(string title, string description, float duration, Transform position, ObjectiveType type)
    {
        ShooterUI.Instance.objectiveUI.ChangeObjective(title, description, duration, position, type);
    }

    public void LevelFailed(string reason)
    {
        ShooterUI.Instance.ShowLevelFailedUI(reason);
        Time.timeScale = 0.4f;
    }

    public void RestartMission()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartFromCheckpoint()
    {
        ShooterUI.Instance.CloseLevelFailedUI();
        currentP.GetComponent<RespawnPlayer>().RestartFromCheckpoint();
    }

    //Indent for methods


    [Serializable]
    public struct Mission
    {
        public string missionName;
        public string missionDescription;
        public List<Objective> objectives;
    }

    

    [Serializable]
    public struct BulletImpact
    {
        public Texture surfaceTexture;
        public ParticleSystem impactEffect;
        public AudioClip impactSound;    
    }



}

[Serializable]

public struct Objective
{
    public string title;
    public string description;
    public Transform location;
    public AudioClip briefAudio;
    public ObjectiveType type;
}

public interface IDamageable
{
    float Health { get; set; }
    void TakeDamage(float amount);
}

