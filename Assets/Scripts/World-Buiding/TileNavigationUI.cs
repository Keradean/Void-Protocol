/*
====================================================================
* TILENAVIGATIONUI - Key Tile Navigation Display
====================================================================
* Project: Space Colony Game
* Course: PIP
* Script-Developer: Julian
* Date: 17.09.2025
* Version: Final Clean v1.0
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die akademische
* Bewertung erforderlich und darf nicht entfernt werden!
*
* AUTHORSHIP CLASSIFICATION:
* [HUMAN-AUTHORED] - UI design decisions and navigation display logic
* [AI-ASSISTED] - Component integration and update optimization
* [AI-GENERATED] - Direction calculation and compass algorithms
====================================================================
*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TileNavigationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI directionText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI keyTileCountText;
    [SerializeField] private Image directionArrow;
    [SerializeField] private GameObject navigationPanel;

    [Header("Display Settings")]
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private bool enableArrowRotation = true;
    [SerializeField] private float arrowSmoothTime = 0.3f;

    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color nearColor = Color.green;
    [SerializeField] private float nearDistance = 5f;

    // Cached references - Julian's pattern
    private TileManager tileManager;
    private PlayerController player;

    // Current navigation state
    private KeyTileInfo currentTarget;
    private float lastUpdateTime;
    private float currentArrowRotation;
    private float targetArrowRotation;

    private void Start()
    {
        InitializeReferences();
        SetupEventListeners();
        UpdateNavigationVisibility(false); // Start hidden
    }

    private void Update()
    {
        if (ShouldUpdateNavigation())
        {
            UpdateNavigationDisplay();
            lastUpdateTime = Time.time;
        }

        if (enableArrowRotation && directionArrow != null)
        {
            UpdateArrowRotation();
        }
    }

    private void OnDestroy()
    {
        CleanupEventListeners();
    }

    #region Initialization

    private void InitializeReferences()
    {
        tileManager = FindFirstObjectByType<TileManager>();
        player = FindFirstObjectByType<PlayerController>();

        if (tileManager == null)
        {
            Debug.LogError("TileNavigationUI: TileManager nicht gefunden!");
            enabled = false;
            return;
        }

        if (player == null)
        {
            Debug.LogError("TileNavigationUI: PlayerController nicht gefunden!");
            enabled = false;
            return;
        }

        Debug.Log("TileNavigationUI: Initialized successfully");
    }

    private void SetupEventListeners()
    {
        if (tileManager != null)
        {
            tileManager.OnKeyTilesUpdated += OnKeyTilesUpdated;
            tileManager.OnKeyTileReached += OnKeyTileReached;
        }
    }

    private void CleanupEventListeners()
    {
        if (tileManager != null)
        {
            tileManager.OnKeyTilesUpdated -= OnKeyTilesUpdated;
            tileManager.OnKeyTileReached -= OnKeyTileReached;
        }
    }

    #endregion

    #region Event Handlers

    private void OnKeyTilesUpdated(System.Collections.Generic.List<KeyTileInfo> keyTiles)
    {
        int unvisitedCount = CountUnvisitedTiles(keyTiles);

        UpdateKeyTileCountDisplay(unvisitedCount);
        UpdateNavigationVisibility(unvisitedCount > 0);

        // Update target if current one is invalid
        if (currentTarget == null || currentTarget.isVisited)
        {
            UpdateCurrentTarget();
        }
    }

    private void OnKeyTileReached(KeyTileInfo reachedTile)
    {
        // Visual feedback for reaching a key tile
        ShowKeyTileReachedFeedback();

        // Update to next target
        UpdateCurrentTarget();
    }

    private int CountUnvisitedTiles(System.Collections.Generic.List<KeyTileInfo> keyTiles)
    {
        int count = 0;
        foreach (KeyTileInfo tile in keyTiles)
        {
            if (!tile.isVisited) count++;
        }
        return count;
    }

    #endregion

    #region Navigation Updates

    private bool ShouldUpdateNavigation()
    {
        return (Time.time - lastUpdateTime) >= updateInterval &&
               currentTarget != null &&
               player != null;
    }

    private void UpdateNavigationDisplay()
    {
        if (currentTarget == null || currentTarget.isVisited)
        {
            UpdateCurrentTarget();
            return;
        }

        Vector3 playerPosition = player.transform.position;
        Vector3 targetPosition = currentTarget.worldPosition;

        float distance = Vector3.Distance(playerPosition, targetPosition);
        Vector3 direction = (targetPosition - playerPosition).normalized;

        UpdateDistanceDisplay(distance);
        UpdateDirectionDisplay(direction, distance);

        if (enableArrowRotation)
        {
            UpdateTargetArrowRotation(direction);
        }
    }

    private void UpdateCurrentTarget()
    {
        if (player == null || tileManager == null) return;

        currentTarget = tileManager.GetNearestUnvisitedKeyTile(player.transform.position);
    }

    #endregion

    #region UI Display Updates

    private void UpdateDistanceDisplay(float distance)
    {
        if (distanceText == null) return;

        string distanceString = distance < 1f ? $"{distance:F1}m" : $"{distance:F0}m";
        distanceText.text = distanceString;
        distanceText.color = GetDistanceColor(distance);
    }

    private void UpdateDirectionDisplay(Vector3 direction, float distance)
    {
        if (directionText == null) return;

        string directionName = GetDirectionName(direction);
        directionText.text = directionName;
        directionText.color = GetDistanceColor(distance);
    }

    private void UpdateKeyTileCountDisplay(int count)
    {
        if (keyTileCountText == null) return;

        keyTileCountText.text = $"Ziele: {count}";
        keyTileCountText.color = count == 0 ? nearColor : normalColor;
    }

    private void UpdateNavigationVisibility(bool shouldShow)
    {
        if (navigationPanel != null)
        {
            navigationPanel.SetActive(shouldShow);
        }
    }

    #endregion

    #region Arrow Rotation

    private void UpdateTargetArrowRotation(Vector3 direction)
    {
        targetArrowRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    }

    private void UpdateArrowRotation()
    {
        if (directionArrow == null) return;

        currentArrowRotation = Mathf.LerpAngle(currentArrowRotation, targetArrowRotation,
            Time.deltaTime / arrowSmoothTime);

        directionArrow.transform.rotation = Quaternion.Euler(0, 0, -currentArrowRotation);

        // Update arrow color based on distance
        if (currentTarget != null && player != null)
        {
            float distance = Vector3.Distance(player.transform.position, currentTarget.worldPosition);
            directionArrow.color = GetDistanceColor(distance);
        }
    }

    #endregion

    #region Utility Methods

    private Color GetDistanceColor(float distance)
    {
        return distance <= nearDistance ? nearColor : normalColor;
    }

    private string GetDirectionName(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // 8-direction compass
        if (angle >= 337.5f || angle < 22.5f) return "Nord";
        if (angle >= 22.5f && angle < 67.5f) return "Nord-Ost";
        if (angle >= 67.5f && angle < 112.5f) return "Ost";
        if (angle >= 112.5f && angle < 157.5f) return "Süd-Ost";
        if (angle >= 157.5f && angle < 202.5f) return "Süd";
        if (angle >= 202.5f && angle < 247.5f) return "Süd-West";
        if (angle >= 247.5f && angle < 292.5f) return "West";
        if (angle >= 292.5f && angle < 337.5f) return "Nord-West";

        return "Nord"; // Default fallback
    }

    private void ShowKeyTileReachedFeedback()
    {
        // Simple visual feedback - could be enhanced with animations
        if (keyTileCountText != null)
        {
            Color originalColor = keyTileCountText.color;
            keyTileCountText.color = nearColor;

            // Reset color after a brief moment
            Invoke(nameof(ResetKeyTileCountColor), 0.5f);
        }
    }

    private void ResetKeyTileCountColor()
    {
        if (keyTileCountText != null)
        {
            keyTileCountText.color = normalColor;
        }
    }

    #endregion

    #region Public API

    public KeyTileInfo GetCurrentTarget()
    {
        return currentTarget;
    }

    public bool HasActiveTarget()
    {
        return currentTarget != null && !currentTarget.isVisited;
    }

    public float GetDistanceToCurrentTarget()
    {
        if (currentTarget == null || player == null) return -1f;
        return Vector3.Distance(player.transform.position, currentTarget.worldPosition);
    }

    #endregion
}