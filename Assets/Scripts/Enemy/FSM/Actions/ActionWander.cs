using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionWander : FSMAction
{
    [Header("Movement Config")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float wanderTime = 5f;
    [SerializeField] private Vector3 moveRange = new Vector3(10f, 0f, 10f);
    [SerializeField] private bool useGravity = false;
    [SerializeField] private bool stickToGround = true;
    [SerializeField] private float arrivalDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer = 1;

    private Vector3 movePosition;
    private float timer;
    private Vector3 startPosition;
    private CharacterController characterController;
    private Rigidbody rb; 
    private Vector3 velocity;

    private void Start()
    {
        startPosition = transform.position;
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        GetNewDestination();
        timer = wanderTime;
    }

    public override void Act()
    {
        timer -= Time.deltaTime;

        // Calculate movement
        Vector3 moveDirection = (movePosition - transform.position).normalized;
        Vector3 horizontalMovement = moveDirection * speed * Time.deltaTime;

        // Handle different movement types
        if (characterController != null)
        {
            MoveWithCharacterController(horizontalMovement);
        }
        else if (rb != null)
        {
            MoveWithRigidbody(horizontalMovement);
        }
        else
        {
            MoveWithTransform(horizontalMovement);
        }

        // Check if we've reached the destination
        float distanceToTarget = Vector3.Distance(transform.position, movePosition);
        if (distanceToTarget <= arrivalDistance || timer <= 0f)
        {
            GetNewDestination();
            timer = wanderTime;
        }
    }

    private void MoveWithRigidbody(Vector3 horizontalMovement)
    {
        if (stickToGround)
        {
            // Keep Y velocity controlled, only move horizontally
            Vector3 currentVelocity = rb.linearVelocity;
            Vector3 targetVelocity = horizontalMovement / Time.deltaTime;
            targetVelocity.y = currentVelocity.y; 
            rb.linearVelocity = targetVelocity;
        }
        else
        {
            // Full 3D rigidbody movement
            Vector3 targetVelocity = horizontalMovement / Time.deltaTime;
            if (!useGravity)
            {
                // Override Y velocity if gravity is disabled
                targetVelocity.y = rb.linearVelocity.y;
            }
            rb.linearVelocity = targetVelocity;
        }
    }

    private void MoveWithCharacterController(Vector3 horizontalMovement)
    {
        // Apply gravity if enabled
        if (useGravity)
        {
            if (characterController.isGrounded)
            {
                velocity.y = -0.5f; // Small downward force to stay grounded
            }
            else
            {
                velocity.y += Physics.gravity.y * Time.deltaTime;
            }
            horizontalMovement.y = velocity.y * Time.deltaTime;
        }

        characterController.Move(horizontalMovement);
    }

    private void MoveWithTransform(Vector3 horizontalMovement)
    {
        if (stickToGround)
        {
            // Keep Y position fixed by raycasting to ground
            Vector3 targetPos = transform.position + horizontalMovement;
            if (Physics.Raycast(targetPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                targetPos.y = hit.point.y + 0.1f; // Kleine H�he �ber Boden
            }
            transform.position = targetPos;
        }
        else
        {
            // Full 3D movement
            transform.Translate(horizontalMovement, Space.World);

            // Apply simple gravity if enabled (nur ohne Rigidbody)
            if (useGravity && rb == null) // Korrekte Variable verwendet
            {
                velocity.y += Physics.gravity.y * Time.deltaTime;
                transform.Translate(Vector3.up * velocity.y * Time.deltaTime, Space.World);

                // Simple ground check
                if (Physics.Raycast(transform.position, Vector3.down, 1f, groundLayer))
                {
                    velocity.y = 0f;
                }
            }
        }
    }

    private void GetNewDestination()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-moveRange.x, moveRange.x),
            Random.Range(-moveRange.y, moveRange.y),
            Random.Range(-moveRange.z, moveRange.z)
        );

        Vector3 newPosition = startPosition + randomOffset;

        if (stickToGround)
        {
            newPosition = EnsurePositionOnGround(newPosition);
        }

        movePosition = newPosition;
    }

    private Vector3 EnsurePositionOnGround(Vector3 position)
    {
        // Raycast down to find ground
        if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            position.y = hit.point.y + 0.1f; // Kleine H�he �ber Boden
        }
        else
        {
            // Fallback to start position Y if no ground found
            position.y = startPosition.y;
        }
        return position;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;

        if (moveRange != Vector3.zero)
        {
            // Draw patrol area as wireframe cube
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(center, moveRange * 2f);

            // Draw 3D corner spheres for better 3D visualization
            Gizmos.color = Color.cyan;
            Vector3 size = moveRange;

            // All 8 corners of the 3D box
            Vector3[] corners = {
                center + new Vector3(size.x, size.y, size.z),
                center + new Vector3(-size.x, size.y, size.z),
                center + new Vector3(size.x, -size.y, size.z),
                center + new Vector3(-size.x, -size.y, size.z),
                center + new Vector3(size.x, size.y, -size.z),
                center + new Vector3(-size.x, size.y, -size.z),
                center + new Vector3(size.x, -size.y, -size.z),
                center + new Vector3(-size.x, -size.y, -size.z)
            };

            foreach (Vector3 corner in corners)
            {
                Gizmos.DrawWireSphere(corner, 0.2f);
            }

            if (Application.isPlaying)
            {
                // Draw current target
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(movePosition, 0.5f);
                Gizmos.DrawLine(transform.position, movePosition);

                // Draw start position
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(startPosition, 0.3f);

                // Draw movement path
                Vector3 direction = (movePosition - transform.position).normalized;
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, direction * Vector3.Distance(transform.position, movePosition));

                // Draw Y-axis movement indicator
                if (Mathf.Abs(movePosition.y - transform.position.y) > 0.1f)
                {
                    Gizmos.color = movePosition.y > transform.position.y ? Color.green : Color.red;
                    Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, movePosition.y, transform.position.z));
                }
            }
        }

        // Always draw current position
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
}