using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FakeLoadingOverlayUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private float totalDuration;
    [SerializeField] private float fadeInDuration = 0.28f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private float startPauseMin = 0.08f;
    [SerializeField] private float startPauseMax = 0.28f;
    [SerializeField] private float checkpointPauseMin = 0.18f;
    [SerializeField] private float checkpointPauseMax = 0.75f;
    [SerializeField] private float endPauseMin = 0.06f;
    [SerializeField] private float endPauseMax = 0.2f;
    [SerializeField] private int checkpointCount = 3;
    [SerializeField] private int checkpointMinPercent = 15;
    [SerializeField] private int checkpointMaxPercent = 75;
    [SerializeField] private int minCheckpointGap = 15;
    [SerializeField] private string initialHint = "Preparing battlefield...";
    [SerializeField] private string loadMomentHint = "Syncing systems...";
    [SerializeField] private TMP_Text hintText;

    private Sequence currentSequence;

    private void Awake()
    {
        HideImmediate();
    }

    public void Play(Action onLoadMoment, Action onFinished, float duration = -1f)
    {
        if (canvasGroup == null || progressFill == null || progressText == null)
        {
            onLoadMoment?.Invoke();
            onFinished?.Invoke();
            return;
        }

        float resolvedDuration = duration > 0f ? duration : totalDuration;
        List<float> checkpoints = GenerateCheckpoints();
        float startPause = UnityEngine.Random.Range(startPauseMin, startPauseMax);
        float endPause = UnityEngine.Random.Range(endPauseMin, endPauseMax);
        float[] checkpointPauses = GenerateCheckpointPauses(checkpoints.Count);
        float stageBudget = Mathf.Max(
            1.5f,
            resolvedDuration - fadeInDuration - fadeOutDuration - startPause - endPause - Sum(checkpointPauses));
        float[] stageDurations = GenerateStageDurations(checkpoints, stageBudget);

        gameObject.SetActive(true);
        currentSequence?.Kill();
        canvasGroup.alpha = 0f;
        progressFill.fillAmount = 0f;
        progressText.text = "0%";
        if (hintText != null)
        {
            hintText.text = initialHint;
        }

        bool invoked = false;

        currentSequence = DOTween.Sequence().SetUpdate(true);
        currentSequence.Append(canvasGroup.DOFade(1f, fadeInDuration));
        currentSequence.AppendInterval(startPause);
        float previousProgress = 0f;
        int loadMomentIndex = Mathf.Clamp(checkpoints.Count / 2, 0, checkpoints.Count - 1);

        for (int i = 0; i < checkpoints.Count; i++)
        {
            float targetProgress = checkpoints[i];
            Ease ease = GetStageEase(i, checkpoints.Count);
            currentSequence.Append(CreateProgressTween(previousProgress, targetProgress, stageDurations[i], ease));
            previousProgress = targetProgress;

            currentSequence.AppendInterval(checkpointPauses[i]);

            if (i == loadMomentIndex)
            {
                currentSequence.AppendCallback(() =>
                {
                    if (invoked)
                    {
                        return;
                    }

                    invoked = true;
                    if (hintText != null)
                    {
                        hintText.text = loadMomentHint;
                    }

                    onLoadMoment?.Invoke();
                });
            }
        }

        currentSequence.Append(CreateProgressTween(previousProgress, 1f, stageDurations[stageDurations.Length - 1], Ease.InExpo));
        currentSequence.AppendInterval(endPause);
        currentSequence.Append(canvasGroup.DOFade(0f, fadeOutDuration));
        currentSequence.OnComplete(() =>
        {
            HideImmediate();
            onFinished?.Invoke();
        });
    }

    public void HideImmediate()
    {
        currentSequence?.Kill();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        gameObject.SetActive(false);
    }

    private void UpdateProgress(float normalized)
    {
        float clampedProgress = Mathf.Clamp01(normalized);
        progressFill.fillAmount = clampedProgress;
        progressText.text = $"{Mathf.RoundToInt(clampedProgress * 100f)}%";
    }

    private Tween CreateProgressTween(float from, float to, float duration, Ease ease)
    {
        float tweenValue = from;
        return DOTween.To(() => tweenValue, value =>
        {
            tweenValue = value;
            UpdateProgress(tweenValue);
        }, to, duration).SetEase(ease);
    }

    private List<float> GenerateCheckpoints()
    {
        int minPercent = Mathf.Clamp(checkpointMinPercent, 5, 90);
        int maxPercent = Mathf.Clamp(checkpointMaxPercent, minPercent + minCheckpointGap, 95);
        int maxCountByRange = Mathf.Max(1, ((maxPercent - minPercent) / Mathf.Max(1, minCheckpointGap)) + 1);
        int count = Mathf.Clamp(checkpointCount, 1, maxCountByRange);

        List<int> values = new List<int>();
        int attempts = 0;

        while (values.Count < count && attempts < 500)
        {
            attempts++;
            int candidate = UnityEngine.Random.Range(minPercent, maxPercent + 1);
            bool valid = true;

            for (int i = 0; i < values.Count; i++)
            {
                if (Mathf.Abs(values[i] - candidate) < minCheckpointGap)
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
            {
                continue;
            }

            values.Add(candidate);
        }

        if (values.Count < count)
        {
            values.Clear();
            int current = minPercent;
            for (int i = 0; i < count; i++)
            {
                values.Add(Mathf.Min(current, maxPercent));
                current += minCheckpointGap;
            }
        }

        values.Sort();

        List<float> checkpoints = new List<float>(values.Count);
        for (int i = 0; i < values.Count; i++)
        {
            checkpoints.Add(values[i] / 100f);
        }

        return checkpoints;
    }

    private float[] GenerateCheckpointPauses(int count)
    {
        float[] pauses = new float[count];
        for (int i = 0; i < count; i++)
        {
            float bias = i == count - 1 ? 1.15f : 1f;
            pauses[i] = UnityEngine.Random.Range(checkpointPauseMin, checkpointPauseMax) * bias;
        }

        return pauses;
    }

    private float[] GenerateStageDurations(List<float> checkpoints, float totalMoveDuration)
    {
        int segmentCount = checkpoints.Count + 1;
        float[] stageDurations = new float[segmentCount];
        float[] progressStops = new float[segmentCount + 1];
        progressStops[0] = 0f;

        for (int i = 0; i < checkpoints.Count; i++)
        {
            progressStops[i + 1] = checkpoints[i];
        }

        progressStops[segmentCount] = 1f;

        float[] weights = new float[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            float delta = progressStops[i + 1] - progressStops[i];
            float speedFactor = UnityEngine.Random.Range(0.6f, 1.6f);
            float stickiness = i == segmentCount - 1
                ? UnityEngine.Random.Range(0.4f, 0.7f)
                : UnityEngine.Random.Range(0.75f, 1.35f);
            weights[i] = Mathf.Max(0.01f, delta * stickiness / speedFactor);
        }

        float totalWeight = Sum(weights);
        for (int i = 0; i < segmentCount; i++)
        {
            stageDurations[i] = totalMoveDuration * (weights[i] / totalWeight);
        }

        return stageDurations;
    }

    private Ease GetStageEase(int index, int checkpointCountValue)
    {
        if (index == 0)
        {
            return Ease.OutCubic;
        }

        if (index == checkpointCountValue - 1)
        {
            return Ease.InOutSine;
        }

        return UnityEngine.Random.value > 0.5f ? Ease.OutQuad : Ease.InOutQuad;
    }

    private float Sum(float[] values)
    {
        float total = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            total += values[i];
        }

        return total;
    }
}
