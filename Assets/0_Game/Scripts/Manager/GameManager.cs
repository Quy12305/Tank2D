using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { MainMenu, GamePlay, Win, Lose}
public class GameManager : Singleton<GameManager>
{
    private GameState state;

    private void Awake()
    {
        ConfigureProjectileCollisions();
        ChangeState(GameState.MainMenu);
    }

    public void ChangeState(GameState gameState)
    {
        state = gameState;
    }

    public bool IsState(GameState gameState)
    {
        return state == gameState;
    }

    private void ConfigureProjectileCollisions()
    {
        int playerProjectileLayer = LayerMask.NameToLayer("PlayerProjectile");
        int enemyProjectileLayer = LayerMask.NameToLayer("EnemyProjectile");

        if (playerProjectileLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(playerProjectileLayer, playerProjectileLayer, true);
        }

        if (enemyProjectileLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(enemyProjectileLayer, enemyProjectileLayer, true);
        }

        if (playerProjectileLayer >= 0 && enemyProjectileLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(playerProjectileLayer, enemyProjectileLayer, true);
        }
    }
}
