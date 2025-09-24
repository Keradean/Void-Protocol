using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset playerControls;
    [SerializeField] private string actionMapName = "Player";

    [Header("Config")]
    [SerializeField] private string movement = "Movement";
    [SerializeField] private string rotation = "Rotation";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string sprint = "Sprint";
    [SerializeField] private string interact = "Interact";
    [SerializeField] private string shoot = "Attack";
    [SerializeField] private string reload = "Reloading";
    [SerializeField] private string next = "Next";
    [SerializeField] private string previous = "Previous";
    [SerializeField] private string paused = "Paused";

    private InputAction movementAction;
    private InputAction rotationAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction shootAction;
    private InputAction reloadAction;
    private InputAction nextAction;
    private InputAction previousAction;

    public bool ShootWasPressedThisFrame => shootAction.WasPressedThisFrame();
    public bool ShootIsPressed => shootAction.IsPressed();

    public Vector2 MovementInput { get; private set; }
    public Vector2 RotationInput { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool SprintTriggered { get; private set; }
    public bool InteractTriggered { get; private set; }  
    public bool ShootTriggered { get; private set; } 
    public bool ReloadTriggered { get; private set; }
    public bool NextTriggered { get; private set; }
    public bool PreviousTriggered { get; private set; }

    private void Awake()
    {
        InputActionMap mapReference = playerControls.FindActionMap(actionMapName);

        movementAction = mapReference.FindAction(movement);
        rotationAction = mapReference.FindAction(rotation);
        jumpAction = mapReference.FindAction(jump);
        sprintAction = mapReference.FindAction(sprint);
        interactAction = mapReference.FindAction(interact);
        shootAction = mapReference.FindAction(shoot);
        reloadAction = mapReference.FindAction(reload);
        nextAction = mapReference.FindAction(next);
        previousAction = mapReference.FindAction(previous);

        SetupCallbacks();
    }

    private void SetupCallbacks()
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

        reloadAction.performed += ctx => ReloadTriggered = true;
        reloadAction.canceled += ctx => ReloadTriggered = false;
       
        nextAction.performed += ctx => NextTriggered = true;
        nextAction.canceled += ctx => NextTriggered = false;

        previousAction.performed += ctx => PreviousTriggered = true;
        previousAction.canceled += ctx => PreviousTriggered = false;
    }

    private void OnEnable()
    {
        playerControls.FindActionMap(actionMapName).Enable();
    }

    private void OnDisable()
    {
        playerControls.FindActionMap(actionMapName).Disable();
    }
}

