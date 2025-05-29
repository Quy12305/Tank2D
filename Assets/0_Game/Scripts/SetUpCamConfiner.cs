using System;
using Cinemachine;
using UnityEngine;

public class SetUpCamConfiner : MonoBehaviour
{
    public PolygonCollider2D boundaryCollider;
    private CinemachineConfiner confiner;

    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner>();
    }

    private void OnEnable()
    {
        if (MazeGenerator.Instance != null)
        {
            MazeGenerator.Instance.OnMapGenerationCompleted += SetupBoundaryCollider;
        }
    }

    private void Start()
    {
        if (boundaryCollider == null)
        {
            boundaryCollider = GetComponent<PolygonCollider2D>();
            if (boundaryCollider == null)
            {
                boundaryCollider = gameObject.AddComponent<PolygonCollider2D>();
            }
        }

        // Set initial confiner reference
        if (confiner != null && boundaryCollider != null)
        {
            confiner.m_BoundingShape2D = boundaryCollider;
        }
    }

    void OnDestroy()
    {
        if (MazeGenerator.Instance != null)
        {
            MazeGenerator.Instance.OnMapGenerationCompleted -= SetupBoundaryCollider;
        }
    }

    void SetupBoundaryCollider()
    {
        if (boundaryCollider == null || confiner == null) return;

        float mapGridRows = MazeGenerator.Instance.height;
        float mapGridCols = MazeGenerator.Instance.width;
        float tileSize = MazeGenerator.Instance.tileSize;

        float mapWorldDimensionX = mapGridRows * tileSize;
        float mapWorldDimensionY = mapGridCols * tileSize;

        Vector2[] colliderPoints = new Vector2[4];
        float halfWorldDimensionX = mapWorldDimensionX / 2f;
        float halfWorldDimensionY = mapWorldDimensionY / 2f;

        colliderPoints[0] = new Vector2(-halfWorldDimensionX, -halfWorldDimensionY);
        colliderPoints[1] = new Vector2(halfWorldDimensionX, -halfWorldDimensionY);
        colliderPoints[2] = new Vector2(halfWorldDimensionX, halfWorldDimensionY);
        colliderPoints[3] = new Vector2(-halfWorldDimensionX, halfWorldDimensionY);

        boundaryCollider.points = colliderPoints;

        // Update confiner
        confiner.m_BoundingShape2D = boundaryCollider;
        confiner.InvalidatePathCache();
    }
}