/*
====================================================================
* TileNavigationUI.cs - Phase 1 Compatibility Update
====================================================================
* Project: Space Colony Game
* Course: PIP
* Script-Developer: Julian
* Date: 18.09.2025
* Version: Phase 1 Compatible - Academic Attribution
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUTHORSHIP CLASSIFICATION:
* [HUMAN-AUTHORED] - Navigation logic, UI direction calculation, distance display
* [AI-ASSISTED] - TileManager integration, performance optimization, compatibility updates
====================================================================
*/

using UnityEngine;
using TMPro;

public class TileNavigationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI directionText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI keyTileCountText;
    [SerializeField] private RectTransform directionArrow;

    [Header("Navigation Settings")]
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private bool showDistance = true;
    [SerializeField] private bool showDirection = true;

    [Header("TileManager Reference")]
    [SerializeField] private TileManager tileManager;

    // Private variables
    private float lastUpdateTime;
    private Vector2Int cachedNearestKeyTile;
    private float cachedDistance;
    private string[] directionNames = { "Nord", "Nord-Ost", "Ost", "Sud-Ost", "Sud", "Sud-West", "West", "Nord-West" };

    void Start()
    {
        InitializeUI();
        FindTileManager();
    }

    void Update()
    {
        if (Time.time - lastUpdateTime > updateInterval)
        {
            UpdateNavigationUI();
            lastUpdateTime = Time.time;
        }
    }

    private void InitializeUI()
    {
        // Initialize UI elements if not assigned
        if (directionText == null)
            directionText = transform.Find("DirectionText")?.GetComponent<TextMeshProUGUI>();

        if (distanceText == null)
            distanceText = transform.Find("DistanceText")?.GetComponent<TextMeshProUGUI>();

        if (keyTileCountText == null)
            keyTileCountText = transform.Find("KeyTileCountText")?.GetComponent<TextMeshProUGUI>();

        if (directionArrow == null)
            directionArrow = transform.Find("DirectionArrow")?.GetComponent<RectTransform>();

        // Subscribe to TileManager events
        if (tileManager != null)
        {
            tileManager.OnKeyTilesUpdated += UpdateKeyTileCount;
        }
    }

    private void FindTileManager()
    {
        if (tileManager == null)
        {
            tileManager = FindFirstObjectByType<TileManager>();
            if (tileManager != null)
            {
                tileManager.OnKeyTilesUpdated += UpdateKeyTileCount;
            }
            else
            {
                Debug.LogWarning("TileManager not found! Assign manually in Inspector.");
            }
        }
    }

    private void UpdateNavigationUI()
    {
        if (tileManager == null) return;

        Vector3 playerPosition = GetPlayerPosition();
        if (playerPosition == Vector3.zero) return;

        // Get nearest key tile from TileManager
        Vector3 nearestKeyTileWorldPos = tileManager.GetNearestKeyTileWorldPosition(playerPosition);
        Vector2Int nearestKeyTileGrid = WorldToGridPosition(nearestKeyTileWorldPos);

        // Only update if nearest tile changed
        if (nearestKeyTileGrid != cachedNearestKeyTile)
        {
            cachedNearestKeyTile = nearestKeyTileGrid;
            UpdateDirectionDisplay(playerPosition, nearestKeyTileWorldPos);
        }

        // Update distance (always, as player moves)
        UpdateDistanceDisplay(playerPosition, nearestKeyTileWorldPos);
    }

    private void UpdateDirectionDisplay(Vector3 playerPos, Vector3 targetPos)
    {
        if (!showDirection || directionText == null) return;

        // Calculate direction vector
        Vector3 directionVector = (targetPos - playerPos).normalized;

        // Convert to compass direction (8 directions)
        float angle = Mathf.Atan2(directionVector.x, directionVector.z) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        // Map angle to 8-direction compass
        int directionIndex = Mathf.RoundToInt(angle / 45f) % 8;
        string directionName = directionNames[directionIndex];

        directionText.text = directionName;

        // Update arrow rotation if present
        if (directionArrow != null)
        {
            float targetRotation = -angle; // Negative for UI coordinate system
            directionArrow.rotation = Quaternion.Lerp(
                directionArrow.rotation,
                Quaternion.Euler(0, 0, targetRotation),
                Time.deltaTime * 5f
            );
        }
    }

    private void UpdateDistanceDisplay(Vector3 playerPos, Vector3 targetPos)
    {
        if (!showDistance || distanceText == null) return;

        float distance = Vector3.Distance(playerPos, targetPos);
        cachedDistance = distance;

        // Format distance display
        if (distance < 10f)
        {
            distanceText.text = $"{distance:F1}m";
            distanceText.color = Color.green; // Close
        }
        else if (distance < 50f)
        {
            distanceText.text = $"{distance:F0}m";
            distanceText.color = Color.yellow; // Medium
        }
        else
        {
            distanceText.text = $"{distance:F0}m";
            distanceText.color = Color.red; // Far
        }
    }

    private void UpdateKeyTileCount()
    {
        if (keyTileCountText == null || tileManager == null) return;

        var keyTilePositions = tileManager.GetKeyTilePositions();
        keyTileCountText.text = $"Key Tiles: {keyTilePositions.Count}";
    }

    private Vector3 GetPlayerPosition()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        return player != null ? player.transform.position : Vector3.zero;
    }

    private Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x),
            Mathf.RoundToInt(worldPos.z)
        );
    }

    // Public interface for external access
    public float GetDistanceToNearestKeyTile()
    {
        return cachedDistance;
    }

    public Vector2Int GetNearestKeyTilePosition()
    {
        return cachedNearestKeyTile;
    }

    // Debug info
    private void OnGUI()
    {
        if (!Application.isPlaying) return;
        if (tileManager == null) return;

        // Debug information in top-right corner
        GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 180, 100));
        GUILayout.Label($"Navigation Debug", GUI.skin.box);
        GUILayout.Label($"Distance: {cachedDistance:F1}m");
        GUILayout.Label($"Grid Pos: {cachedNearestKeyTile}");
        GUILayout.Label($"Key Tiles: {tileManager.GetKeyTilePositions().Count}");
        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (tileManager != null)
        {
            tileManager.OnKeyTilesUpdated -= UpdateKeyTileCount;
        }
    }
}