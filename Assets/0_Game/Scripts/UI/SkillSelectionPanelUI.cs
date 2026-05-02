using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectionPanelUI : MonoBehaviour
{
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private SkillCardItemUI[] cards;

    private Canvas canvas;
    private GraphicRaycaster graphicRaycaster;
    private Action<SkillType> selectionCallback;
    private bool isAnimatingSelection;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        graphicRaycaster = GetComponent<GraphicRaycaster>();
    }

    public bool Show(
        SkillType[] skillChoices,
        Func<SkillType, float> durationProvider,
        Func<SkillType, string> titleProvider,
        Func<SkillType, string> descriptionProvider,
        Func<SkillType, Sprite> iconProvider,
        Action<SkillType> onSelected)
    {
        if (skillChoices == null || skillChoices.Length == 0 || cards == null || cards.Length == 0)
        {
            return false;
        }

        bool hasAvailableCard = false;
        for (int i = 0; i < cards.Length && i < skillChoices.Length; i++)
        {
            if (cards[i] != null && cards[i].Button != null)
            {
                hasAvailableCard = true;
                break;
            }
        }

        if (!hasAvailableCard)
        {
            return false;
        }

        selectionCallback = onSelected;
        isAnimatingSelection = false;
        gameObject.SetActive(true);
        BringToFront();

        if (contentRect != null)
        {
            contentRect.localScale = Vector3.one * 0.94f;
            contentRect.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        for (int i = 0; i < cards.Length; i++)
        {
            SkillCardItemUI card = cards[i];
            bool hasSkill = i < skillChoices.Length;
            if (card != null)
            {
                card.gameObject.SetActive(hasSkill);
                if (card.RectTransform != null)
                {
                    card.RectTransform.localScale = Vector3.one;
                }
            }

            if (!hasSkill || card == null)
            {
                continue;
            }

            SkillType skillType = skillChoices[i];
            card.Bind(skillType, durationProvider, titleProvider, descriptionProvider, iconProvider, SelectSkill);
        }

        return true;
    }

    public void HideImmediate()
    {
        selectionCallback = null;

        if (cards != null)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null)
                {
                    continue;
                }

                RectTransform rectTransform = cards[i].RectTransform;
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.localScale = Vector3.one;
                }
            }
        }

        gameObject.SetActive(false);
    }

    private void SelectSkill(SkillType skillType)
    {
        if (isAnimatingSelection)
        {
            return;
        }

        SkillCardItemUI selectedCard = null;
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null && cards[i].SkillType == skillType && cards[i].gameObject.activeSelf)
            {
                selectedCard = cards[i];
                break;
            }
        }

        if (selectedCard == null)
        {
            return;
        }

        isAnimatingSelection = true;
        Action<SkillType> callback = selectionCallback;
        selectionCallback = null;
        callback?.Invoke(skillType);

        for (int i = 0; i < cards.Length; i++)
        {
            SkillCardItemUI card = cards[i];
            if (card == null)
            {
                continue;
            }

            if (card.Button != null)
            {
                card.Button.interactable = false;
            }

            if (card == selectedCard)
            {
                continue;
            }

            if (card.RectTransform != null)
            {
                card.RectTransform.DOScale(Vector3.one * 0.86f, 0.18f)
                    .SetUpdate(true)
                    .OnComplete(() => card.gameObject.SetActive(false));
            }
            else
            {
                card.gameObject.SetActive(false);
            }
        }

        if (selectedCard.RectTransform != null)
        {
            selectedCard.RectTransform.SetAsLastSibling();
        }

        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.AppendInterval(0.08f);

        if (selectedCard.RectTransform != null)
        {
            sequence.Append(selectedCard.RectTransform.DOAnchorPos(Vector2.zero, 0.28f).SetEase(Ease.InOutQuad));
            sequence.Join(selectedCard.RectTransform.DOScale(Vector3.one * 1.16f, 0.28f).SetEase(Ease.OutBack));
            sequence.Append(selectedCard.RectTransform.DOScale(Vector3.one * 1.34f, 0.22f).SetEase(Ease.InBack));
        }

        sequence.OnComplete(() =>
        {
            HideImmediate();
        });
    }

    private void BringToFront()
    {
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
        }

        if (graphicRaycaster == null)
        {
            graphicRaycaster = GetComponent<GraphicRaycaster>();
        }

        if (canvas == null)
        {
            return;
        }

        transform.SetAsLastSibling();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        if (graphicRaycaster != null)
        {
            graphicRaycaster.enabled = true;
        }
    }

}
