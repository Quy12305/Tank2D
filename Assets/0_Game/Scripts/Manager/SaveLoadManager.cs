using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    public GameDataLevel levelManager;

    private string filePath;

    private void Awake()
    {
        filePath = Application.persistentDataPath + "/gamedata.json";
        Debug.Log("Save path: " + filePath);
    }

    public void SaveGame()
    {
        GameData data = new GameData();
        data.levelData   = levelManager.GetData();

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

            levelManager.LoadFromData(data.levelData);

            Debug.Log("Game loaded!");
        }
        else
        {
            Debug.LogWarning("No save file found.");
        }
    }

    #if UNITY_EDITOR
    [Button("Delete Data")]
    public void DeleteSaveFile()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Save file deleted.");
        }
        else
        {
            Debug.LogWarning("No save file to delete.");
        }
    }
    #endif
}