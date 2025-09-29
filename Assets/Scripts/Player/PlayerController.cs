/*
====================================================================
* PlayerController.cs - Enhanced Movement Audio Integration v3.1
====================================================================
* Project: Space Colony Game
* Course: PIP
* Script-Developer: Dennis De Col
* Created: 2025-08-25
* Last Modified: 2025-09-28
* Version: v3.1 - Movement Audio Fixes Applied
*
* WICHTIG: KOMMENTIERUNG NICHT L�SCHEN!
* Diese detaillierte Authorship-Dokumentation ist f�r die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUDIO INTEGRATION ATTRIBUTION:
* [HUMAN-AUTHORED] - Audio Integration Konzept von Julian Gomez
* [AI-ASSISTED] - SoundManager Integration Implementierung
* 
* BEREINIGUNGSNOTIZEN v3.1:
* - Movement Audio Integration durch Julian Gomez hinzugef�gt
* - Landing Detection Logic optimiert
* - SoundManager Method-Calls korrigiert
====================================================================
*/

/*
====================================================================
PlayerController
====================================================================
Project: Space Colony Game
Course: PIP
Script-Developer: Dennis De Col 
*
WICHTIG: KOMMENTIERUNG NICHT L�SCHEN!
Diese detaillierte Authorship-Dokumentation ist f�r die
akademische Bewertung erforderlich und darf nicht entfernt werden!
*
Dieses Script wurde in einer voherigen Abgabe verwendet. 
The Last Refuge / 3D Interactive
====================================================================
*/
using UnityEngine;

// ==================================================
// PLAYER CONTROLLER CLASS
// ==================================================
// This class controls all player movement, camera rotation, shooting, and interactions
// It connects many different systems together (movement, weapons, UI, input, etc.)
public class PlayerController : MonoBehaviour
{
    // ==================================================
    // VARIABLE DECLARATION - REFERENCES
    // ==================================================

    [Header("Referenz")] // Header in Unity Inspector for organizing references
    [SerializeField] private CharacterController characterController; // Unity's built-in component for character movement
    [SerializeField] private WeaponsManager weaponsManager; // Manages all weapon-related functions (shooting, reloading, switching)
    [SerializeField] private UIManager uiManager; // Manages the user interface (menus, HUD, etc.)
    [SerializeField] private Camera mainCamera; // Reference to the main camera for looking around
    [SerializeField] private InputManager inputManager; // Gets all player inputs (keyboard, mouse, controller)
    [SerializeField] private PlayerStats stats; // Stores player statistics (health, stamina, etc.)
    [SerializeField] private PlayerInteraction playerInteraction; // Handles interactions with objects in the game world

    // ==================================================
    // VARIABLE DECLARATION - MOVEMENT PARAMETERS
    // ==================================================

    [Header("Config Movement Parameters")] // Header for movement settings
    [SerializeField] private float moveSpeed; // Base walking speed of the player
    [SerializeField] private float sprintSpeedMultiplier; // How much faster the player moves when sprinting (e.g. 2.0 = twice as fast)

    // ==================================================
    // VARIABLE DECLARATION - CAMERA PARAMETERS
    // ==================================================

    [Header("Config Look Parameters")] // Header for camera/look settings
    [SerializeField] private float mouseSensitivity; // How fast the camera rotates when moving the mouse
    [SerializeField] private float upDownLookRange; // Maximum angle for looking up and down (prevents looking too far up/down)

    // ==================================================
    // VARIABLE DECLARATION - JUMP PARAMETERS
    // ==================================================

    [Header("Config Jump Parameters")] // Header for jump settings
    [SerializeField] private float jumpForce; // How high the player can jump
    [SerializeField] private float gravity; // Custom gravity multiplier (how fast player falls)

    private bool wasGroundedLastFrame = false;
    // ==================================================
    // VARIABLE DECLARATION - SPRINT PARAMETERS
    // ==================================================

    [Header("Config Sprint Parameter")] // Header for sprint settings
    [SerializeField] private float sprintCooldown; // Time player must wait after stamina runs out before sprinting again

    // ==================================================
    // VARIABLE DECLARATION - STAMINA PARAMETERS
    // ==================================================

    [Header("Config Stamina Parameter")] // Header for stamina settings
    [SerializeField] private float staminaDrain; // How fast stamina decreases when sprinting
    [SerializeField] private float staminaRegen; // How fast stamina recovers when not sprinting

    // ==================================================
    // PRIVATE VARIABLES
    // ==================================================

    private float currentSprintCooldown; // Timer that counts down the sprint cooldown
    private Vector3 currentMovement; // Stores the current movement direction and speed (x, y, z)
    private float verticalRotation; // Stores the up/down camera rotation angle

    public bool canRotate = true; // Controls whether the camera can rotate (public so other scripts can disable it)

    // RESPONSIVE AUDIO CONTROL - Added by Julian with AI-Support
    private bool isCurrentlyMoving = false; // Is the player moving right now?
    private bool wasMovingLastFrame = false; // Was the player moving in the previous frame?

    // ==================================================
    // PROPERTIES (SHORTCUTS)
    // ==================================================

    // Property that checks if player CAN sprint right now
    // Requirements: Sprint button pressed AND has stamina AND cooldown is finished
    private bool CanSprint => inputManager.SprintTriggered && stats.Stamina > 0f && currentSprintCooldown <= 0f;

    // Property that calculates current movement speed
    // If sprinting: base speed � multiplier, otherwise: just base speed
    private float CurrentSpeed => moveSpeed * (CanSprint ? sprintSpeedMultiplier : 1);

    // ==================================================
    // START METHOD
    // ==================================================
    // Start is called once when the game begins (after Awake)
    // Used for initialization
    void Start()
    {
        // Lock the mouse cursor to the center of the screen (for FPS camera control)
        Cursor.lockState = CursorLockMode.Locked;
        // Make the cursor invisible
        Cursor.visible = false;
        // Reset player stats to default values (full health, full stamina, etc.)
        stats.ResetStats();
        wasGroundedLastFrame = characterController.isGrounded;
        // Find the InputManager in the scene (currently not used after this line)
        InputManager shootAction = FindFirstObjectByType<InputManager>();
        // Get the PlayerInteraction component attached to this GameObject
        playerInteraction = GetComponent<PlayerInteraction>();
    }

    // ==================================================
    // UPDATE METHOD
    // ==================================================
    // Update is called every frame by Unity
    // This is where all the game logic happens continuously
    void Update()
    {
        // Check if player pressed pause button
        HandlePausedUnpaused();

        // Handle player walking/running/jumping
        HandleMovement();
        // Handle camera rotation (looking around)
        HandleRotation();
        // Handle stamina drain and regeneration
        HandleStamina();
        // Handle weapon firing
        HandleShooting();
        // Handle weapon reloading
        HandleReloading();
        // Handle switching between weapons
        HandleWeaponSwitching();
        // Handle interacting with objects
        HandleInteraction();
    }

    // ==================================================
    // HANDLE MOVEMENT METHOD
    // ==================================================
    // This method controls all player movement: walking, sprinting, jumping, and gravity
    private void HandleMovement()
    {
        // Get input from WASD or joystick (x = left/right, y = forward/backward)
        // We use y for forward/backward because that's how Unity's input system works
        Vector3 inputDirection = new Vector3(inputManager.MovementInput.x, 0f, inputManager.MovementInput.y);

        // Transform the input from local space to world space
        // This makes the player move relative to where they're facing
        Vector3 calculateMove = transform.TransformVector(inputDirection).normalized;

        // Set horizontal movement speed (left/right and forward/backward)
        currentMovement.x = calculateMove.x * CurrentSpeed;
        currentMovement.z = calculateMove.z * CurrentSpeed;

        // RESPONSIVE AUDIO CONTROL - Added by Julian with AI-Support
        // Check if player is moving (input greater than 0.1 to avoid tiny accidental inputs) AND on the ground
        isCurrentlyMoving = inputDirection.magnitude > 0.1f && characterController.isGrounded;

        // Immediate Start/Stop Audio Response - Julian's Audio Integration
        if (isCurrentlyMoving && !wasMovingLastFrame)
        {
            // START footsteps immediately when movement begins
            SoundManager.Instance?.StartFootsteps(CanSprint, transform.position);
        }
        else if (!isCurrentlyMoving && wasMovingLastFrame)
        {
            // STOP footsteps immediately when movement ends
            SoundManager.Instance?.StopFootsteps();
        }
        // CONTINUE footsteps with updated position and speed if moving
        else if (isCurrentlyMoving && wasMovingLastFrame)
        {
            SoundManager.Instance?.StartFootsteps(CanSprint, transform.position);
        }

        // Check if player is on the ground
        if (characterController.isGrounded)
        {
            // Apply small downward force to keep player grounded (prevents floating)
            currentMovement.y = -0.5f;

            // Check if player pressed jump button
            // Landing Detection - Separate Landing Audio
            if (!wasGroundedLastFrame && characterController.isGrounded)
            {
                // AUDIO INTEGRATION - Added by Julian with AI-Support
                SoundManager.Instance?.PlayLanding(transform.position);
            }

            if (inputManager.JumpTriggered)
            {
                // AUDIO INTEGRATION - Added by Julian with AI-Support
                // Play jump sound effect
                SoundManager.Instance?.PlayJump(transform.position);
                // Apply upward force for jumping
                currentMovement.y = jumpForce;
            }
        }
        else // Player is in the air
        {
            // Apply gravity to pull player down
            // Physics.gravity.y is Unity's default gravity, multiplied by our custom gravity value
            currentMovement.y += Physics.gravity.y * gravity * Time.deltaTime;
        }

        // Actually move the character controller
        // Time.deltaTime makes movement frame-rate independent (same speed on all computers)
        characterController.Move(currentMovement * Time.deltaTime);

        // Remember movement state for next frame (for audio system)
        wasMovingLastFrame = isCurrentlyMoving;
        wasGroundedLastFrame = characterController.isGrounded;
    }

    // ==================================================
    // HANDLE ROTATION METHOD
    // ==================================================
    // This method controls camera rotation (looking around with mouse or right stick)
    private void HandleRotation()
    {
        // If rotation is disabled, exit this method early
        if (!canRotate) return;

        // Get horizontal mouse movement (left/right) and multiply by sensitivity
        float mouseXRotation = inputManager.RotationInput.x * mouseSensitivity;
        // Get vertical mouse movement (up/down) and multiply by sensitivity
        float mouseYRotation = inputManager.RotationInput.y * mouseSensitivity;

        // Rotate the entire player body left/right (Y axis rotation)
        transform.Rotate(0, mouseXRotation, 0);

        // Calculate up/down camera rotation
        // Clamp prevents looking too far up or down (stays within upDownLookRange)
        verticalRotation = Mathf.Clamp(verticalRotation - mouseYRotation, -upDownLookRange, upDownLookRange);
        // Apply the vertical rotation only to the camera (not the whole player body)
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    // ==================================================
    // HANDLE STAMINA METHOD
    // ==================================================
    // This method manages stamina drain when sprinting and regeneration when not sprinting
    private void HandleStamina()
    {
        // Count down the sprint cooldown timer
        if (currentSprintCooldown > 0f)
            currentSprintCooldown -= Time.deltaTime;

        // If player is currently sprinting
        if (CanSprint)
        {
            // Drain stamina over time
            stats.Stamina -= staminaDrain * Time.deltaTime;
            // If stamina runs out
            if (stats.Stamina <= 0f)
            {
                // Set stamina to exactly 0 (prevent negative values)
                stats.Stamina = 0f;
                // Start cooldown timer (player must wait before sprinting again)
                currentSprintCooldown = sprintCooldown;
            }
        }
        else // Player is NOT sprinting
        {
            // If stamina is not full yet
            if (stats.Stamina < stats.MaxStamina)
            {
                // Regenerate stamina over time
                stats.Stamina += staminaRegen * Time.deltaTime;
                // If regeneration goes over max, cap it at max
                if (stats.Stamina > stats.MaxStamina)
                    stats.Stamina = stats.MaxStamina;
            }
        }
    }

    // ==================================================
    // HANDLE SHOOTING METHOD
    // ==================================================
    // This method handles weapon shooting (both single shot and automatic fire)
    private void HandleShooting()
    {
        // Check if shoot button was pressed THIS frame (for semi-automatic weapons)
        if (inputManager.ShootWasPressedThisFrame)
        {
            weaponsManager.Shoot();
        }

        // Check if shoot button is being HELD down (for automatic weapons)
        if (inputManager.ShootIsPressed)
        {
            weaponsManager.ShootHeld();
        }
    }

    // ==================================================
    // HANDLE RELOADING METHOD
    // ==================================================
    // This method handles weapon reloading when reload button is pressed
    private void HandleReloading()
    {
        // Check if reload button was pressed AND weaponsManager exists
        if (inputManager.ReloadTriggered && weaponsManager != null)
        {
            weaponsManager.Reload();
        }
    }

    // ==================================================
    // HANDLE WEAPON SWITCHING METHOD
    // ==================================================
    // This method handles switching to next/previous weapon
    private void HandleWeaponSwitching()
    {
        // Check if "next weapon" button was pressed AND weaponsManager exists
        if (inputManager.NextTriggered && weaponsManager != null)
        {
            weaponsManager.NextWeapon();
        }

        // Check if "previous weapon" button was pressed AND weaponsManager exists
        if (inputManager.PreviousTriggered && weaponsManager != null)
        {
            weaponsManager.PreviousWeapon();
        }
    }

    // ==================================================
    // HANDLE INTERACTION METHOD
    // ==================================================
    // This method handles interacting with objects in the game world (doors, items, NPCs, etc.)
    private void HandleInteraction()
    {
        // Check if interact button was pressed AND playerInteraction component exists
        if (inputManager.InteractTriggered && playerInteraction != null)
        {
            // Try to interact with the closest interactable object
            playerInteraction.InteractWithClosest();
        }
    }

    // ==================================================
    // HANDLE PAUSED/UNPAUSED METHOD
    // ==================================================
    // This method handles pausing and unpausing the game
    private void HandlePausedUnpaused()
    {
        // Check if pause button was pressed
        if (inputManager.PausedTriggered)
        {
            // Print message to console (for debugging)
            Debug.Log("Take your time...");
            // Toggle pause state (if paused, unpause; if unpaused, pause)
            uiManager.PausedUnpaused();
        }
    }

    // ==================================================
    // ONDISABLE METHOD
    // ==================================================
    // Called automatically when this GameObject is disabled
    // RESPONSIVE AUDIO CLEANUP - Added by Julian with AI-Support
    // ENHANCED AUDIO CLEANUP - Added by Julian with AI-Support
    private void OnDisable()
    {
        // Stop footsteps when player is disabled/destroyed
        SoundManager.Instance?.StopFootsteps();
    }

    // ==================================================
    // ONDESTROY METHOD
    // ==================================================
    // Called automatically when this GameObject is destroyed/removed from the scene
    private void OnDestroy()
    {
        // Ensure footsteps are stopped when player is destroyed
        SoundManager.Instance?.StopFootsteps();
    }
}