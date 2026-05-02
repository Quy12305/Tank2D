using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardPanelUI : MonoBehaviour
{
    [SerializeField] private Button toggleButton;
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private DailyRewardDayItemUI[] dayItems;
    [SerializeField] private string dayLabelPrefix = "Day ";
    [SerializeField] private string amountPrefix = "x";

    private void Awake()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(TogglePanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HidePanel);
        }

        HideImmediate();
    }

    private void OnEnable()
    {
        DailyRewardManager.Instance.StateChanged += Refresh;
    }

    private void OnDisable()
    {
        DailyRewardManager.Instance.StateChanged -= Refresh;
    }

    public void Refresh()
    {
        if (dayItems == null || dayItems.Length == 0)
        {
            return;
        }

        DailyRewardManager.DailyRewardDayInfo[] infos = DailyRewardManager.Instance.GetDayInfos();
        for (int i = 0; i < dayItems.Length && i < infos.Length; i++)
        {
            DailyRewardManager.DailyRewardDayInfo info = infos[i];
            DailyRewardDayItemUI dayItem = dayItems[i];
            if (dayItem == null)
            {
                continue;
            }

            int capturedIndex = i;
            dayItem.Bind(info, dayLabelPrefix, amountPrefix, () =>
            {
                if (DailyRewardManager.Instance.ClaimReward(capturedIndex))
                {
                    Refresh();
                }
            });
        }
    }

    public void HideImmediate()
    {
        if (panelRoot != null)
        {
            panelRoot.gameObject.SetActive(false);
        }
    }

    private void TogglePanel()
    {
        if (panelRoot != null && panelRoot.gameObject.activeSelf)
        {
            HidePanel();
            return;
        }

        ShowPanel();
    }

    private void ShowPanel()
    {
        Refresh();

        if (panelRoot == null)
        {
            return;
        }

        panelRoot.gameObject.SetActive(true);
        panelRoot.localScale = Vector3.one * 0.92f;
        panelRoot.DOScale(Vector3.one, 0.22f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private void HidePanel()
    {
        if (panelRoot == null)
        {
            return;
        }

        panelRoot.DOScale(Vector3.one * 0.92f, 0.15f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => panelRoot.gameObject.SetActive(false));
    }
}
