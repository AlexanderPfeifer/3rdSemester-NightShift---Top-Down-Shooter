using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> spawnPointList;
    [SerializeField] private float timeBetweenSpawns = 5f;
    [SerializeField] private Enemy enemy;
    
    private void OnEnable()
    {
        StartCoroutine(SpawnEnemyCoroutine());
    }

    private IEnumerator SpawnEnemyCoroutine()
    {
        while (true)
        {
            SpawnPoint spawnPoint = spawnPointList[Random.Range(0, spawnPointList.Count)];
            if (spawnPoint.IsFreeToSpawn())
            {
                Instantiate(enemy, spawnPoint.transform.position, Quaternion.identity);
                yield return new WaitForSeconds(timeBetweenSpawns);
            }
            else
            {
                yield return null;
            }
        }
    }
}
