using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable


{
    [Header("Config")] // Configuration settings for the PlayerHealth component
    [SerializeField] private PlayerStats stats; // Reference to the PlayerStats scriptable object

    //private PlayerAnimations playerAnimations; // Reference to the PlayerAnimations script for handling animations

    private void Awake()
    {
       // playerAnimations = GetComponent<PlayerAnimations>(); // Get the PlayerAnimations component attached to the player GameObject
    }
    /// <summary>
    /// Test Area
    /// </summary>
    private void Update()
    {

    }
    public void TakeDamage(float amount)
    {
        stats.Health -= amount; // Reduce health by the damage amount
        if (stats.Health <= 0f)
        {
            stats.Health = 0f;
            PlayerDead(); // Call the method to handle player death

        }
    }

    public void RestoreHealth(float amount)
    {
        stats.Health += amount; // Increase health by the restoration amount
        if (stats.Health > stats.MaxHealth) // Ensure health does not exceed maximum health
        {
            stats.Health = stats.MaxHealth; // Cap health at maximum value
        }
    }
    public bool CanRestoreHealth()
    {
        return stats.Health > 0 && stats.Health < stats.MaxHealth; // Check if the player can restore health
    }

    private void PlayerDead()
    {
        // playerAnimations.SetDeadAnimation(); // Trigger the dead animation
        Debug.Log("I am Dead");
    }

}
