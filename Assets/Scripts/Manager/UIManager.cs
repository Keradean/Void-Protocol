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
    [SerializeField] private GameObject CanvasUI; // Reference to the PlayerStats scriptable object

    [Header("Bars")]
    [SerializeField] private Image healthBar;  // Reference to the health bar UI element
    [SerializeField] private Image staminaBar;  // Reference to the mana bar UI element 

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI levelTMP; // Reference to the level text UI element
    [SerializeField] private TextMeshProUGUI healthTMP; // Reference to the level text UI element
    [SerializeField] private TextMeshProUGUI staminaTMP; // Reference to the level text UI element
    [SerializeField] private TextMeshProUGUI ammoTMP; // Reference to the ammo text UI element
    [SerializeField] private TextMeshProUGUI remainigAmmoTMP; // Reference to the ammo text UI element

    private void Update()
    {
        UpdatePlayerUI(); // Call the method to update the player UI elements
    }

    private void UpdatePlayerUI()
    {
        staminaBar.fillAmount = stats.Stamina / stats.MaxStamina;

        levelTMP.text = $"Level {stats.Level}"; // Update the level text with the player's level
        healthTMP.text = $"{stats.Health} / {stats.MaxHealth}"; // Update the health text with the player's current and maximum health 
        staminaTMP.text = $"{Mathf.FloorToInt(stats.Stamina)} / {Mathf.FloorToInt(stats.MaxStamina)}"; // Update the mana text with the player's current and maximum mana
      
    }




}
