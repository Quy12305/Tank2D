using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardDayItemUI : MonoBehaviour
{
    [SerializeField] private Button claimButton;
    [SerializeField] private Button[] stateButtons;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text amountText;

    public void Bind(DailyRewardManager.DailyRewardDayInfo info, string dayLabelPrefix, string amountPrefix, Action onClaim)
    {
        if (info == null)
        {
            return;
        }

        if (dayText != null)
        {
            dayText.text = $"{dayLabelPrefix}{info.dayIndex + 1}";
        }

        if (amountText != null)
        {
            amountText.text = $"{amountPrefix}{info.rewardAmount}";
        }

        if (claimButton != null)
        {
            claimButton.interactable = info.canClaim;
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(() => onClaim?.Invoke());
        }

        SetState(info);
    }

    private void SetState(DailyRewardManager.DailyRewardDayInfo info)
    {
        if (stateButtons == null || stateButtons.Length == 0)
        {
            return;
        }

        for (int i = 0; i < stateButtons.Length; i++)
        {
            if (stateButtons[i] != null)
            {
                stateButtons[i].gameObject.SetActive(false);
            }
        }

        int stateIndex = info.canClaim ? 0 : info.isClaimed ? 1 : 2;
        if (stateIndex >= 0 && stateIndex < stateButtons.Length && stateButtons[stateIndex] != null)
        {
            stateButtons[stateIndex].gameObject.SetActive(true);
        }
    }
}
