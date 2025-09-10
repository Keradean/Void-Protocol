using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class InputManager : MonoBehaviour
{
    /// <summary>
    /// Cheats 
    /// </summary>
    //[Header("")]
    //[Tooltip("")]
    //[SerializeField]

    /// <summary>
    /// Reference to the Input Action Asset for player controls.
    /// </summary>
    [Header("Input Actions")]
    [Tooltip("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;

    [Tooltip("Action Map Name Reference")]
    [SerializeField] private string actionMapName = "Player";

    [Header("Confiq")]
    [SerializeField] private string movement = "Movement";
    [SerializeField] private string rotation = "Rotation";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string sprint = "Sprint";
    [SerializeField] private string interact = "Interact";
    [SerializeField] private string shoot = "Attack";

    private InputAction movementAction;
    private InputAction rotationAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction shootAction;

    public Vector2 MovementInput { get; private set; }
    public Vector2 RotationInput { get; private set; }
    public bool JumpTriggered { get; internal set; }
    public bool SprintTriggered { get; private set; }
    public bool InteractTriggered { get; private set; }
    public bool ShootTriggered { get; private set; }

    private void Awake()
    {
        InputActionMap mapReference = playerControls.FindActionMap(actionMapName);

        movementAction = mapReference.FindAction(movement);
        rotationAction = mapReference.FindAction(rotation);
        jumpAction = mapReference.FindAction(jump);
        sprintAction = mapReference.FindAction(sprint);
        interactAction = mapReference.FindAction(interact);
        shootAction = mapReference.FindAction(shoot);

        ActionValues();
    }

    private void ActionValues()
    {
        movementAction.performed += ctx => MovementInput = ctx.ReadValue<Vector2>();
        movementAction.canceled += ctx => MovementInput = Vector2.zero;

        rotationAction.performed += ctx => RotationInput = ctx.ReadValue<Vector2>();
        rotationAction.canceled += ctx => RotationInput = Vector2.zero;

        jumpAction.performed += ctx => JumpTriggered = true;
        jumpAction.canceled += ctx => JumpTriggered = false;

        sprintAction.performed += ctx => SprintTriggered = true;
        sprintAction.canceled += ctx => SprintTriggered = false;

        interactAction.performed += ctx => InteractTriggered = true;
        interactAction.canceled += ctx => InteractTriggered = false;
        
        shootAction.performed += ctx => ShootTriggered = true;
        shootAction.canceled += ctx => ShootTriggered = false;
    }

    private void OnEnable()
    {
        playerControls.FindActionMap(actionMapName).Enable();

    }

    private void OnDisable()
    {
        playerControls.FindActionMap(actionMapName).Disable();

    }

    public void ResetInteract()
    {
        InteractTriggered = false;
    }
    
    public void ResetShoot()
    {
        ShootTriggered = false;
    }
}

