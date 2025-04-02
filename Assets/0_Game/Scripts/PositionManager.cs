using System;
using System.Collections.Generic;
using UnityEngine;

public class PositionManager : MonoBehaviour
{
    private List<Vector3> botPositions = new List<Vector3>();
    private Vector3       playerPosition;

    private void Update()
    {
        List<BotTank> bots = new List<BotTank>(FindObjectsOfType<BotTank>());

        botPositions.Clear();
        foreach (BotTank bot in bots)
        {
            botPositions.Add(bot.transform.position);
        }

        playerPosition = FindObjectOfType<PlayerTank>().transform.position;
    }
}