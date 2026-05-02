using System.IO;
using UnityEngine;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    public GameDataLevel levelManager;
    public TankManager   tankManager;

    private string filePath;

    private void Awake()
    {
        filePath = Application.persistentDataPath + "/gamedata.json";
        Debug.Log("Save path: " + filePath);
    }

    public string SaveFilePath => filePath;

    public void SaveGame()
    {
        if (levelManager == null || tankManager == null || Coin.Instance == null)
        {
            return;
        }

        GameData data = new GameData();
        data.levelData = levelManager.GetData();
        data.tankData = tankManager.GetData();
        data.coinCount = Coin.Instance.coinCount;
        data.dailyRewardData = DailyRewardManager.Instance.GetSaveData();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Game saved!");
    }

    public void LoadGame()
    {
        if (File.Exists(filePath))
        {
            string   json = File.ReadAllText(filePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            if (data != null)
            {
                if (data.levelData != null)
                {
                    levelManager.LoadFromData(data.levelData);
                }

                if (data.tankData != null)
                {
                    tankManager.LoadFromData(data.tankData);
                }

                if (Coin.Instance != null)
                {
                    Coin.Instance.coinCount = data.coinCount;
                }

                DailyRewardManager.Instance.LoadFromData(data.dailyRewardData);

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateCoin();
                }
            }

            Debug.Log("Game loaded!");
        }
        else
        {
            DailyRewardManager.Instance.ForceRefresh();
        }
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        Debug.Log("Save file deleted!");
    }
}
