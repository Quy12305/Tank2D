using UnityEngine;

public static class SaveSystem
{
    private static SaveLoadManager saveManager;

    private static SaveLoadManager GetManager()
    {
        if (saveManager == null)
        {
            saveManager = GameObject.FindObjectOfType<SaveLoadManager>();
            if (saveManager == null)
            {
                Debug.LogError("SaveSystem Error");
            }
        }
        return saveManager;
    }

    public static void SaveGame()
    {
        GetManager()?.SaveGame();
    }

    public static void LoadGame()
    {
        GetManager()?.LoadGame();
    }
}