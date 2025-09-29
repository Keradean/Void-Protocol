using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerStats stats; // Reference to the PlayerStats scriptable object
    [SerializeField] private WeaponsManager WeaponsManager; // Reference to the PlayerStats scriptable object
    [SerializeField] private PlayerController playerController; // Reference to the PlayerStats scriptable object

    [Header("Bars")]
    [SerializeField] private Image BgBar;  // Reference to the health bar UI element
    [SerializeField] private Image healthBar;  // Reference to the health bar UI element
    [SerializeField] private Image staminaBar;  // Reference to the mana bar UI element 
    [SerializeField] public Image ammoBar;  // Reference to the ammo bar UI element 
    [SerializeField] private Image oxyBar;  // Reference to the Oxy bar UI element 

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI levelTMP; // Reference to the level text UI element
    [SerializeField] private TextMeshProUGUI healthTMP; // Reference to the level text UI element
    [SerializeField] private TextMeshProUGUI staminaTMP; // Reference to the level text UI element
    [SerializeField] public TextMeshProUGUI ammoTMP; // Reference to the level text UI element

    [Header("DeathScreen")]
    [SerializeField] private GameObject showDeathScreen;
   
    [Header("PausedScreen")]
    [SerializeField] private GameObject showPausedScreen;


    private void Update()
    {
        UpdatePlayerUI(); // Call the method to update the player UI elements
    }

    public void UpdatePlayerUI()
    {
        // Update Health & Stamina & Oxy
        if (stats.MaxHealth > 0)
            healthBar.fillAmount = stats.Health / stats.MaxHealth;
        else
            healthBar.fillAmount = 0;

        if (stats.MaxStamina > 0)
            staminaBar.fillAmount = stats.Stamina / stats.MaxStamina;
        else
            staminaBar.fillAmount = 0;

        if (stats.MaxOxy > 0)
            oxyBar.fillAmount = stats.Oxy / stats.MaxOxy;
        else
            oxyBar.fillAmount = 0;

        if (WeaponsManager.ClipSize > 0)
            ammoBar.fillAmount = (float)WeaponsManager.CurrentAmmo / (float)WeaponsManager.ClipSize;
        else
            ammoBar.fillAmount = 0;

        healthTMP.text = $"{Mathf.FloorToInt(stats.Health)}";
        staminaTMP.text = $"{Mathf.FloorToInt(stats.Stamina)}";
        levelTMP.text = $"Level {stats.Level}";
        ammoTMP.text = $"{WeaponsManager.CurrentAmmo} / {WeaponsManager.RemainingAmmo}";
        
    }

    public void ShowDeathScreen()
    {
        showDeathScreen.SetActive(true);
        Time.timeScale = 0;
        playerController.canRotate = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void ShowPausedScreen()
    {
        if (showPausedScreen.activeSelf == false)
        {
            showPausedScreen.SetActive(true);
            Time.timeScale = 0;
            playerController.canRotate = false; 
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }
        else
        {
            showPausedScreen.SetActive(false);
            Time.timeScale = 1;
            playerController.canRotate = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

    }
    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        stats.ResetStats();
    }
    public void ExitGame()
    {
        Debug.Log("Tüdülüü, ihr blöden...!");
        Application.Quit();
    }

    public void BackToMainMenu()
    {
        Debug.Log("ET nach Hause telefonieren..");
        //SceneManager.LoadScene(MainMenu);
    }
    public void PausedUnpaused()
    {
        Debug.Log("It’s not a game. It’s a game!");
        //SceneManager.LoadScene(MainMenu);
        ShowPausedScreen();
    }
}
