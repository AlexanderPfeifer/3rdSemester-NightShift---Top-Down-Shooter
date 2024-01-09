using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ride : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    [SerializeField] private List<SpawnPoint> spawnPoints;

    [SerializeField] private float waveTimer = 120f;

    public float currentRideHp;
    [SerializeField] private float maxRideHp;

    private void Start()
    {
        currentRideHp = maxRideHp;
    }

    public IEnumerator StartWave()
    {
        while (waveTimer > 0)
        {
            Instantiate(enemyPrefab, spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position, Quaternion.identity, transform);

            yield return new WaitForSeconds(1);
        }
    }

    public void ResetRide()
    {
        currentRideHp = maxRideHp;
        for (int i = 0; i < transform.childCount; i++)
        {
            var enemy = transform.GetChild(0).transform.gameObject;
            Destroy(enemy);
        }
    }
}
