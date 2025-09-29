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
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUDIO INTEGRATION ATTRIBUTION:
* [HUMAN-AUTHORED] - Audio Integration Konzept von Julian Gomez
* [AI-ASSISTED] - SoundManager Integration Implementierung
* 
* BEREINIGUNGSNOTIZEN v3.1:
* - Movement Audio Integration durch Julian Gomez hinzugefügt
* - Landing Detection Logic optimiert
* - SoundManager Method-Calls korrigiert
====================================================================
*/

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Referenz")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private WeaponsManager weaponsManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerInteraction playerInteraction;

    [Header("Config Movement Parameters")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintSpeedMultiplier;

    [Header("Config Look Parameters")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float upDownLookRange;

    [Header("Config Jump Parameters")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float gravity;

    [Header("Config Sprint Parameter")]
    [SerializeField] private float sprintCooldown;

    [Header("Config Stamina Parameter")]
    [SerializeField] private float staminaDrain;
    [SerializeField] private float staminaRegen;

    private float currentSprintCooldown;
    private Vector3 currentMovement;
    private float verticalRotation;

    public bool canRotate = true;

    // ENHANCED MOVEMENT AUDIO CONTROL - Added by Julian with AI-Support
    private bool isCurrentlyMoving = false;
    private bool wasMovingLastFrame = false;
    private bool wasGroundedLastFrame = false;

    private bool CanSprint => inputManager.SprintTriggered && stats.Stamina > 0f && currentSprintCooldown <= 0f;
    private float CurrentSpeed => moveSpeed * (CanSprint ? sprintSpeedMultiplier : 1);

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        stats.ResetStats();
        playerInteraction = GetComponent<PlayerInteraction>();
        wasGroundedLastFrame = characterController.isGrounded;
    }

    void Update()
    {
        HandlePausedUnpaused();
        HandleMovement();
        HandleRotation();
        HandleStamina();
        HandleShooting();
        HandleReloading();
        HandleWeaponSwitching();
        HandleInteraction();
    }

    private void HandleMovement()
    {
        Vector3 inputDirection = new Vector3(inputManager.MovementInput.x, 0f, inputManager.MovementInput.y);
        Vector3 calculateMove = transform.TransformVector(inputDirection).normalized;

        currentMovement.x = calculateMove.x * CurrentSpeed;
        currentMovement.z = calculateMove.z * CurrentSpeed;

        // ENHANCED MOVEMENT AUDIO CONTROL - Added by Julian with AI-Support
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

        // Jump and Landing Logic with Enhanced Audio - Julian's Audio Integration
        if (characterController.isGrounded)
        {
            currentMovement.y = -0.5f;

            // Landing Detection - Separate Landing Audio
            if (!wasGroundedLastFrame && characterController.isGrounded)
            {
                // AUDIO INTEGRATION - Added by Julian with AI-Support
                SoundManager.Instance?.PlayLanding(transform.position);
            }

            if (inputManager.JumpTriggered)
            {
                // AUDIO INTEGRATION - Added by Julian with AI-Support
                SoundManager.Instance?.PlayJump(transform.position);
                currentMovement.y = jumpForce;
            }
        }
        else
        {
            currentMovement.y += Physics.gravity.y * gravity * Time.deltaTime;
        }

        characterController.Move(currentMovement * Time.deltaTime);

        // Update frame state tracking
        wasMovingLastFrame = isCurrentlyMoving;
        wasGroundedLastFrame = characterController.isGrounded;
    }

    private void HandleRotation()
    {
        if (!canRotate) return;
        float mouseXRotation = inputManager.RotationInput.x * mouseSensitivity;
        float mouseYRotation = inputManager.RotationInput.y * mouseSensitivity;

        transform.Rotate(0, mouseXRotation, 0);

        verticalRotation = Mathf.Clamp(verticalRotation - mouseYRotation, -upDownLookRange, upDownLookRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleStamina()
    {
        if (currentSprintCooldown > 0f)
            currentSprintCooldown -= Time.deltaTime;

        if (CanSprint)
        {
            stats.Stamina -= staminaDrain * Time.deltaTime;
            if (stats.Stamina <= 0f)
            {
                stats.Stamina = 0f;
                currentSprintCooldown = sprintCooldown;
            }
        }
        else
        {
            if (stats.Stamina < stats.MaxStamina)
            {
                stats.Stamina += staminaRegen * Time.deltaTime;
                if (stats.Stamina > stats.MaxStamina)
                    stats.Stamina = stats.MaxStamina;
            }
        }
    }

    private void HandleShooting()
    {
        if (inputManager.ShootWasPressedThisFrame)
        {
            weaponsManager.Shoot();
        }

        if (inputManager.ShootIsPressed)
        {
            weaponsManager.ShootHeld();
        }
    }

    private void HandleReloading()
    {
        if (inputManager.ReloadTriggered && weaponsManager != null)
        {
            weaponsManager.Reload();
        }
    }

    private void HandleWeaponSwitching()
    {
        if (inputManager.NextTriggered && weaponsManager != null)
        {
            weaponsManager.NextWeapon();
        }

        if (inputManager.PreviousTriggered && weaponsManager != null)
        {
            weaponsManager.PreviousWeapon();
        }
    }

    private void HandleInteraction()
    {
        if (inputManager.InteractTriggered && playerInteraction != null)
        {
            playerInteraction.InteractWithClosest();
        }
    }

    private void HandlePausedUnpaused()
    {
        if (inputManager.PausedTriggered)
        {
            uiManager.PausedUnpaused();
        }
    }

    // ENHANCED AUDIO CLEANUP - Added by Julian with AI-Support
    private void OnDisable()
    {
        // Stop footsteps when player is disabled/destroyed
        SoundManager.Instance?.StopFootsteps();
    }

    private void OnDestroy()
    {
        // Ensure footsteps are stopped when player is destroyed
        SoundManager.Instance?.StopFootsteps();
    }
}