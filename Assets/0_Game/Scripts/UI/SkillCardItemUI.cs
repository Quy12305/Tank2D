using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCardItemUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text duration;

    private SkillType skillType;

    public RectTransform RectTransform => transform as RectTransform;
    public Button Button => button;
    public SkillType SkillType => skillType;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    public void Bind(
        SkillType newSkillType,
        Func<SkillType, float> durationProvider,
        Func<SkillType, string> titleProvider,
        Func<SkillType, string> descriptionProvider,
        Func<SkillType, Sprite> iconProvider,
        Action<SkillType> onSelect)
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        skillType = newSkillType;

        if (title != null)
        {
            title.text = titleProvider != null ? titleProvider(skillType) : skillType.GetTitle();
        }

        if (description != null)
        {
            description.text = descriptionProvider != null ? descriptionProvider(skillType) : skillType.GetDescription();
        }

        if (duration != null)
        {
            duration.text = $"{Mathf.RoundToInt(durationProvider(skillType))}s";
        }

        if (icon != null)
        {
            Sprite skillIcon = iconProvider != null ? iconProvider(skillType) : null;
            if (skillIcon != null)
            {
                icon.sprite = skillIcon;
                icon.color = Color.white;
            }
            else
            {
                icon.color = GetSkillColor(skillType);
            }
        }

        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onSelect?.Invoke(skillType));
        }
    }

    private static Color GetSkillColor(SkillType skillType)
    {
        return skillType switch
        {
            SkillType.Shield => new Color(0.27f, 0.82f, 1f, 1f),
            SkillType.BarrelUpgrade => new Color(1f, 0.7f, 0.22f, 1f),
            SkillType.SideTurrets => new Color(0.53f, 0.92f, 0.42f, 1f),
            SkillType.OrbitBlades => new Color(1f, 0.4f, 0.35f, 1f),
            _ => Color.white
        };
    }
}
