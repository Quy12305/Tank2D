using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public GameObject map;
    public GameObject BotAndPlayer;

    public bool CheckWin()
    {
        if (this.BotInMap() == 0)
        {
            return true;
        }
        return false;
    }

    public int BotInMap()
    {
        BotTank[] botTanks = FindObjectsOfType<BotTank>();
        return botTanks.Length;
    }

    public void DeleteAllData()
    {
        if (map != null)
        {
            DestroyAllChildren(map.transform);
        }

        if (BotAndPlayer != null)
        {
            DestroyAllChildren(BotAndPlayer.transform);
        }

        var bullets = FindObjectsOfType<Bullet>();
        var pool    = FindObjectOfType<ObjectPool>();
        if (pool != null)
        {
            foreach (var bullet in bullets)
            {
                pool.ReturnObject(bullet.gameObject);
            }
        }

        var joystick = FindObjectOfType<FloatingJoystick>();
        if (joystick != null)
        {
            joystick.OnPointerUp(null);
        }
    }

    private void DestroyAllChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
}