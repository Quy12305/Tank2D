using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public GameObject map;
    public GameObject BotAndPlayer;

    public bool CheckWin()
    {
        BotTank[] botTanks = FindObjectsOfType<BotTank>();
        if (botTanks.Length > 0)
        {
            return false;
        }

        return true;
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
    }

    private void DestroyAllChildren(Transform parent)
    {
        // Tạo danh sách tạm để tránh lỗi khi xóa trong lúc lặp
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
}