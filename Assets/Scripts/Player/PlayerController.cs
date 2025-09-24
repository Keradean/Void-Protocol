using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Referenz")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private WeaponsManager weaponsManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerStats stats;

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

    private bool CanSprint => inputManager.SprintTriggered && stats.Stamina > 0f && currentSprintCooldown <= 0f;
    private float CurrentSpeed => moveSpeed * (CanSprint ? sprintSpeedMultiplier : 1);

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        stats.ResetStats();
        InputManager shootAction = FindFirstObjectByType<InputManager>();

    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleStamina();
        HandleShooting();
        HandleReloading();
        HandleWeaponSwitching();
    }

    private void HandleMovement()
    {
        Vector3 inputDirection = new Vector3(inputManager.MovementInput.x, 0f, inputManager.MovementInput.y);
        Vector3 calculateMove = transform.TransformVector(inputDirection).normalized;

        currentMovement.x = calculateMove.x * CurrentSpeed;
        currentMovement.z = calculateMove.z * CurrentSpeed;

        if (characterController.isGrounded)
        {
            currentMovement.y = -0.5f;

            if (inputManager.JumpTriggered)
            {
                currentMovement.y = jumpForce;
            }
        }
        else
        {
            currentMovement.y += Physics.gravity.y * gravity * Time.deltaTime;
        }

        characterController.Move(currentMovement * Time.deltaTime);
    }

    private void HandleRotation()
    {
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
}

