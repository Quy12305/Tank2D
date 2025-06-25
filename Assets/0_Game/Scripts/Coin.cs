using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Coin : Singleton<Coin>
{
    public                   int            coinCount;
    private                  float          flyDuration = 0.8f;
    private                  AnimationCurve flyCurve;
    [SerializeField] private GameObject     CoinImg;
    [SerializeField] private RectTransform  coinUI;
    [SerializeField] private RectTransform  startPos;
    [SerializeField] private RectTransform  canvasRect;

    public int CoinCount { set { this.coinCount = value; } }

    public void AddCoin(int amount)
    {
        this.coinCount += amount;
        UIManager.Instance.UpdateCoin();
        SaveLoadManager.Instance.SaveGame();
    }

    public void SpawnWinCoins(int coin)
    {
        int   visualCount = 20;
        float radius      = 100f;
        float flyDuration = 0.8f;

        Vector2 actualStartPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, startPos.position),
            null,
            out actualStartPos
        );

        Vector2 targetPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, coinUI.position),
            null,
            out targetPos
        );

        for (int i = 0; i < visualCount; i++)
        {
            GameObject    coinObj = Instantiate(this.CoinImg, canvasRect);
            RectTransform coinRT  = coinObj.GetComponent<RectTransform>();
            coinRT.anchoredPosition = actualStartPos;

            // Random offset trong vòng tròn
            Vector2 randomOffset = Random.insideUnitCircle * radius;
            Vector2 midTargetPos = actualStartPos + randomOffset;

            coinRT.DOAnchorPos(midTargetPos, 0.4f)
                  .SetEase(Ease.OutBack)
                  .OnComplete(() =>
                  {
                      Sequence s = DOTween.Sequence();
                      s.AppendInterval(Random.Range(0.2f, 0.4f))
                       .Append(coinRT.DOScale(1.3f, 0.2f).SetEase(Ease.OutBack))
                       .AppendCallback(() =>
                       {
                           coinRT.DOScale(0.5f, flyDuration * 0.7f).SetEase(Ease.InBack);
                           coinRT.DOAnchorPos(targetPos, flyDuration)
                                 .SetEase(Ease.InOutCubic)
                                 .OnComplete(() =>
                                 {
                                     Destroy(coinObj);
                                 });
                       });
                  });
        }

        DOVirtual.DelayedCall(1.8f, () =>
        {
            this.AddCoin(coin);
        });
    }
}