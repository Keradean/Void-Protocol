/*
====================================================================
UIManager
====================================================================
Project: Space Colony Game
Course: PIP
Script-Developer: Dennis De Col 
*
WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
Diese detaillierte Authorship-Dokumentation ist für die
akademische Bewertung erforderlich und darf nicht entfernt werden!
*
====================================================================
*/
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ==================================================
// UI MANAGER CLASS
// ==================================================
// This class manages all User Interface (UI) elements in the game
// It updates health bars, stamina bars, ammo counters, and handles game screens (death, pause)
// This is the central hub for all UI-related functionality
public class UIManager : MonoBehaviour
{
    // ==================================================
    // VARIABLE DECLARATION - REFERENCES
    // ==================================================

    [Header("Reference")] // Header in Unity Inspector for organizing references
    [SerializeField] private PlayerStats stats; // Reference to the PlayerStats scriptable object (stores health, stamina, level data)
    [SerializeField] private WeaponsManager WeaponsManager; // Reference to the WeaponsManager (handles weapon and ammo data)
    [SerializeField] private PlayerController playerController; // Reference to the PlayerController (controls player movement and camera)

    // ==================================================
    // VARIABLE DECLARATION - UI BARS (VISUAL FILLS)
    // ==================================================

    [Header("Bars")] // Header for UI bar elements
    [SerializeField] private Image BgBar;  // Background bar image (not currently used in code)
    [SerializeField] private Image healthBar;  // Visual fill image for health bar (fills from 0 to 1 based on current health)
    [SerializeField] private Image staminaBar;  // Visual fill image for stamina bar (fills from 0 to 1 based on current stamina)
    [SerializeField] public Image ammoBar;  // Visual fill image for ammo bar (fills from 0 to 1 based on current ammo in clip)
    [SerializeField] private Image oxyBar;  // Visual fill image for oxygen bar (fills from 0 to 1 based on current oxygen)

    // ==================================================
    // VARIABLE DECLARATION - UI TEXT ELEMENTS
    // ==================================================

    [Header("Text")] // Header for text UI elements
    [SerializeField] private TextMeshProUGUI levelTMP; // Text display for player's level (e.g. "Level 5")
    [SerializeField] private TextMeshProUGUI healthTMP; // Text display for current health value (e.g. "75")
    [SerializeField] private TextMeshProUGUI staminaTMP; // Text display for current stamina value (e.g. "100")
    [SerializeField] public TextMeshProUGUI ammoTMP; // Text display for ammo count (e.g. "15 / 120")

    // ==================================================
    // VARIABLE DECLARATION - GAME SCREENS
    // ==================================================

    [Header("DeathScreen")] // Header for death screen
    [SerializeField] private GameObject showDeathScreen; // GameObject containing the death/game over screen UI

    [Header("PausedScreen")] // Header for pause screen
    [SerializeField] private GameObject showPausedScreen; // GameObject containing the pause menu UI

    // ==================================================
    // UPDATE METHOD
    // ==================================================
    // Update is called every frame by Unity
    // Continuously updates all UI elements to reflect current game state
    private void Update()
    {
        UpdatePlayerUI(); // Call the method to update the player UI elements each frame
    }

    // ==================================================
    // UPDATE PLAYER UI METHOD
    // ==================================================
    // This public method updates all UI bars and text displays
    // It reads data from PlayerStats and WeaponsManager and updates the visual elements
    public void UpdatePlayerUI()
    {
        // ==================================================
        // UPDATE HEALTH BAR
        // ==================================================
        // Calculate fill amount (0.0 to 1.0) based on current health vs max health
        if (stats.MaxHealth > 0) // Safety check: prevent division by zero
            healthBar.fillAmount = stats.Health / stats.MaxHealth; // Example: 75/100 = 0.75 (75% filled)
        else
            healthBar.fillAmount = 0; // If max health is 0, empty the bar

        // ==================================================
        // UPDATE STAMINA BAR
        // ==================================================
        // Calculate fill amount (0.0 to 1.0) based on current stamina vs max stamina
        if (stats.MaxStamina > 0) // Safety check: prevent division by zero
            staminaBar.fillAmount = stats.Stamina / stats.MaxStamina; // Example: 50/100 = 0.5 (50% filled)
        else
            staminaBar.fillAmount = 0; // If max stamina is 0, empty the bar

        // ==================================================
        // UPDATE OXYGEN BAR
        // ==================================================
        // Calculate fill amount (0.0 to 1.0) based on current oxygen vs max oxygen
        if (stats.MaxOxy > 0) // Safety check: prevent division by zero
            oxyBar.fillAmount = stats.Oxy / stats.MaxOxy; // Example: 80/100 = 0.8 (80% filled)
        else
            oxyBar.fillAmount = 0; // If max oxygen is 0, empty the bar

        // ==================================================
        // UPDATE AMMO BAR
        // ==================================================
        // Calculate fill amount (0.0 to 1.0) based on current ammo in clip vs clip size
        if (WeaponsManager.ClipSize > 0) // Safety check: prevent division by zero
            // Cast to float because CurrentAmmo and ClipSize might be integers
            // Example: 15/30 = 0.5 (50% filled - half a clip remaining)
            ammoBar.fillAmount = (float)WeaponsManager.CurrentAmmo / (float)WeaponsManager.ClipSize;
        else
            ammoBar.fillAmount = 0; // If clip size is 0, empty the bar

        // ==================================================
        // UPDATE TEXT DISPLAYS
        // ==================================================
        // $"{...}" is string interpolation - inserts variables into text

        // Display health as whole number (no decimals)
        // Mathf.FloorToInt rounds down to nearest integer (75.9 becomes 75)
        //healthTMP.text = $"{Mathf.FloorToInt(stats.Health)}";

        // Display stamina as whole number (no decimals)
        //staminaTMP.text = $"{Mathf.FloorToInt(stats.Stamina)}";

        // Display player level (e.g. "Level 5")
        levelTMP.text = $"Level {stats.Level}";

        // Display ammo count (e.g. "15 / 120" = 15 in clip, 120 remaining)
        ammoTMP.text = $"{WeaponsManager.CurrentAmmo} / {WeaponsManager.RemainingAmmo}";
    }

    // ==================================================
    // SHOW DEATH SCREEN METHOD
    // ==================================================
    // This public method is called when the player dies
    // It displays the death/game over screen and pauses the game
    public void ShowDeathScreen()
    {
        // Enable/show the death screen GameObject
        showDeathScreen.SetActive(true);

        // Pause the game by setting time scale to 0 (stops all time-based calculations)
        Time.timeScale = 0;

        // Disable camera rotation so player can't look around while dead
        playerController.canRotate = false;

        // Unlock cursor from center of screen (allow cursor to move freely)
        Cursor.lockState = CursorLockMode.None;

        // Make cursor visible so player can click UI buttons
        Cursor.visible = true;
    }

    // ==================================================
    // SHOW PAUSED SCREEN METHOD
    // ==================================================
    // This public method toggles the pause menu on/off
    // It's called when player presses the pause button (usually ESC)
    public void ShowPausedScreen()
    {
        // Check if pause menu is currently hidden
        if (showPausedScreen.activeSelf == false)
        {
            // PAUSE THE GAME
            showPausedScreen.SetActive(true); // Show pause menu
            Time.timeScale = 0; // Freeze game time
            playerController.canRotate = false; // Disable camera rotation
            Cursor.lockState = CursorLockMode.None; // Unlock cursor
            Cursor.visible = true; // Show cursor
        }
        else
        {
            // UNPAUSE THE GAME
            showPausedScreen.SetActive(false); // Hide pause menu
            Time.timeScale = 1; // Resume game time (normal speed)
            playerController.canRotate = true; // Enable camera rotation
            Cursor.lockState = CursorLockMode.Locked; // Lock cursor to center
            Cursor.visible = false; // Hide cursor
        }
    }

    // ==================================================
    // RESTART GAME METHOD
    // ==================================================
    // This public method restarts the current level/scene
    // Called from death screen or pause menu "Restart" button
    public void RestartGame()
    {
        // Resume game time (in case it was paused)
        Time.timeScale = 1;

        // Reload the current scene (restart level)
        // GetActiveScene().name gets the name of the currently loaded scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Reset player stats to default values (full health, full stamina, etc.)
        stats.ResetStats();
    }

    // ==================================================
    // EXIT GAME METHOD
    // ==================================================
    // This public method closes/quits the application
    // Called from pause menu or death screen "Exit" button
    public void ExitGame()
    {
        // Print farewell message to console (for debugging)
        Debug.Log("Tüdülüü, ihr blöden...!");

        // Quit the application (only works in built game, not in Unity Editor)
        Application.Quit();
    }

    // ==================================================
    // BACK TO MAIN MENU METHOD
    // ==================================================
    // This public method should return player to main menu
    // Currently not fully implemented (commented out)
    public void BackToMainMenu()
    {
        // Print message to console (for debugging)
        Debug.Log("ET nach Hause telefonieren..");

        // TODO: Load main menu scene (currently commented out)
        //SceneManager.LoadScene(MainMenu);
    }

    // ==================================================
    // PAUSED/UNPAUSED METHOD
    // ==================================================
    // This public method is called when player presses pause button
    // It's a wrapper that calls ShowPausedScreen()
    public void PausedUnpaused()
    {
        // Print message to console (for debugging)
        Debug.Log("It's not a game. It's a game!");

        // TODO: This commented line doesn't make sense here (would load main menu)
        //SceneManager.LoadScene(MainMenu);

        // Toggle pause menu on/off
        ShowPausedScreen();
    }
}