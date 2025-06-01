using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> boosterPrefabs;
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

        GameObject boosterPrefab = boosterPrefabs[Random.Range(0, boosterPrefabs.Count)];
        Instantiate(boosterPrefab, spawnPosition, Quaternion.identity, transform);

        emptyCells.Remove(randomCell);
    }
}