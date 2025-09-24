/*
====================================================================
* TileSystemSetup.cs - Editor Tool für Tile System Automatisierung
====================================================================
* Project: Space Colony Game
* Course: PIP
* Script-Developer: Julian
* Date: 18.09.2025
* Version: v2.0 - Academic Attribution
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUTHORSHIP CLASSIFICATION:
* [HUMAN-AUTHORED] - Setup logic, GameObject creation, component assignment
* [AI-ASSISTED] - Editor integration, null safety patterns, Unity best practices
* 
* FUNCTIONALITY: Automatisierte Erstellung des Tile System mit allen
* erforderlichen Komponenten ohne Domain Reload Konflikte
====================================================================
*/

using UnityEngine;
using UnityEditor;

public class TileSystemSetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject keyTilePrefab;
    [SerializeField] private GameObject navigationUIPrefab;

#if UNITY_EDITOR
    [ContextMenu("Setup Complete Tile System")]
    private void SetupTileSystem()
    {
        CreateTileSystemStructure();
        Debug.Log("Tile System Setup Complete");
    }

    private void CreateTileSystemStructure()
    {
        // Step 1: Create Main Container
        GameObject tileSystemRoot = CreateGameObjectIfNotExists("TileSystem", Vector3.zero);

        // Step 2: Create Manager Container
        GameObject managersContainer = CreateGameObjectIfNotExists("Managers", Vector3.zero, tileSystemRoot.transform);

        // Step 3: Setup TileManager
        GameObject tileManagerGO = CreateGameObjectIfNotExists("TileManager", Vector3.zero, managersContainer.transform);
        SetupTileManagerComponent(tileManagerGO);

        // Step 4: Setup Navigation UI
        SetupNavigationUI();

        // Step 5: Create Tile Container
        CreateGameObjectIfNotExists("Generated_Tiles", Vector3.zero, tileSystemRoot.transform);
    }

    private GameObject CreateGameObjectIfNotExists(string name, Vector3 position, Transform parent = null)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null)
        {
            Debug.Log($"{name} already exists - using existing GameObject");
            return existing;
        }

        GameObject newGO = new GameObject(name);
        newGO.transform.position = position;

        if (parent != null)
        {
            newGO.transform.SetParent(parent);
        }

        return newGO;
    }

    private void SetupTileManagerComponent(GameObject tileManagerGO)
    {
        // Check if TileManager component exists
        TileManager existingManager = tileManagerGO.GetComponent<TileManager>();
        if (existingManager != null)
        {
            Debug.Log("TileManager component already exists");
            return;
        }

        // Add TileManager component
        TileManager tileManager = tileManagerGO.AddComponent<TileManager>();

        // Direct component assignment (no SerializedObject)
        if (tilePrefab != null && keyTilePrefab != null)
        {
            Debug.Log("TileManager component added - assign prefabs manually in Inspector");
        }
        else
        {
            Debug.LogWarning("Tile prefabs not assigned - please assign in Inspector");
        }
    }

    private void SetupNavigationUI()
    {
        // Find existing Canvas
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas == null)
        {
            Debug.LogWarning("No Canvas found - create Canvas first");
            return;
        }

        // Check if Navigation UI already exists
        TileNavigationUI existingNavUI = FindFirstObjectByType<TileNavigationUI>();
        if (existingNavUI != null)
        {
            Debug.Log("TileNavigationUI already exists");
            return;
        }

        // Create Navigation UI GameObject
        GameObject navUIGO = CreateGameObjectIfNotExists("NavigationPanel", Vector3.zero, existingCanvas.transform);

        // Add RectTransform positioning
        RectTransform rectTransform = navUIGO.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = navUIGO.AddComponent<RectTransform>();
        }

        // Set UI positioning (top-left)
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(20, -20);

        // Add TileNavigationUI component
        TileNavigationUI navUI = navUIGO.AddComponent<TileNavigationUI>();
        Debug.Log("NavigationUI created - assign UI elements manually in Inspector");
    }

    [ContextMenu("Validate System Setup")]
    private void ValidateSystemSetup()
    {
        bool isValid = true;

        // Check TileManager
        TileManager tileManager = FindFirstObjectByType<TileManager>();
        if (tileManager == null)
        {
            Debug.LogError("TileManager not found");
            isValid = false;
        }

        // Check NavigationUI
        TileNavigationUI navUI = FindFirstObjectByType<TileNavigationUI>();
        if (navUI == null)
        {
            Debug.LogError("TileNavigationUI not found");
            isValid = false;
        }

        // Check Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found");
            isValid = false;
        }

        // Check PlayerController
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogError("PlayerController not found");
            isValid = false;
        }

        if (isValid)
        {
            Debug.Log("Tile System Setup Validation PASSED");
        }
        else
        {
            Debug.LogError("Tile System Setup Validation FAILED");
        }
    }
#endif
}