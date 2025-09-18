using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Config")] // Configuration settings for the Player
    [SerializeField] private PlayerStats stats; // Reference to the PlayerStats scriptable object

    public PlayerStats Stats => stats; // Public property to access PlayerStats
    public PlayerHealth PlayerHealth { get; private set; } // Property to access PlayerHealth, which is initialized in Awake

    //private PlayerAnimations animations; // Reference to PlayerAnimations for handling animations


    private void Awake()
    {
        PlayerHealth = GetComponent<PlayerHealth>(); // Get the PlayerHealth component attached to this GameObject
    }
    private void Update()
    {

    }
    public void ResetStats()
    {
        stats.ResetStats(); // Call the ResetStats method from PlayerStats to reset player stats
    }

}
