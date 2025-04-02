using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private Vector2 startPoint;

    public Vector2 StartPoint => startPoint;

    public bool CheckWin()
    {
        bool checkwin = true;

        return checkwin;
    }
}