using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerStats stats; // Reference to the PlayerStats scriptable object


    [Header("Bars")]
    [SerializeField] private Image healthBar;  // Reference to the health bar UI element
    [SerializeField] private Image staminaBar;  // Reference to the mana bar UI element 
    [SerializeField] private Image ammoBar;  // Reference to the ammo bar UI element 

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI levelTMP; // Reference to the level text UI element
    [SerializeField] private TextMeshProUGUI healthTMP; // Reference to the level text UI element
    [SerializeField] private TextMeshProUGUI staminaTMP; // Reference to the level text UI element
    [SerializeField] private TextMeshProUGUI ammoTMP; // Reference to the ammo text UI element
    [SerializeField] private TextMeshProUGUI remainingAmmoTMP; // Reference to the ammo text UI element

    private void Update()
    {
        UpdatePlayerUI(); // Call the method to update the player UI elements
    }

    private void UpdatePlayerUI()
    {
        // Update bars
        healthBar.fillAmount = stats.Health / stats.MaxHealth;
        staminaBar.fillAmount = stats.Stamina / stats.MaxStamina;

        // Update ammo bar (current ammo / clip size)
        if (ammoBar != null && stats.ClipSize > 0)
        {
            ammoBar.fillAmount = (float)stats.CurrentAmmo / (float)stats.ClipSize;
            Debug.Log($"Ammo Bar: {stats.CurrentAmmo}/{stats.ClipSize} = {ammoBar.fillAmount}"); // DEBUG
        }
        else
        {
            Debug.LogWarning("AmmoBar is null or ClipSize is 0!"); // DEBUG
        }

        /*// Update oxygen bar if you have one
        if (oxygenBar != null)
        {
            oxygenBar.fillAmount = stats.Oxy / stats.MaxOxy;
        }
        */

        // Update text elements
        levelTMP.text = $"Level {stats.Level}"; // Update the level text with the player's level
        healthTMP.text = $"{Mathf.FloorToInt(stats.Health)}"; // Update the health text with the player's current health 
        ammoTMP.text = $"{stats.CurrentAmmo}"; // Update the ammo text with current ammo
        remainingAmmoTMP.text = $"{stats.RemainingAmmo}"; // Update the remaining ammo text
    }

}
