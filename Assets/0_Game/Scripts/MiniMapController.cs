using UnityEngine;
using UnityEngine.UI;

public class MiniMapController : MonoBehaviour
{
    [SerializeField] private Camera miniMapCamera;
    [SerializeField] private RawImage miniMapImage;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private LayerMask miniMapLayerMask = -1;
    [SerializeField] private Color backgroundColor = new Color(0.05f, 0.05f, 0.05f, 1f);
    [SerializeField] private bool fitToRawImage = true;

    private void OnDestroy()
    {
        if (MazeGenerator.Instance != null)
        {
            MazeGenerator.Instance.OnMapGenerationCompleted -= RefreshBounds;
        }
    }

    private void Start()
    {
        if (MazeGenerator.Instance != null)
        {
            MazeGenerator.Instance.OnMapGenerationCompleted += RefreshBounds;
        }

        if (miniMapCamera != null)
        {
            miniMapCamera.orthographic = true;
            miniMapCamera.clearFlags = CameraClearFlags.SolidColor;
            miniMapCamera.backgroundColor = backgroundColor;
            miniMapCamera.cullingMask = miniMapLayerMask.value == 0 ? ~LayerMask.GetMask("UI") : miniMapLayerMask;
            miniMapCamera.targetTexture = renderTexture;
        }

        if (miniMapImage != null)
        {
            miniMapImage.texture = renderTexture;
            miniMapImage.color = Color.white;
            miniMapImage.uvRect = new Rect(0f, 0f, 1f, 1f);
        }

        RefreshBounds();
    }

    private void RefreshBounds()
    {
        if (miniMapCamera == null || MazeGenerator.Instance == null)
        {
            return;
        }

        float mapWidth = MazeGenerator.Instance.GetMapWorldWidth();
        float mapHeight = MazeGenerator.Instance.GetMapWorldHeight();
        miniMapCamera.transform.position = MazeGenerator.Instance.GetMapCenter() + new Vector3(0f, 0f, -10f);

        if (fitToRawImage && miniMapImage != null && miniMapImage.rectTransform.rect.height > 0.01f)
        {
            float targetAspect = miniMapImage.rectTransform.rect.width / miniMapImage.rectTransform.rect.height;
            float mapAspect = mapWidth / mapHeight;

            if (mapAspect > targetAspect)
            {
                miniMapCamera.orthographicSize = (mapWidth / targetAspect) * 0.5f;
            }
            else
            {
                miniMapCamera.orthographicSize = mapHeight * 0.5f;
            }
        }
        else
        {
            miniMapCamera.orthographicSize = Mathf.Max(mapWidth, mapHeight) * 0.5f;
        }
    }
}
