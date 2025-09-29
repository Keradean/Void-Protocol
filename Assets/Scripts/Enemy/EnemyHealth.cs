/*
====================================================================
* EnemyHealth
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
using UnityEngine;
using UnityEngine.Pool;

// ==================================================
// ENEMY HEALTH CLASS
// ==================================================
// This class manages enemy health and death behavior
// It implements IDamageable interface so weapons/projectiles can damage enemies
// Works with Object Pooling system - enemies are returned to pool when killed (not destroyed)
//
// KEY FEATURES:
// - Health management (take damage, track current health)
// - Death handling (return to pool instead of destroying)
// - Experience drop (rewards player with EXP on kill)
// - Pool integration (spawner can reuse dead enemies)
public class EnemyHealth : MonoBehaviour, IDamageable
{
    // ==================================================
    // VARIABLE DECLARATION - HEALTH SYSTEM
    // ==================================================

    [Header("Config")] // Header in Unity Inspector
    // Maximum health value (starting health when enemy spawns)
    [SerializeField] private float health;

    // Current health of the enemy (decreases when taking damage)
    // { get; private set; } means: other scripts can read it, only this script can change it
    public float CurrentHealth { get; private set; }

    // ==================================================
    // VARIABLE DECLARATION - COMPONENT REFERENCES
    // ==================================================

    // Reference to EnemyBrain component (handles AI behavior)
    private EnemyBrain enemyBrain;

    // Reference to EnemyEXP component (stores experience drop value)
    private EnemyEXP enemyExp;

    //private Animator animator; // Animator reference (commented out - not implemented yet)

    // ==================================================
    // VARIABLE DECLARATION - OBJECT POOLING
    // ==================================================

    // Reference to the object pool this enemy belongs to
    // Used to return enemy to pool when it dies
    private IObjectPool<EnemyHealth> enemyPool;

    // ==================================================
    // SET POOL METHOD
    // ==================================================
    // This public method is called by the Spawner to assign the pool reference
    // Must be called after instantiating the enemy
    //
    // PARAMETERS:
    // IObjectPool<EnemyHealth> pool - The pool that manages this enemy
    public void SetPool(IObjectPool<EnemyHealth> pool)
    {
        // Store reference to the pool
        enemyPool = pool;
    }

    // ==================================================
    // AWAKE METHOD
    // ==================================================
    // Awake is called when the script instance is being loaded
    // Used to get component references
    private void Awake()
    {
        // Get the EnemyBrain component (AI controller)
        enemyBrain = GetComponent<EnemyBrain>();

        // Get the EnemyEXP component (experience drop data)
        enemyExp = GetComponent<EnemyEXP>();

        // TODO: Get Animator component when animation system is implemented
        // animator = GetComponent<Animator>();
    }

    // ==================================================
    // ON SPAWN METHOD
    // ==================================================
    // This public method is called by the pool when the enemy is spawned/activated
    // Resets the enemy to its initial state (full health, AI enabled)
    public void OnSpawn()
    {
        // Reset health to maximum (full health when spawning)
        CurrentHealth = health;

        // Enable the AI brain (allows enemy to move, patrol, chase, attack)
        enemyBrain.enabled = true;
    }

    // ==================================================
    // TAKE DAMAGE METHOD
    // ==================================================
    // This public method is called when the enemy receives damage
    // Required by IDamageable interface - allows weapons to damage this enemy
    //
    // PARAMETERS:
    // float amount - Amount of damage to apply to the enemy's health
    public void TakeDamage(float amount)
    {
        // Subtract damage from current health
        CurrentHealth -= amount;

        // Check if enemy is dead (health at or below zero)
        if (CurrentHealth <= 0f)
        {
            // Trigger death sequence
            EnemyDead();
        }
    }

    // ==================================================
    // ENEMY DEAD METHOD
    // ==================================================
    // This private method handles enemy death
    // Returns enemy to pool and gives player experience points
    // Von Julian [AI-ASSISTED] Audio integration for enemy death feedback
    private void EnemyDead()
    {
        // Safety check: make sure pool reference exists
        if (enemyPool != null)
        {
            // Von Julian [AI-ASSISTED] - Play spider defeat sound effect at enemy's position
            SoundManager.Instance?.PlaySpiderDefeat(transform.position);

            // Disable the AI brain (stop all enemy behavior)
            // This prevents the enemy from moving/attacking while being returned to pool
            enemyBrain.enabled = false;

            // Return this enemy to the pool (deactivates and stores for reuse)
            // This is more efficient than destroying the enemy
            enemyPool.Release(this);

            // Give player experience points for killing this enemy
            // GameManager handles adding EXP and potential level-up
            GameManager.Instance.AddPlayerExp(enemyExp.ExpDrop);
        }
    }

    // ==================================================
    // COMMENTED OUT: ORIGINAL ENEMY DEAD METHOD
    // ==================================================
    // This is the original version before Julian added audio integration
    // Kept for reference/documentation purposes
    /*
     private void EnemyDead()
     {
           if (enemyPool != null)
           {
               enemyBrain.enabled = false;
               enemyPool.Release(this);
               GameManager.Instance.AddPlayerExp(enemyExp.ExpDrop);
           }
           // Animation
     }
    */
}