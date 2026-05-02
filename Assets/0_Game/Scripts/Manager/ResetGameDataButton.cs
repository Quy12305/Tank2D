using UnityEngine;

public class ResetGameDataButton : MonoBehaviour
{
    public void ResetAllGameData()
    {
        SaveLoadManager saveLoadManager = SaveLoadManager.Instance;
        if (saveLoadManager == null)
        {
            return;
        }

        saveLoadManager.DeleteSaveFile();

        GameDataLevel.Instance.currentMode = Mode.TankWarfare;
        GameDataLevel.Instance.LevelIndexByMode[Mode.TankWarfare] = 0;
        GameDataLevel.Instance.LevelIndexByMode[Mode.GemQuest] = 0;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetToDefaults();
        }

        if (TankManager.Instance != null)
        {
            TankManager.Instance.ResetToDefaults();
        }

        if (Coin.Instance != null)
        {
            Coin.Instance.coinCount = 0;
        }

        DailyRewardManager.Instance.LoadFromData(null);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCoin();
        }

        Debug.Log("All game data reset to defaults.");
    }
}
