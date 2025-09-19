/*
====================================================================
* TileNavigationUI.cs - Clean Compilation Safe Version
====================================================================
* Project: Space Colony Game
* Course: PIP
* Script-Developer: Julian
* Date: 18.09.2025
* Version: Clean - Academic Attribution
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUTHORSHIP CLASSIFICATION:
* [HUMAN-AUTHORED] - Navigation logic, UI direction calculation, distance display
* [AI-ASSISTED] - TileManager integration, performance optimization, syntax correction
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
    [SerializeField] private float updateInterval = 0.2f;
    [SerializeField] private bool showDistance = true;
    [SerializeField] private bool showDirection = true;
    [SerializeField] private bool enableDebugDisplay = false;

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
            Vector3 playerPos = GetPlayerPosition();
            Debug.Log($"Player Position: {playerPos}");
            Vector3 targetPos = tileManager.GetNearestKeyTileWorldPosition(playerPos);
            Debug.Log($"Target Position: {targetPos}");
        }
    }

    private void InitializeUI()
    {
        // Initialize UI elements if not assigned
        if (directionText == null)
        {
            Transform foundText = transform.Find("DirectionText");
            if (foundText != null)
                directionText = foundText.GetComponent<TextMeshProUGUI>();
        }

        if (distanceText == null)
        {
            Transform foundText = transform.Find("DistanceText");
            if (foundText != null)
                distanceText = foundText.GetComponent<TextMeshProUGUI>();
        }

        if (keyTileCountText == null)
        {
            Transform foundText = transform.Find("KeyTileCountText");
            if (foundText != null)
                keyTileCountText = foundText.GetComponent<TextMeshProUGUI>();
        }

        if (directionArrow == null)
        {
            Transform foundArrow = transform.Find("DirectionArrow");
            if (foundArrow != null)
                directionArrow = foundArrow.GetComponent<RectTransform>();
        }
    }

    private void FindTileManager()
    {
        if (tileManager == null)
        {
            tileManager = FindFirstObjectByType<TileManager>();
            if (tileManager == null)
            {
                Debug.LogWarning("TileManager not found. Assign manually in Inspector.");
            }
        }

        // Subscribe to events if TileManager found
        if (tileManager != null && tileManager.OnKeyTilesUpdated != null)
        {
            tileManager.OnKeyTilesUpdated += UpdateKeyTileCount;
        }
    }

    private void UpdateNavigationUI()
    {
        if (tileManager == null) return;

        Vector3 playerPosition = GetPlayerPosition();
        if (playerPosition == Vector3.zero) return;

        try
        {
            // Get nearest key tile from TileManager
            Vector3 nearestKeyTileWorldPos = tileManager.GetNearestKeyTileWorldPosition(playerPosition);
            Vector2Int nearestKeyTileGrid = WorldToGridPosition(nearestKeyTileWorldPos);

            // Only update direction if nearest tile changed
            if (nearestKeyTileGrid != cachedNearestKeyTile)
            {
                cachedNearestKeyTile = nearestKeyTileGrid;
                UpdateDirectionDisplay(playerPosition, nearestKeyTileWorldPos);
            }

            // Always update distance as player moves
            UpdateDistanceDisplay(playerPosition, nearestKeyTileWorldPos);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Navigation UI update error: {e.Message}");
        }
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
            float targetRotation = -angle;
            directionArrow.rotation = Quaternion.Lerp(
                directionArrow.rotation,
                Quaternion.Euler(0, 0, targetRotation),
                Time.deltaTime * 3f
            );
        }
    }

    private void UpdateDistanceDisplay(Vector3 playerPos, Vector3 targetPos)
    {
        if (!showDistance || distanceText == null) return;

        float distance = Vector3.Distance(playerPos, targetPos);
        cachedDistance = distance;

        // Format distance display with color coding
        if (distance < 5f)
        {
            distanceText.text = $"{distance:F1}m";
            distanceText.color = Color.green;
        }
        else if (distance < 15f)
        {
            distanceText.text = $"{distance:F1}m";
            distanceText.color = Color.yellow;
        }
        else
        {
            distanceText.text = $"{distance:F0}m";
            distanceText.color = Color.red;
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

    // Public interface methods
    public float GetDistanceToNearestKeyTile()
    {
        return cachedDistance;
    }

    public Vector2Int GetNearestKeyTilePosition()
    {
        return cachedNearestKeyTile;
    }

    // Debug display
    private void OnGUI()
    {
        if (!enableDebugDisplay || !Application.isPlaying || tileManager == null) return;

        // Debug information in screen corner
        GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 180, 120));
        GUILayout.Box("Navigation Debug");
        GUILayout.Label($"Distance: {cachedDistance:F1}m");
        GUILayout.Label($"Grid Pos: {cachedNearestKeyTile}");
        GUILayout.Label($"Key Tiles: {tileManager.GetKeyTilePositions().Count}");
        GUILayout.Label($"Update Rate: {1f / updateInterval:F1} Hz");
        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (tileManager != null && tileManager.OnKeyTilesUpdated != null)
        {
            tileManager.OnKeyTilesUpdated -= UpdateKeyTileCount;
        }
    }
}