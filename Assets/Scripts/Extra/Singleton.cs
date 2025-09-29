/*
====================================================================
Singleton
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==================================================
// SINGLETON CLASS (GENERIC BASE CLASS)
// ==================================================
// This is a generic Singleton pattern implementation for Unity
// A Singleton ensures only ONE instance of a class exists in the entire game
// This is useful for managers like GameManager, AudioManager, UIManager, etc.
//
// WHAT IS <T>?
// <T> is a "generic type parameter" - it means this class can work with ANY type
// "where T : MonoBehaviour" means T must be a MonoBehaviour (a Unity component)
//
// HOW TO USE:
// Instead of "public class MyManager : MonoBehaviour"
// Write "public class MyManager : Singleton<MyManager>"
// Then you can access it from anywhere with: MyManager.Instance
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // ==================================================
    // SINGLETON INSTANCE PROPERTY
    // ==================================================
    // This static property stores the single instance of type T
    // "static" means it belongs to the CLASS itself, not to individual objects
    // { get; private set; } means:
    //   - Any script can READ the instance (public get)
    //   - Only this class can WRITE/change the instance (private set)
    public static T Instance { get; private set; } // Singleton instance of type T (accessible from anywhere)

    // ==================================================
    // AWAKE METHOD
    // ==================================================
    // Awake is called automatically by Unity when the object is created
    // "protected virtual" means:
    //   - protected: Only this class and classes that inherit from it can see this method
    //   - virtual: Child classes can override this method if they need custom Awake logic
    protected virtual void Awake()
    {
        // Set the singleton instance to this object
        // "this as T" casts this object to type T (converts it to the correct type)
        // Example: If T is GameManager, this converts "this" into a GameManager reference
        Instance = this as T; // Set the singleton instance to this instance

        // Note: This basic implementation doesn't check for duplicate instances
        // In more advanced versions, you might add code here to:
        // - Destroy duplicate instances if they exist
        // - Make this object persist between scenes with DontDestroyOnLoad()
    }
}