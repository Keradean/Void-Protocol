/*
====================================================================
MainMenu
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
using UnityEngine;

// ==================================================
// MAIN MENU CLASS
// ==================================================
// This class manages the main menu of the game
// It handles button clicks for starting the game and quitting the application
// This script is typically attached to a MainMenu GameObject in the menu scene
public class MainMenu : MonoBehaviour
{
    // ==================================================
    // PLAY GAME METHOD
    // ==================================================
    // This public method is called when the player clicks the "Play" button
    // It loads the main game scene to start playing
    //
    // HOW TO CONNECT:
    // In Unity Inspector, add this method to the Play button's OnClick() event
    public void PlayGame()
    {
        // Load the main game scene using Unity's Scene Management system
        // "MainScene" is the name of the scene as it appears in Unity's Build Settings
        // This will unload the current menu scene and load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    // ==================================================
    // QUIT GAME METHOD
    // ==================================================
    // This public method is called when the player clicks the "Quit" button
    // It closes/exits the application
    //
    // HOW TO CONNECT:
    // In Unity Inspector, add this method to the Quit button's OnClick() event
    public void QuitGame()
    {
        // Quit the application (close the game)
        // NOTE: This only works in a built/compiled game, NOT in Unity Editor
        // In the Unity Editor, the game will not close when this is called
        Application.Quit();

        // Print a farewell message to the console (for debugging/testing)
        // This will show in Unity Editor's Console window
        Debug.Log("Tschüss gell ...");
    }
}