using System;
using UnityEngine;

public class SetUpCamConfiner : MonoBehaviour
{
    public PolygonCollider2D boundaryCollider;

    private void OnEnable()
    {
        MazeGenerator.Instance.OnMapGenerationCompleted += SetupBoundaryCollider;
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
    }

    void OnDestroy()
    {
        MazeGenerator.Instance.OnMapGenerationCompleted -= SetupBoundaryCollider;
    }

    void SetupBoundaryCollider()
    {
        float mapGridRows = MazeGenerator.Instance.height;
        float mapGridCols = MazeGenerator.Instance.width;
        float tileSize    = MazeGenerator.Instance.tileSize;

        float mapWorldDimensionX = mapGridRows * tileSize;
        float mapWorldDimensionY = mapGridCols * tileSize;

        Vector2[] colliderPoints = new Vector2[4];

        // Tính toán kích thước
        float halfWorldDimensionX = mapWorldDimensionX / 2f;
        float halfWorldDimensionY = mapWorldDimensionY / 2f;

        // Định nghĩa 4 góc của collider
        colliderPoints[0] = new Vector2(-halfWorldDimensionX, -halfWorldDimensionY);
        colliderPoints[1] = new Vector2(halfWorldDimensionX, -halfWorldDimensionY);
        colliderPoints[2] = new Vector2(halfWorldDimensionX, halfWorldDimensionY);
        colliderPoints[3] = new Vector2(-halfWorldDimensionX, halfWorldDimensionY);

        boundaryCollider.points = colliderPoints;
    }
}