using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameplayProgressionHud : MonoBehaviour
{
    [SerializeField] private Image progressFill;

    private ProgressionTracker tracker;

    private void Awake()
    {
        if (progressFill != null)
        {
            progressFill.fillAmount = 0f;
        }

        gameObject.SetActive(false);
    }

    public void BindTracker(ProgressionTracker progressionTracker)
    {
        if (tracker != null)
        {
            tracker.Updated -= HandleTrackerUpdated;
            tracker.Completed -= HandleTrackerCompleted;
        }

        tracker = progressionTracker;

        if (tracker == null)
        {
            if (progressFill != null)
            {
                progressFill.fillAmount = 0f;
            }

            gameObject.SetActive(false);
            return;
        }

        tracker.Updated += HandleTrackerUpdated;
        tracker.Completed += HandleTrackerCompleted;
        gameObject.SetActive(true);
        HandleTrackerUpdated(tracker);
    }

    private void OnDestroy()
    {
        if (tracker == null)
        {
            return;
        }

        tracker.Updated -= HandleTrackerUpdated;
        tracker.Completed -= HandleTrackerCompleted;
    }

    private void HandleTrackerUpdated(ProgressionTracker updatedTracker)
    {
        if (updatedTracker == null)
        {
            return;
        }

        if (progressFill != null)
        {
            progressFill.DOFillAmount(updatedTracker.NormalizedProgress, 0.2f).SetUpdate(true);
        }
    }

    private void HandleTrackerCompleted(ProgressionTracker completedTracker)
    {
        HandleTrackerUpdated(completedTracker);
        transform.DOPunchScale(Vector3.one * 0.08f, 0.35f, 6, 0.6f).SetUpdate(true);
    }
}
