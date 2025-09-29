/*
====================================================================
TileManager.cs - Phase 1: Multi-Tile System Foundation
====================================================================
Project: Space Colony Game
Course: PIP
Script-Developer: Marcel Regele 3D - Artist
Date: 29.09.2025
Version: Phase 1 - Academic Attribution
*
WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
Diese detaillierte Authorship-Dokumentation ist für die
akademische Bewertung erforderlich und darf nicht entfernt werden!
*
AUTHORSHIP CLASSIFICATION:
[AI-ASSISTED] - SandstormWorldFollow
====================================================================
*/

using UnityEngine;

public class SandstormWorldFollow : MonoBehaviour
{
    [Tooltip("Camera to follow. If null, Camera.main will be used.")]
    public Transform playerCamera;

    [Tooltip("Horizontal distance from the player (XZ plane).")]
    public float distance = 80f;

    [Tooltip("Vertical offset (always global Y).")]
    public float heightOffset = 10f;

    [Tooltip("Magnitude of the Force over Lifetime (world space).")]
    public float forceStrength = 5f;

    private Vector3 horizontalOffset; // fixed at Start
    private ParticleSystem ps;
    private ParticleSystem.ForceOverLifetimeModule forceOverLifetime;

    void Start()
    {
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        if (playerCamera == null) return;

        ps = GetComponent<ParticleSystem>();
        if (ps != null)
            forceOverLifetime = ps.forceOverLifetime;

        // Random horizontal angle (full 360°)
        float angle = Random.Range(0f, 360f);
        Vector3 dirXZ = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

        // Horizontal offset only (Y = 0)
        horizontalOffset = dirXZ * distance;

        // Position on same Y level as camera (rotation calculated here)
        Vector3 basePos = playerCamera.position + horizontalOffset;

        // Rotate emitter once so its forward faces the player on the XZ plane
        Vector3 dirToPlayerXZ = (playerCamera.position - basePos);
        dirToPlayerXZ.y = 0f; // ignore vertical difference so no tilt
        transform.rotation = Quaternion.LookRotation(dirToPlayerXZ.normalized, Vector3.up);

        // Force over Lifetime: towards player, but flat (XZ only)
        if (ps != null)
        {
            forceOverLifetime.space = ParticleSystemSimulationSpace.World;
            forceOverLifetime.enabled = true;

            Vector3 forceVec = dirToPlayerXZ.normalized * forceStrength;
            forceOverLifetime.x = new ParticleSystem.MinMaxCurve(forceVec.x);
            forceOverLifetime.y = new ParticleSystem.MinMaxCurve(forceVec.y); // will be 0
            forceOverLifetime.z = new ParticleSystem.MinMaxCurve(forceVec.z);
        }

        // Finally: move emitter upward after rotation & force setup
        transform.position = basePos + new Vector3(0f, heightOffset, 0f);
    }

    void LateUpdate()
    {
        if (playerCamera == null) return;

        // Keep emitter following player with same offset + Y lift
        Vector3 basePos = playerCamera.position + horizontalOffset;
        transform.position = basePos + new Vector3(0f, heightOffset, 0f);
        // Note: rotation and force stay unchanged
    }
}
