using System.Dynamic;
using UnityEngine;
using UnityEngine.Pool;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float timeBtwSpawns;
    [SerializeField] private int spawnStop;

    private float timeSinceLastSpawn;
    private int currentSpawnCount;

    [SerializeField] private EnemyHealth enemyPrefab;
    private IObjectPool<EnemyHealth> enemyPool;

    private void Awake()
    {
        enemyPool = new ObjectPool<EnemyHealth>(CreateEnemy,OnGet, OnRelease);
        currentSpawnCount = 0;
    }

    private void OnGet(EnemyHealth enemyHealth)
    {
        enemyHealth.gameObject.SetActive(true);
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        enemyHealth.transform.position = randomSpawnPoint.position;
    }

    private void OnRelease(EnemyHealth enemyHealth)
    {
        enemyHealth.gameObject?.SetActive(false);
    }

    private EnemyHealth CreateEnemy()
    {
        EnemyHealth enemy = Instantiate(enemyPrefab);
        enemy.SetPool(enemyPool);  
        return enemy;
    }

    public void Update()
    {
        if (currentSpawnCount >= spawnStop) return; 
        if (Time.time > timeSinceLastSpawn)
        {
            enemyPool.Get();
            timeSinceLastSpawn = Time.time + timeBtwSpawns;
            currentSpawnCount++;
        }
    }

}
