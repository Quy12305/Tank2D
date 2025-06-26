using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum TankState
{
    NotBuy,
    NotEquip,
    Equipped,
}

public class TankManager : Singleton<TankManager>
{
    public                   int              currentTankIndex = 0;
    public                   float            currentHealth;
    public                   float            currentSpeed;
    public                   float            currentDamage;
    public List<PlayerData> playerDataList;

    public Dictionary<int, TankState> TankStateByIndex = new Dictionary<int, TankState>
    {
        { 0, TankState.Equipped },
        { 1, TankState.NotBuy },
        { 2, TankState.NotBuy },
        { 3, TankState.NotBuy },
    };

    private void Start()
    {
        SaveLoadManager.Instance.LoadGame();
        DOVirtual.DelayedCall(0.5f, () =>
        {
            this.currentHealth = playerDataList[this.currentTankIndex].health;
            this.currentSpeed  = playerDataList[this.currentTankIndex].speed;
            this.currentDamage = playerDataList[this.currentTankIndex].damage;
        });
    }

    [System.Serializable]
    public class StateTank
    {
        public int tankIndex;
        public TankState  tankState;
    }

    [System.Serializable]
    public class TankStateData
    {
        public int         tankIndex;
        public StateTank[] tankStateByIndex;
    }

    public TankStateData GetData()
    {
        var tankStates = new StateTank[TankStateByIndex.Count];
        int i          = 0;
        foreach (var pair in TankStateByIndex)
        {
            tankStates[i++] = new StateTank
            {
                tankIndex = pair.Key,
                tankState = pair.Value
            };
        }

        return new TankStateData
        {
            tankIndex        = this.currentTankIndex,
            tankStateByIndex = tankStates
        };
    }

    public void LoadFromData(TankStateData data)
    {
        this.currentTankIndex = data.tankIndex;

        TankStateByIndex.Clear();
        foreach (var item in data.tankStateByIndex)
        {
            TankStateByIndex[item.tankIndex] = item.tankState;
        }
    }
}