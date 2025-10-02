/*
====================================================================
* Spawner
====================================================================
* Project: Void Protocol
* Course: PIP
* Script-Developer: Dennis De Col
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
====================================================================
*/
using System.Dynamic;
using UnityEngine;
using UnityEngine.Pool;

// ==================================================
// SPAWNER CLASS
// ==================================================
// This class manages enemy spawning using Unity's Object Pooling system
// Instead of constantly creating and destroying enemies (expensive operations),
// it reuses enemies from a pool for better performance
//
// OBJECT POOLING CONCEPT:
// - Create a pool of enemies at the start
// - When spawning: "Get" an inactive enemy from pool and activate it
// - When enemy dies: return it to pool (deactivate, don't destroy)
// - This reduces garbage collection and improves performance
//
// SPAWNING BEHAVIOR:
// - Spawns enemies at random spawn points
// - Time-based spawning (spawns every X seconds)
// - Can set a maximum spawn count limit
public class Spawner : MonoBehaviour
{
    // ==================================================
    // VARIABLE DECLARATION - SPAWN CONFIGURATION
    // ==================================================

    // Array of possible spawn locations (Transform positions in the scene)
    // Enemies will spawn randomly at one of these points
    [SerializeField] private Transform[] spawnPoints;

    // Time delay between each enemy spawn (in seconds)
    // Example: 2.0f = spawn a new enemy every 2 seconds
    [SerializeField] private float timeBtwSpawns;

    // Maximum number of enemies to spawn (spawn limit)
    // Example: 10 = stop spawning after 10 enemies have been spawned
    // Useful for wave-based gameplay or preventing endless spawning
    [SerializeField] private int spawnStop;

    // Timestamp for when the next spawn should occur
    // Compared with Time.time to determine if it's time to spawn
    private float timeSinceLastSpawn;

    // Counter tracking how many enemies have been spawned so far
    // Used to check against spawnStop limit
    private int currentSpawnCount;

    // ==================================================
    // VARIABLE DECLARATION - OBJECT POOLING
    // ==================================================

    // Reference to the enemy prefab that will be spawned
    // This is the template used to create enemies in the pool
    [SerializeField] private EnemyHealth enemyPrefab;

    // The object pool that manages enemy instances
    // IObjectPool is Unity's built-in interface for object pooling
    // Generic type <EnemyHealth> means this pool stores EnemyHealth objects
    private IObjectPool<EnemyHealth> enemyPool;

    // ==================================================
    // AWAKE METHOD
    // ==================================================
    // Awake is called when the script instance is being loaded (before Start)
    // Used to initialize the object pool
    private void Awake()
    {
        // Create a new object pool for enemies
        // ObjectPool constructor takes three delegate functions:
        // 1. CreateEnemy - called when pool needs to create a new enemy
        // 2. OnGet - called when an enemy is taken from the pool
        // 3. OnRelease - called when an enemy is returned to the pool
        enemyPool = new ObjectPool<EnemyHealth>(CreateEnemy, OnGet, OnRelease);

        // Initialize spawn counter to zero
        currentSpawnCount = 0;
    }

    // ==================================================
    // ON GET METHOD (POOL CALLBACK)
    // ==================================================
    // This method is called automatically when getting an enemy from the pool
    // It "activates" the enemy and positions it at a random spawn point
    //
    // PARAMETERS:
    // EnemyHealth enemyHealth - The enemy being retrieved from the pool
    private void OnGet(EnemyHealth enemyHealth)
    {
        // Call the enemy's OnSpawn method (resets health, AI state, etc.)
        enemyHealth.OnSpawn();

        // Activate the enemy GameObject (make it visible and active in the scene)
        enemyHealth.gameObject.SetActive(true);

        // Choose a random spawn point from the spawnPoints array
        // Random.Range(0, length) returns a random index between 0 and length-1
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Position the enemy at the selected spawn point
        enemyHealth.transform.position = randomSpawnPoint.position;
    }

    // ==================================================
    // ON RELEASE METHOD (POOL CALLBACK)
    // ==================================================
    // This method is called automatically when an enemy is returned to the pool
    // It "deactivates" the enemy without destroying it (for reuse later)
    //
    // PARAMETERS:
    // EnemyHealth enemyHealth - The enemy being returned to the pool
    private void OnRelease(EnemyHealth enemyHealth)
    {
        // Deactivate the enemy GameObject (hide it and stop its behavior)
        // The ?. operator is null-conditional (only calls SetActive if gameObject isn't null)
        enemyHealth.gameObject?.SetActive(false);
    }

    // ==================================================
    // CREATE ENEMY METHOD (POOL CALLBACK)
    // ==================================================
    // This method is called automatically when the pool needs to create a new enemy
    // It instantiates a new enemy and configures it to work with the pool
    //
    // RETURNS:
    // EnemyHealth - The newly created enemy instance
    private EnemyHealth CreateEnemy()
    {
        // Instantiate a new enemy from the prefab (creates a copy in the scene)
        EnemyHealth enemy = Instantiate(enemyPrefab);

        // Give the enemy a reference to this pool (so it can return itself when it dies)
        enemy.SetPool(enemyPool);

        // Return the newly created enemy
        return enemy;
    }

    // ==================================================
    // UPDATE METHOD
    // ==================================================
    // Update is called every frame by Unity
    // Handles time-based enemy spawning
    public void Update()
    {
        // Check if spawn limit has been reached
        // If we've spawned enough enemies, stop spawning (exit method early)
        if (currentSpawnCount >= spawnStop) return;

        // Check if enough time has passed since last spawn
        // Time.time returns the time in seconds since the game startedw
        if (Time.time > timeSinceLastSpawn)
        {
            // Get an enemy from the pool (spawns/activates it)
            // This triggers OnGet() which positions and activates the enemy
            enemyPool.Get();

            // Calculate the timestamp for the next spawn
            // Current time + delay = when next spawn should occur
            timeSinceLastSpawn = Time.time + timeBtwSpawns;

            // Increment the spawn counter
            currentSpawnCount++;
        }
    }
}