using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    /// <summary>
    /// Cheats 
    /// </summary>
    //[Header("")]
    //[Tooltip("")]
    //[SerializeField]

    [Header("Config Movement Parameters")]
    [Tooltip("Configure your Movement & Sprint Speeds")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintSpeed;

    [Header("Config Jump Parameters")]
    [Tooltip("Configure your  Jump & Gravity Variables")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float gravity;

    [Header("Config Look Parameters")]
    [Tooltip("Configure your  Look Parameters Variables")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float upDownLookRange;

    [Header("Referenz")]
    [Tooltip("")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InputManager inputManager;

    [Header("Config Stamina Parameter")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private float staminaDrainPerSecond;
    [SerializeField] private float staminaRegenPerSecond;

    [Header("Config Sprint Parameter")]
    [SerializeField] private float sprintCooldown;


    private float currentSprintCooldown = 0f;
    private Vector3 currentMovement;
    private float verticalRotation;

    private bool CanSprint => inputManager.SprintTriggered && stats.Stamina > 0f && currentSprintCooldown <= 0f;
    private float currentSpeed => moveSpeed * (CanSprint ? sprintSpeed : 1);

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

    }

    private Vector3 CalculateMove()
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
        Vector3 calculateMove = CalculateMove();
        currentMovement.x = calculateMove.x * currentSpeed;
        currentMovement.z = calculateMove.z * currentSpeed;

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
            stats.Stamina -= staminaDrainPerSecond * Time.deltaTime;
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
                stats.Stamina += staminaRegenPerSecond * Time.deltaTime;
                if (stats.Stamina > stats.MaxStamina) stats.Stamina = stats.MaxStamina;
            }
        }

    }
}
