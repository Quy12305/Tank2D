using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    [SerializeField] private int maxHits = 2;

    private int            currentHits;
    private SpriteRenderer cachedRenderer;
    private Color          originalColor;

    private void Awake()
    {
        cachedRenderer = GetComponent<SpriteRenderer>();
        if (cachedRenderer != null)
        {
            originalColor = cachedRenderer.color;
        }
    }

    public void RegisterHit()
    {
        currentHits++;

        if (currentHits >= maxHits)
        {
            MazeGenerator.Instance.RemoveBreakableWallAtWorld(transform.position);
            Destroy(gameObject);
            return;
        }

        ApplyCrackedVisual();
    }

    private void ApplyCrackedVisual()
    {
        if (cachedRenderer == null)
        {
            return;
        }

        cachedRenderer.color = Color.Lerp(originalColor, new Color(0.55f, 0.32f, 0.32f, 1f), 0.6f);
    }
}
