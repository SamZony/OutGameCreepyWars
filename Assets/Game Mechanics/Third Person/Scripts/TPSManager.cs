using Unity.VisualScripting;
using UnityEngine;

public class TPSManager : MonoBehaviour
{
    private static TPSManager _instance;

    [Header("Navigation")]
    public KeyCode Forward = KeyCode.W;
    public KeyCode Backward = KeyCode.S;
    public KeyCode Left = KeyCode.A;
    public KeyCode Right = KeyCode.D;
    public KeyCode Sprint = KeyCode.LeftShift;

    [Header("Actions")]
    public KeyCode Action = KeyCode.E;
    public KeyCode Use = KeyCode.F;


    [Header("Weapons")]
    public KeyCode Holster = KeyCode.Q;
    public KeyCode PrimaryWeaponSwitch = KeyCode.Alpha1;
    public KeyCode SecondaryWeaponSwitch = KeyCode.Alpha2;
    public KeyCode TertiaryWeaponSwitch = KeyCode.Alpha3;
    public KeyCode ThrowablesSwitch = KeyCode.Alpha4;

    [Header("Equipments")]
    public KeyCode Binoculars = KeyCode.B;
    public KeyCode SpecializedPhone = KeyCode.UpArrow;

    [Header("UI")]
    public KeyCode PauseGame = KeyCode.Escape;
    public KeyCode WeaponSwitch = KeyCode.Tab;
    public MouseButton WeaponSwitch2 = MouseButton.Middle;


    public static TPSManager Instance
    {

        get
        {
            if (_instance == null)
            {
                _instance = new GameObject().AddComponent<TPSManager>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public static void OnMove()
    {

    }
}
