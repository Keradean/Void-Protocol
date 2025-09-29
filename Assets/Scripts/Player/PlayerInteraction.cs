/*
====================================================================
PlayerInteraction
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
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// ==================================================
// PLAYER INTERACTION CLASS
// ==================================================
// This class manages player interactions with objects in the game world
// It tracks all nearby interactable objects and determines which one is closest
// When player presses interact button, it interacts with the closest object
public class PlayerInteraction : MonoBehaviour
{
    // ==================================================
    // VARIABLE DECLARATION
    // ==================================================

    // List that stores all interactable objects currently in range of the player
    // Objects are added/removed from this list when they enter/exit the player's interaction zone
    public List<Interactable> interactables;

    // Reference to the interactable object that is currently closest to the player
    // This is the object that will be interacted with when player presses interact button
    private Interactable closestInteractable;

    // ==================================================
    // START METHOD
    // ==================================================
    // Start is called once when the game begins (after Awake)
    // Currently this just finds PlayerInteraction component (but doesn't use it)
    private void Start()
    {
        // Find the PlayerInteraction component in the scene
        // Note: This line doesn't seem to be used for anything currently
        PlayerInteraction playerInteraction = FindFirstObjectByType<PlayerInteraction>();
    }

    // ==================================================
    // INTERACT WITH CLOSEST METHOD
    // ==================================================
    // This public method is called when the player presses the interact button
    // It triggers the interaction with the closest interactable object
    public void InteractWithClosest()
    {
        // The ?. is a "null-conditional operator"
        // It only calls Interaction() if closestInteractable is NOT null
        // This prevents errors if there's no interactable object nearby
        closestInteractable?.Interaction(); // Execute the interaction (open door, pick up item, talk to NPC, etc.)
    }

    // ==================================================
    // UPDATE CLOSEST INTERACTABLE METHOD
    // ==================================================
    // This public method recalculates which interactable object is closest to the player
    // Should be called regularly (e.g. when objects enter/leave interaction range)
    // It also updates UI hints and highlights for the closest object
    public void UpdateClosestInteractable()
    {
        // First, clean up the previous closest interactable
        // Hide its interaction text (e.g. "Press E to open door")
        closestInteractable?.InteractionText(false);
        // Turn off its highlight effect (outline, glow, etc.)
        closestInteractable?.HighlightActive(false);
        // Clear the reference (no closest object yet)
        closestInteractable = null;

        // Start with maximum possible distance
        // float.MaxValue is the largest possible float value in C# (approximately 3.4 × 10^38)
        float closestDistance = float.MaxValue;

        // Loop through ALL interactable objects in the list
        foreach (Interactable interactable in interactables)
        {
            // Safety check: skip if this interactable doesn't exist (was destroyed, null, etc.)
            if (interactable == null) continue;

            // Calculate the distance between player and this interactable object
            // Vector3.Distance calculates straight-line distance in 3D space
            float distance = Vector3.Distance(transform.position, interactable.transform.position);

            // Check if this object is closer than the current closest object
            if (distance < closestDistance)
            {
                // Update the closest distance
                closestDistance = distance; // For example: was 5.0, now 3.2 (this object is closer)
                // Update the closest interactable reference
                closestInteractable = interactable; // This is now the new closest object
            }
        }

        // After checking all objects, show UI for the closest one
        // Show interaction text for the closest object (e.g. "Press E to interact")
        closestInteractable?.InteractionText(true);
        // Turn on highlight effect for the closest object (so player knows what they'll interact with)
        closestInteractable?.HighlightActive(true);
    }
}