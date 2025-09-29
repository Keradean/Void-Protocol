/*
====================================================================
InputManager
====================================================================
Project: Space Colony Game
Course: PIP
Script-Developer: Dennis De Col 
*
WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
Diese detaillierte Authorship-Dokumentation ist für die
akademische Bewertung erforderlich und darf nicht entfernt werden!
*
Dieses Script wurde in einer voherigen Abgabe verwendet. 
The Last Refuge / 3D Interactive
====================================================================
*/
using UnityEngine;
using UnityEngine.InputSystem;

// This class is responsible for managing all player inputs
// That means: Keyboard, Mouse, Controller, etc.
public class InputManager : MonoBehaviour
{
    // ==================================================
    // VARIABLE DECLARATION
    // ==================================================

    // [Header] creates a heading in the Unity Inspector to organize variables better
    [Header("Input Actions")]
    // SerializeField makes the variable visible in Unity Inspector even though it's private
    [SerializeField] private InputActionAsset playerControls; // Stores all player controls
    [SerializeField] private string actionMapName = "Player"; // Name of the Action Map for player actions

    [SerializeField] private InputActionAsset UIControls; // Stores all UI controls (e.g. pause menu)
    [SerializeField] private string actionMapNameUI = "UI"; // Name of the Action Map for UI actions

    [Header("Config")]
    // These strings are the names of individual actions as defined in Unity's Input System
    [SerializeField] private string movement = "Movement"; // Name for movement (WASD or Joystick)
    [SerializeField] private string rotation = "Rotation"; // Name for camera rotation (Mouse or right stick)
    [SerializeField] private string jump = "Jump"; // Name for jumping (Spacebar or A button)
    [SerializeField] private string sprint = "Sprint"; // Name for sprinting (Shift or B button)
    [SerializeField] private string interact = "Interact"; // Name for interaction (E key or X button)
    [SerializeField] private string shoot = "Attack"; // Name for shooting (Left mouse button or Trigger)
    [SerializeField] private string reload = "Reloading"; // Name for reloading (R key)
    [SerializeField] private string next = "Next"; // Name for "Next" (e.g. next weapon)
    [SerializeField] private string previous = "Previous"; // Name for "Previous" (e.g. previous weapon)

    [Header("UI")]
    [SerializeField] private string paused = "Paused"; // Name for pause function (ESC key)

    // These variables store the actual Input Actions (the real input commands)
    private InputAction movementAction;
    private InputAction rotationAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction shootAction;
    private InputAction reloadAction;
    private InputAction nextAction;
    private InputAction previousAction;

    private InputAction pausedAction;

    // These two properties return whether the shoot button was pressed or is being pressed
    // => means "returns" (this is a shortened syntax)
    public bool ShootWasPressedThisFrame => shootAction.WasPressedThisFrame(); // Was the button pressed this frame?
    public bool ShootIsPressed => shootAction.IsPressed(); // Is the button currently pressed?

    // These properties store the current input values
    // { get; private set; } means: other scripts can read the value, but only this script can change it
    public Vector2 MovementInput { get; private set; } // Movement input (x and y values, e.g. WASD)
    public Vector2 RotationInput { get; private set; } // Rotation input (mouse movement or right stick)
    public bool JumpTriggered { get; private set; } // Is jump button pressed?
    public bool SprintTriggered { get; private set; } // Is sprint button pressed?
    public bool InteractTriggered { get; private set; } // Is interact button pressed?
    public bool ShootTriggered { get; private set; } // Is shoot button pressed?
    public bool ReloadTriggered { get; private set; } // Is reload button pressed?
    public bool NextTriggered { get; private set; } // Is "next" button pressed?
    public bool PreviousTriggered { get; private set; } // Is "previous" button pressed?
    public bool PausedTriggered { get; private set; } // Is pause button pressed?

    // ==================================================
    // AWAKE METHOD
    // ==================================================
    // Awake is automatically called by Unity before the game starts
    // Here we initialize (prepare) all Input Actions
    private void Awake()
    {
        // Find the Action Map named "Player" in playerControls
        InputActionMap mapReference = playerControls.FindActionMap(actionMapName);

        // Now we search for all individual actions in this map and store them
        movementAction = mapReference.FindAction(movement); // Find "Movement" action
        rotationAction = mapReference.FindAction(rotation); // Find "Rotation" action
        jumpAction = mapReference.FindAction(jump); // Find "Jump" action
        sprintAction = mapReference.FindAction(sprint); // Find "Sprint" action
        interactAction = mapReference.FindAction(interact); // Find "Interact" action
        shootAction = mapReference.FindAction(shoot); // Find "Attack" action
        reloadAction = mapReference.FindAction(reload); // Find "Reloading" action
        nextAction = mapReference.FindAction(next); // Find "Next" action
        previousAction = mapReference.FindAction(previous); // Find "Previous" action

        // We do the same for the UI Action Map
        InputActionMap mapReferenceUI = UIControls.FindActionMap(actionMapNameUI);
        pausedAction = mapReferenceUI.FindAction(paused); // Find "Paused" action

        // Now we call the method that sets up all callbacks
        SetupCallbacks();
    }

    // ==================================================
    // SETUP CALLBACKS METHOD
    // ==================================================
    // This method connects all Input Actions with functions
    // A "Callback" is a function that is automatically called when something happens
    private void SetupCallbacks()
    {
        // Movement
        // "performed" = is called when the action is executed
        // "ctx" is short for "context" and contains information about the input
        movementAction.performed += ctx => MovementInput = ctx.ReadValue<Vector2>(); // Read the Vector2 value (x and y)
        // "canceled" = is called when the button is released
        movementAction.canceled += ctx => MovementInput = Vector2.zero; // Set movement to 0

        // Rotation (Camera rotation)
        rotationAction.performed += ctx => RotationInput = ctx.ReadValue<Vector2>(); // Read rotation value
        rotationAction.canceled += ctx => RotationInput = Vector2.zero; // Set rotation to 0

        // Jump
        jumpAction.performed += ctx => JumpTriggered = true; // When button pressed: set to true
        jumpAction.canceled += ctx => JumpTriggered = false; // When button released: set to false

        // Sprint
        sprintAction.performed += ctx => SprintTriggered = true; // Sprint activated
        sprintAction.canceled += ctx => SprintTriggered = false; // Sprint deactivated

        // Interact
        interactAction.performed += ctx => InteractTriggered = true; // Interaction activated
        interactAction.canceled += ctx => InteractTriggered = false; // Interaction deactivated

        // Shoot
        shootAction.performed += ctx => ShootTriggered = true; // Shooting activated
        shootAction.canceled += ctx => ShootTriggered = false; // Shooting deactivated

        // Reload
        reloadAction.performed += ctx => ReloadTriggered = true; // Reloading activated
        reloadAction.canceled += ctx => ReloadTriggered = false; // Reloading deactivated

        // Next
        nextAction.performed += ctx => NextTriggered = true; // "Next" activated
        nextAction.canceled += ctx => NextTriggered = false; // "Next" deactivated

        // Previous
        previousAction.performed += ctx => PreviousTriggered = true; // "Previous" activated
        previousAction.canceled += ctx => PreviousTriggered = false; // "Previous" deactivated

        // Paused
        pausedAction.performed += ctx => PausedTriggered = true; // Pause activated
        pausedAction.canceled += ctx => PausedTriggered = false; // Pause deactivated
    }

    // ==================================================
    // ONENABLE METHOD
    // ==================================================
    // OnEnable is automatically called when the GameObject is activated
    // Here we enable the Input Actions
    private void OnEnable()
    {
        // Enable the Player Action Map (so the inputs work)
        playerControls.FindActionMap(actionMapName).Enable();

        // Enable the UI Action Map
        UIControls.FindActionMap(actionMapNameUI).Enable();
    }

    // ==================================================
    // ONDISABLE METHOD
    // ==================================================
    // OnDisable is automatically called when the GameObject is deactivated
    // Here we disable the Input Actions
    private void OnDisable()
    {
        // Disable the Player Action Map (inputs won't work anymore)
        playerControls.FindActionMap(actionMapName).Disable();

        // Disable the UI Action Map
        UIControls.FindActionMap(actionMapNameUI).Disable();
    }
}
