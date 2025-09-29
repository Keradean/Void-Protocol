/*
====================================================================
* Interactable
====================================================================
* Project: Void Protocol
* Course: PIP
* Script-Developer: Dennis De Col
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
====================================================================
*/
using TMPro;
using UnityEngine;

// ==================================================
// INTERACTABLE CLASS
// ==================================================
// This is a base class for all interactable objects in the game
// Objects that inherit from this can be interacted with by the player (doors, items, NPCs, etc.)
//
// KEY FEATURES:
// - Visual highlight when player is near (material swap)
// - Interaction text display ("Press E to interact")
// - Automatic detection when player enters/exits range (trigger collider)
// - Virtual Interaction() method that child classes can override
//
// HOW IT WORKS:
// 1. Player enters trigger collider -> adds to PlayerInteraction's list
// 2. PlayerInteraction determines which interactable is closest
// 3. Closest interactable gets highlighted and shows interaction text
// 4. Player presses interact button -> calls Interaction() method
//
// USAGE:
// Create child classes that inherit from Interactable (e.g. Door : Interactable)
// Override the Interaction() method to define what happens when player interacts
public class Interactable : MonoBehaviour
{
    // ==================================================
    // VARIABLE DECLARATION - VISUAL HIGHLIGHT
    // ==================================================

    // Material used to highlight the object when player is nearby
    // Typically a glowing or outlined material to show it's interactable
    [SerializeField] private Material highlightMaterial;

    // MeshRenderer component that displays the object's 3D model
    // We change its material to create the highlight effect
    [SerializeField] private MeshRenderer mesh;

    // The object's original material (stored so we can restore it)
    private Material defaultMaterial;

    // ==================================================
    // VARIABLE DECLARATION - INTERACTION UI
    // ==================================================

    [Header("Text")] // Header in Unity Inspector
    // GameObject containing the interaction prompt text (e.g. "Press E to interact")
    // This is typically a world-space UI element or TextMeshPro object
    [SerializeField] private GameObject interactTMP;

    // ==================================================
    // START METHOD
    // ==================================================
    // Start is called once when the game begins (after Awake)
    // Initializes components and stores default material
    private void Start()
    {
        // If MeshRenderer wasn't assigned in Inspector, try to get it from this GameObject
        if (mesh == null) mesh = GetComponent<MeshRenderer>();

        // Store the original material so we can restore it later
        defaultMaterial = mesh.material;

        // Debug log to confirm interactable is initialized (currently commented out)
        // Debug.Log("Interactable" + gameObject.name);
    }

    // ==================================================
    // INTERACTION METHOD (VIRTUAL)
    // ==================================================
    // This virtual method is called when the player interacts with this object
    // "virtual" means child classes can override this to define custom behavior
    //
    // EXAMPLES OF OVERRIDES:
    // - Door: opens/closes the door
    // - Item: adds item to player inventory
    // - NPC: starts dialogue conversation
    // - Button: activates a mechanism
    //
    // Base implementation is empty - child classes must override to add functionality
    public virtual void Interaction()
    {
        // Empty by default - override in child classes to add interaction behavior
    }

    // ==================================================
    // HIGHLIGHT ACTIVE METHOD
    // ==================================================
    // This public method toggles the visual highlight on/off
    // Called by PlayerInteraction when this becomes/stops being the closest interactable
    //
    // PARAMETERS:
    // bool active - true = apply highlight, false = restore default material
    public void HighlightActive(bool active)
    {
        // Safety check: make sure mesh renderer exists
        if (mesh == null) return;

        // If activating highlight AND highlight material exists
        if (active && highlightMaterial != null)
            mesh.material = highlightMaterial; // Apply highlight material (glow, outline, etc.)
        // If deactivating highlight AND default material exists
        else if (!active && defaultMaterial != null)
            mesh.material = defaultMaterial; // Restore original material
    }

    // ==================================================
    // INTERACTION TEXT METHOD
    // ==================================================
    // This public method toggles the interaction text prompt on/off
    // Called by PlayerInteraction to show "Press E to interact" text
    //
    // PARAMETERS:
    // bool active - true = show text, false = hide text
    public void InteractionText(bool active)
    {
        // Safety check: make sure interaction text GameObject exists
        if (interactTMP == null) return;

        // If activating text AND text GameObject exists
        if (active && interactTMP != null)
        {
            interactTMP.SetActive(true); // Show interaction prompt
        }
        // If deactivating text AND text GameObject exists
        else if (!active && interactTMP != null)
        {
            interactTMP.SetActive(false); // Hide interaction prompt
        }
    }

    // ==================================================
    // ON TRIGGER ENTER METHOD
    // ==================================================
    // Called automatically by Unity when another collider enters this object's trigger
    // Adds this interactable to the player's list of nearby interactables
    //
    // PARAMETERS:
    // Collider other - The collider that entered the trigger (hopefully the player)
    private void OnTriggerEnter(Collider other)
    {
        // Try to get PlayerInteraction component from the entering object
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();

        // If entering object doesn't have PlayerInteraction, it's not the player (exit early)
        if (playerInteraction == null) return;

        // Add this interactable to the player's list of nearby interactables
        playerInteraction.interactables.Add(this);

        // Recalculate which interactable is closest (might be this one!)
        playerInteraction.UpdateClosestInteractable();
    }

    // ==================================================
    // ON TRIGGER EXIT METHOD
    // ==================================================
    // Called automatically by Unity when another collider exits this object's trigger
    // Removes this interactable from the player's list of nearby interactables
    //
    // PARAMETERS:
    // Collider other - The collider that exited the trigger (hopefully the player)
    private void OnTriggerExit(Collider other)
    {
        // Try to get PlayerInteraction component from the exiting object
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();

        // If exiting object doesn't have PlayerInteraction, it's not the player (exit early)
        if (playerInteraction == null) return;

        // Remove this interactable from the player's list of nearby interactables
        playerInteraction.interactables.Remove(this);

        // Recalculate which interactable is closest (won't be this one anymore)
        playerInteraction.UpdateClosestInteractable();
    }

    // ==================================================
    // ON DESTROY METHOD
    // ==================================================
    // Called automatically by Unity when this GameObject is destroyed
    // Ensures this interactable is removed from player's list (prevents null reference errors)
    private void OnDestroy()
    {
        // Find the PlayerInteraction in the scene
        PlayerInteraction playerInteraction = FindFirstObjectByType<PlayerInteraction>();

        // If PlayerInteraction exists AND this interactable is in its list
        if (playerInteraction != null && playerInteraction.interactables.Contains(this))
        {
            // Remove this interactable from the list (cleanup)
            playerInteraction.interactables.Remove(this);

            // Recalculate closest interactable (this one no longer exists)
            playerInteraction.UpdateClosestInteractable();
        }
    }
}