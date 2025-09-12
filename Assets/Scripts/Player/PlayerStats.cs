using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player Stats")] // Create a new PlayerStats asset

public class PlayerStats : ScriptableObject
{
    [Header("Configuration")]
    public int Level; // Player level

    [Header("Health")]
    public float Health; // Player health
    public float MaxHealth; // Maximum player health

    [Header("Stamina")]
    public float Stamina; // Player Stamina
    public float MaxStamina; // Maximum player Stamina

    [Header("Ammo")]
    public int CurrentAmmo; // Player Ammo
    public int RemainingAmmo; // Remaining Player Ammo
    public int ClipSize; // Maximum Ammo 

    [Header("Oxygen")]
    public float Oxy; // Player Oxygen
    public float MaxOxy; // Maximum player Oxygen

    [Header("EXP")]
    public float CurrentExp; // Current experience points
    public float NextLevelExp; // Experience points required for the next level
    public float InitialNextLevelExp; // Initial experience points required for the next level
    [Range(1f, 100f)] public float ExpMultiplier; // Multiplier for experience points

    public void ResetStats()
    {
        Health = MaxHealth; // Reset health to maximum health
        Stamina = MaxStamina; // Reset stamina to maximum stamina
        CurrentAmmo = ClipSize; // Reset current ammo to clip size
        RemainingAmmo = ClipSize * 4; // Reset remaining ammo (*3 full clips)
        Oxy = MaxOxy; // Reset Oxy to maximum Oxy
        CurrentExp = 0f; // Reset current experience points to zero
        NextLevelExp = InitialNextLevelExp; // Reset next level experience points to initial value
    }
}
