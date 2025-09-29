/*
====================================================================
* EnemyController
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

// ==================================================
// ENEMY CONTROLLER CLASS
// ==================================================
// This class controls enemy AI behavior in the game
// It handles three main behaviors: Patrol, Chase, and Attack
// The enemy patrols waypoints when player is far away, chases when player is in range, and attacks when close enough
//
// AI STATE MACHINE:
// 1. PATROL: Enemy walks between patrol points when player is outside chase range
// 2. CHASE: Enemy moves toward player when player enters chase range
// 3. ATTACK: Enemy stops and attacks when player is within attack range
public class EnemyController : MonoBehaviour
{
    // ==================================================
    // VARIABLE DECLARATION - REFERENCES
    // ==================================================

    [Header("Referens")] // Header in Unity Inspector (note: typo in original - "Referens" instead of "References")
    [SerializeField] private PlayerController player; // Reference to the player for targeting and attacking

    // ==================================================
    // VARIABLE DECLARATION - MOVEMENT
    // ==================================================

    [Header("Move")] // Header for movement settings
    [SerializeField] float moveSpeed; // How fast the enemy moves (units per second)
    [SerializeField] private Rigidbody rB; // Rigidbody component for physics-based movement
    [SerializeField] private float chaseRange; // Distance at which enemy starts chasing player
    [SerializeField] private float toClose; // Minimum distance enemy tries to maintain from player (stops advancing)
    [SerializeField] private float strafeAmount; // Random strafe amount for more interesting movement patterns

    // ==================================================
    // VARIABLE DECLARATION - PATROL SYSTEM
    // ==================================================

    [Header("Patrol")] // Header for patrol settings
    [SerializeField] private Transform[] patrolsPoints; // Array of waypoints for patrol route
    [SerializeField] private Transform pointHolder; // Parent GameObject that holds all patrol points
    [HideInInspector] private int currentPatrolPoint; // Index of current patrol waypoint (hidden in Inspector)

    // ==================================================
    // VARIABLE DECLARATION - ATTACK SYSTEM
    // ==================================================

    [Header("Attack")] // Header for attack settings
    [SerializeField] private float attack; // Amount of damage dealt to player per attack
    [SerializeField] private float attackRange; // Distance at which enemy can attack player
    [SerializeField] private float attackCooldown; // Time between attacks (seconds)
    private float lastAttackTime; // Timestamp of last attack (used with Time.time)

    // ==================================================
    // VARIABLE DECLARATION - HEALTH SYSTEM
    // ==================================================

    [Header("EnemyHealth")] // Header for health settings
    [SerializeField] private float currentHealth; // Enemy's current health points
    [HideInInspector] private bool isDeath; // Is the enemy dead? (hidden in Inspector)

    // ==================================================
    // START METHOD
    // ==================================================
    // Start is called once when the game begins (after Awake)
    // Used for initialization
    void Start()
    {
        // Find the player in the scene (if not assigned in Inspector)
        player = FindFirstObjectByType<PlayerController>();

        // Randomize strafe amount for varied movement (makes enemies less predictable)
        // Random value between -0.75 and 0.75 (negative = strafe left, positive = strafe right)
        strafeAmount = Random.Range(-0.75f, 0.75f);

        // Unparent the patrol point holder from this enemy
        // This prevents patrol points from moving with the enemy
        pointHolder.SetParent(null);
    }

    // ==================================================
    // UPDATE METHOD
    // ==================================================
    // Update is called every frame by Unity
    // Contains all AI behavior logic
    void Update()
    {
        // ==================================================
        // DEATH CHECK
        // ==================================================
        // If enemy is dead, stop all AI behavior (corpse shouldn't move/attack)
        if (isDeath == true) return;

        // ==================================================
        // MOVEMENT SETUP
        // ==================================================
        // Store current vertical velocity (gravity/jumping)
        // We preserve this when changing horizontal movement
        float moveY = rB.linearVelocity.y;

        // Calculate distance between enemy and player
        float distance = Vector3.Distance(transform.position, player.transform.position);

        // ==================================================
        // CHASE/ATTACK BEHAVIOR
        // ==================================================
        // If player is within chase range, enter chase/attack mode
        if (distance < chaseRange)
        {
            // Look at player (rotate to face player, but only on Y axis - no tilting up/down)
            transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));

            // ==================================================
            // ATTACK STATE
            // ==================================================
            // If player is within attack range, stop moving and attack
            if (distance <= attackRange)
            {
                // Stop horizontal movement (only preserve vertical velocity)
                rB.linearVelocity = new Vector3(0, moveY, 0);

                // Check if enough time has passed since last attack (cooldown finished)
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    // Attack the player
                    AttackPlayer();
                    // Record current time as last attack time
                    lastAttackTime = Time.time;
                }
            }
            // ==================================================
            // CHASE STATE
            // ==================================================
            // If player is beyond "too close" range, move toward player
            else if (distance > toClose)
            {
                // Move forward with strafing (creates more dynamic movement)
                // Formula: forward direction + (right direction × strafe) × speed
                // Example: if strafeAmount = 0.5, enemy moves forward-right
                rB.linearVelocity = (transform.forward + (transform.right * strafeAmount)) * moveSpeed;
            }
            // ==================================================
            // TOO CLOSE STATE
            // ==================================================
            // If player is too close (but not in attack range), stop moving
            else
            {
                // Stop horizontal movement
                rB.linearVelocity = new Vector3(0, moveY, 0);
            }
        }
        // ==================================================
        // PATROL BEHAVIOR
        // ==================================================
        // If player is outside chase range, enter patrol mode
        else
        {
            // Only patrol if patrol points are defined
            if (patrolsPoints.Length > 0)
            {
                // Check if enemy has reached current patrol point (within 0.25 units)
                if (Vector3.Distance(transform.position, patrolsPoints[currentPatrolPoint].position) < .25f)
                {
                    // Move to next patrol point
                    currentPatrolPoint++;

                    // If we've reached the end of the patrol route, loop back to start
                    if (currentPatrolPoint >= patrolsPoints.Length)
                    {
                        currentPatrolPoint = 0; // Reset to first patrol point
                    }
                }

                // Look at current patrol point (only Y rotation - no tilting)
                transform.LookAt(new Vector3(patrolsPoints[currentPatrolPoint].position.x, transform.position.y, patrolsPoints[currentPatrolPoint].position.z));

                // Move toward current patrol point
                // Separate X and Z movement from Y (gravity) for better control
                rB.linearVelocity = new Vector3(transform.forward.x * moveSpeed, moveY, transform.forward.z * moveSpeed);
            }
            else // No patrol points defined
            {
                // Stand still (only preserve vertical velocity)
                rB.linearVelocity = new Vector3(0, moveY, 0);
            }
        }

        // ==================================================
        // FINAL VELOCITY APPLICATION
        // ==================================================
        // Ensure Y velocity is always preserved (gravity stays consistent)
        // This line might be redundant but ensures vertical velocity isn't accidentally overwritten
        rB.linearVelocity = new Vector3(rB.linearVelocity.x, moveY, rB.linearVelocity.z);
    }

    // ==================================================
    // ATTACK PLAYER METHOD
    // ==================================================
    // This private method handles attacking the player
    // Called when player is in attack range and cooldown has finished
    private void AttackPlayer()
    {
        // Safety check: make sure player reference exists
        if (player != null)
        {
            // Try to get IDamageable component from player
            IDamageable damageable = player.GetComponent<IDamageable>();

            // If player can take damage
            if (damageable != null)
            {
                // Deal damage to player
                damageable.TakeDamage(attack);
            }
        }
    }

    // ==================================================
    // TAKE DAMAGE METHOD
    // ==================================================
    // This public method is called when enemy receives damage
    // Called by weapons/projectiles/hazards that hit the enemy
    //
    // PARAMETERS:
    // float damageToTake - Amount of damage to apply to enemy's health
    public void TakeDamage(float damageToTake)
    {
        // Subtract damage from current health
        currentHealth -= damageToTake;

        // Check if enemy is dead (health at or below 0)
        if (currentHealth <= 0)
        {
            // Mark enemy as dead (stops AI behavior in Update)
            isDeath = true;

            // Stop all movement immediately
            rB.linearVelocity = Vector3.zero;

            // Disable collider so player/bullets can pass through corpse
            GetComponent<Collider>().enabled = false;

            // Remove enemy from the game
            // TODO: Consider adding death animation/effects before destroying
            Destroy(gameObject);
        }
    }
}