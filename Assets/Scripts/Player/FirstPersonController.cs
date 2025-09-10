using UnityEngine;

public class FirstPersonController : MonoBehaviour
{

    [Header("Referenz")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private WeaponsController weaponsController;
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
    
    [Header("Config Shoot Parameter")]
    [SerializeField] private float shootCooldown;
    [SerializeField] private bool isAutomatic = false;

    [Header("Config Stamina Parameter")]
    [SerializeField] private float staminaDrain;
    [SerializeField] private float staminaRegen;

    private float currentSprintCooldown;
    private Vector3 currentMovement;
    private float verticalRotation;
    private float currentShootCooldown;

    private bool CanSprint => inputManager.SprintTriggered && stats.Stamina > 0f && currentSprintCooldown <= 0f;
    private float CurrentSpeed => moveSpeed * (CanSprint ? sprintSpeedMultiplier : 1);
    private bool CanShoot => currentShootCooldown <= 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        stats.Stamina = stats.MaxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleStamina();
        HandleShooting();
    }
    private Vector3 Move()
    {
        Vector3 inputDirection = new Vector3(inputManager.MovementInput.x, 0f, inputManager.MovementInput.y);
        Vector3 calculateMove = transform.TransformVector(inputDirection);
        return calculateMove.normalized;
    }

    private void Jump()
    {
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
    }

    private void HandleMovement()
    {
        Vector3 calculateMove = Move();
        currentMovement.x = calculateMove.x * CurrentSpeed;
        currentMovement.z = calculateMove.z * CurrentSpeed;

        Jump();
        characterController.Move(currentMovement * Time.deltaTime);
    }

    private void HorizontalRotation(float rotationAmount)
    {
        transform.Rotate(0, rotationAmount, 0);
    }

    private void VerticalRotation(float rotationAmount)
    {
        verticalRotation = Mathf.Clamp(verticalRotation - rotationAmount, -upDownLookRange, upDownLookRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleRotation()
    {
        float mouseXRotation = inputManager.RotationInput.x * mouseSensitivity;
        float mouseYRotation = inputManager.RotationInput.y * mouseSensitivity;

        HorizontalRotation(mouseXRotation);
        VerticalRotation(mouseYRotation);
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
                if (stats.Stamina > stats.MaxStamina) stats.Stamina = stats.MaxStamina;
            }
        }

    }

    private void HandleShooting()
    {
        if(currentShootCooldown > 0f)
            currentShootCooldown -= Time.deltaTime;

        if(inputManager.ShootTriggered && CanShoot)
        {
            weaponsController.Shoot();
            currentShootCooldown = shootCooldown;
            
            if(!isAutomatic) // Einzelschuss
            {
                inputManager.ResetShoot();
            }
         
        }
    }
}
