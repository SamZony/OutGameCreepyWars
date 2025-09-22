using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;


public enum PlaceInHolster
{
    None, SideArm , Primary, Secondary, Grenade, BigGun
}
public class OutHolsterManager : MonoBehaviour
{
    public enum HolsterInstance
    {
        Player1, Player2
    }

    public Transform primaryWeaponPlace;
    public Transform secondaryWeaponPlace;

    public HolsterInstance holsterInstance;

    public List<Weapon> weapons;

    public static OutHolsterManager Instance1;

    public static OutHolsterManager Instance2;

    [Serializable]
    public struct Weapon
    {
        public GameObject weapon;
        public PlaceInHolster placeInHolster;
    }

    private void Awake()
    {
        if (OutGameManager.Instance.currentPlayer == CurrentPlayer.Player1) holsterInstance = HolsterInstance.Player1;
        else if (OutGameManager.Instance.currentPlayer == CurrentPlayer.Player2) holsterInstance = HolsterInstance.Player2;
        if (holsterInstance == HolsterInstance.Player1)
        {
            Instance1 = this;
            //Instance2 = null;
        }
        else if (holsterInstance == HolsterInstance.Player2)
        {
            Instance2 = this;
            //Instance1 = null;
        }
    }

    private void Update()
    {
        if (OutGameManager.Instance.currentPlayer == CurrentPlayer.Player1) holsterInstance = HolsterInstance.Player1;
        else if (OutGameManager.Instance.currentPlayer == CurrentPlayer.Player2) holsterInstance = HolsterInstance.Player2;

        if (holsterInstance == HolsterInstance.Player1)
        {
            Instance1 = this;
            //Instance2 = null;
        }
        else if (holsterInstance == HolsterInstance.Player2)
        {
            Instance2 = this;
            //Instance1 = null;
        }
    }

}
