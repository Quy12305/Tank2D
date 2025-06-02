using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> boosterPrefabsModeBot;
    [SerializeField] private List<GameObject> boosterPrefabsModeGem;
    [SerializeField] private MazeGenerator    mazeGenerator;
    private                  List<Vector2Int> emptyCells;
    private                  int              emptyCellsCount = 0;
    private                  float            timeToSpawn     = 0f;
    private                  float            time            = 5f;

    // Start is called before the first frame update
    private void Start() { mazeGenerator.OnMapGenerationCompleted += GetEmptyTransform; }

    // Update is called once per frame
    private void Update()
    {
        if (GameManager.Instance.IsState(GameState.GamePlay))
        {
            timeToSpawn += Time.deltaTime;
            if (timeToSpawn >= time && this.transform.childCount * 10f < emptyCellsCount)
            {
                SpawnBooster();
                timeToSpawn = 0f;
                time        = Random.Range(5f, 10f);
            }
        }
    }

    public void GetEmptyTransform()
    {
        emptyCells      = mazeGenerator.GetEmptyCells();
        emptyCellsCount = emptyCells.Count;
    }

    public void SpawnBooster()
    {
        if (emptyCells.Count == 0) return;

        Vector2Int randomCell    = emptyCells[Random.Range(0, emptyCells.Count)];
        Vector3    spawnPosition = mazeGenerator.GridToWorldPosition(randomCell.x, randomCell.y);

        if (LevelManager.Instance.CurrentMode == Mode.TankWarfare)
        {
            GameObject boosterPrefab = this.boosterPrefabsModeBot[Random.Range(0, this.boosterPrefabsModeBot.Count)];
            Instantiate(boosterPrefab, spawnPosition, Quaternion.identity, transform);
        }
        else if (LevelManager.Instance.CurrentMode == Mode.GemQuest)
        {
            GameObject boosterPrefab = this.boosterPrefabsModeGem[Random.Range(0, this.boosterPrefabsModeGem.Count)];
            Instantiate(boosterPrefab, spawnPosition, Quaternion.identity, transform);
        }
        emptyCells.Remove(randomCell);
    }
}