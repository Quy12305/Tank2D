using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField] private GameObject       tankImg;
    [SerializeField] private TMP_Text         speedText;
    [SerializeField] private TMP_Text         healthText;
    [SerializeField] private TMP_Text         damageText;
    [SerializeField] private TMP_Text         costText;
    [SerializeField] private GameObject       buyButton;
    [SerializeField] private GameObject       equipButton;
    [SerializeField] private GameObject       equippedButton;

    private int currentIndex = 0;

    private void Start()
    {
        SetData();
    }

    public void Next()
    {
        SoundManager.Instance.OnClickButton();

        currentIndex++;
        if (currentIndex >= TankManager.Instance.playerDataList.Count)
        {
            currentIndex = 0;
        }

        SetData();
    }

    public void Back()
    {
        SoundManager.Instance.OnClickButton();

        if (currentIndex > 0)
        {
            currentIndex--;
        }
        else
        {
            currentIndex = TankManager.Instance.playerDataList.Count - 1;
        }

        SetData();
    }

    public void Buy()
    {
        SoundManager.Instance.OnClickButton();

        if (Coin.Instance.coinCount >= TankManager.Instance.playerDataList[currentIndex].cost)
        {
            Coin.Instance.AddCoin(-TankManager.Instance.playerDataList[currentIndex].cost);
            TankManager.Instance.TankStateByIndex[currentIndex] = TankState.NotEquip;
        }
        else
        {
            Debug.Log("Not enough coins to buy this item.");
        }

        SetData();
        SaveLoadManager.Instance.SaveGame();
    }

    public void Equip()
    {
        SoundManager.Instance.OnClickButton();

        for (int i = 0; i < TankManager.Instance.TankStateByIndex.Count; i++)
        {
            if (TankManager.Instance.TankStateByIndex[i] == TankState.Equipped)
            {
                TankManager.Instance.TankStateByIndex[i] = TankState.NotEquip;
                break;
            }
        }

        TankManager.Instance.TankStateByIndex[currentIndex] = TankState.Equipped;

        TankManager.Instance.currentTankIndex = currentIndex;
        TankManager.Instance.currentSpeed     = TankManager.Instance.playerDataList[currentIndex].speed;
        TankManager.Instance.currentHealth    = TankManager.Instance.playerDataList[currentIndex].health;
        TankManager.Instance.currentDamage    = TankManager.Instance.playerDataList[currentIndex].damage;

        SetData();
        SaveLoadManager.Instance.SaveGame();
    }

    public void SetData()
    {
        ChangeButton();
        this.tankImg.GetComponent<Image>().sprite = TankManager.Instance.playerDataList[currentIndex].tankImg;
        this.speedText.text                       = TankManager.Instance.playerDataList[currentIndex].speed.ToString();
        this.healthText.text                      = TankManager.Instance.playerDataList[currentIndex].health.ToString();
        this.damageText.text                      = TankManager.Instance.playerDataList[currentIndex].damage.ToString();
        this.costText.text                        = TankManager.Instance.playerDataList[currentIndex].cost.ToString();
    }

    public void ChangeButton()
    {
        if (TankManager.Instance.TankStateByIndex[currentIndex] == TankState.NotBuy)
        {
            this.buyButton.SetActive(true);
            this.equipButton.SetActive(false);
            this.equippedButton.SetActive(false);
        }
        else if (TankManager.Instance.TankStateByIndex[currentIndex] == TankState.NotEquip)
        {
            this.buyButton.SetActive(false);
            this.equipButton.SetActive(true);
            this.equippedButton.SetActive(false);
        }
        else if (TankManager.Instance.TankStateByIndex[currentIndex] == TankState.Equipped)
        {
            this.buyButton.SetActive(false);
            this.equipButton.SetActive(false);
            this.equippedButton.SetActive(true);
        }
    }
}