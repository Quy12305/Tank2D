using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public                   GameObject map;
    [SerializeField] private GameObject tankManager;
    [SerializeField] private GameObject boosterManager;

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

        if (this.tankManager != null)
        {
            DestroyAllChildren(this.tankManager.transform);
        }

        if (this.boosterManager != null)
        {
            DestroyAllChildren(this.boosterManager.transform);
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

        var joystick = FindObjectOfType<VariableJoystick>();
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