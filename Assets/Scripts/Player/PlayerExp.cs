using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerExp : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerStats stats; // Reference to PlayerStats script

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) // For testing purposes, press L to add experience
        {
            AddExp(300f); // Add 300 experience points
        }
    }

    public void AddExp(float amount)
    {
        stats.CurrentExp += amount; // Add experience to the player's current experience
        while (stats.CurrentExp >= stats.NextLevelExp) // Check if the current experience is enough to level up
        {
            stats.CurrentExp -= stats.NextLevelExp; // Subtract the experience needed for the next level
            NextLevel(); // Level up the player

        }
    }

    private void NextLevel()
    {
        stats.Level++; // Increment the player's level
        float currentExpRequired = stats.NextLevelExp; // Store the current experience required for the next level
        float newNextLevelExp = Mathf.Round(currentExpRequired + stats.NextLevelExp * (stats.ExpMultiplier / 100f)); // Calculate the new experience required for the next level based on the multiplier
        stats.NextLevelExp = newNextLevelExp; // Update the next level experience requirement

    }

}
