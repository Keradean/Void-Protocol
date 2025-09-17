/*
====================================================================
* TILESYSTEMSETUP - Unity GraphObject Safe Implementation
====================================================================
* Project: Space Colony Game
* Course: PIP
* Script-Developer: Julian
* Date: 17.09.2025
* Version: Unity Safe v1.2 - GraphObject Domain Reload Fix
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die akademische
* Bewertung erforderlich und darf nicht entfernt werden!
*
* AUTHORSHIP CLASSIFICATION:
* [HUMAN-AUTHORED] - Setup workflow and Unity integration requirements
* [AI-ASSISTED] - Unity domain reload safety and GraphObject conflict resolution
* [AI-GENERATED] - Editor timing workarounds and safe execution patterns
====================================================================
*/

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

[InitializeOnLoad]
public static class TileSystemSetup
{
    private static bool setupRequested = false;
    private static bool isSettingUp = false;

    // Initialize on load - safe from domain reload
    static TileSystemSetup()
    {
        // Register for safe editor update
        EditorApplication.update += SafeEditorUpdate;
    }

    private static void SafeEditorUpdate()
    {
        // Only run setup when Unity is in stable state
        if (setupRequested && !isSettingUp && IsUnityInStableState())
        {
            setupRequested = false;
            isSettingUp = true;

            try
            {
                PerformActualSetup();
            }
            finally
            {
                isSettingUp = false;
            }
        }
    }

    private static bool IsUnityInStableState()
    {
        return !EditorApplication.isCompiling &&
               !EditorApplication.isUpdating &&
               !EditorApplication.isPlayingOrWillChangePlaymode &&
               EditorApplication.timeSinceStartup > 2.0; // Wait 2 seconds after startup
    }

    [MenuItem("Tools/Tile System/Setup Complete System")]
    public static void SetupCompleteSystem()
    {
        if (isSettingUp)
        {
            Debug.LogWarning("Setup already in progress...");
            return;
        }

        if (!IsUnityInStableState())
        {
            Debug.Log("Unity is busy. Setup will start automatically when ready...");
            setupRequested = true;
            return;
        }

        // Direct setup if Unity is stable
        isSettingUp = true;
        try
        {
            PerformActualSetup();
        }
        finally
        {
            isSettingUp = false;
        }
    }

    private static void PerformActualSetup()
    {
        Debug.Log("=== TILE SYSTEM SETUP STARTED ===");

        try
        {
            // Step 1: Create hierarchy without any Unity object searches
            GameObject tileSystemRoot = CreateHierarchyOnly();

            // Step 2: Add components without SerializedObject operations
            SetupComponentsBasic(tileSystemRoot);

            // Step 3: Create UI without advanced operations
            SetupBasicUI();

            // Step 4: Simple validation
            ValidateBasicSetup(tileSystemRoot);

            Debug.Log("SUCCESS: Tile System setup completed");

            // Safe selection and dialog
            if (tileSystemRoot != null)
            {
                Selection.activeGameObject = tileSystemRoot;
            }

            ShowCompletionDialog();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Setup failed: " + ex.Message);
            ShowErrorDialog(ex.Message);
        }
    }

    private static GameObject CreateHierarchyOnly()
    {
        // Check for existing system
        GameObject existing = GameObject.Find("TileSystem");
        if (existing != null)
        {
            bool replace = EditorUtility.DisplayDialog("SystemExists",
                "TileSystem bereits vorhanden. Ersetzen?", "Ja", "Nein");

            if (replace)
            {
                Object.DestroyImmediate(existing);
            }
            else
            {
                return existing;
            }
        }

        // Create clean hierarchy
        GameObject root = new GameObject("TileSystem");
        GameObject managers = new GameObject("Managers");
        GameObject ui = new GameObject("UI");
        GameObject tiles = new GameObject("Generated_Tiles");

        managers.transform.SetParent(root.transform);
        ui.transform.SetParent(root.transform);
        tiles.transform.SetParent(root.transform);

        return root;
    }

    private static void SetupComponentsBasic(GameObject systemRoot)
    {
        Transform managersContainer = systemRoot.transform.Find("Managers");
        Transform tilesContainer = systemRoot.transform.Find("Generated_Tiles");

        // Create TileManager without SerializedObject
        GameObject managerObj = new GameObject("TileManager");
        managerObj.transform.SetParent(managersContainer);

        TileManager manager = managerObj.AddComponent<TileManager>();

        // Direct field assignment - no SerializedObject needed
        // Note: This requires tileParent to be public or we set it via reflection
        SetTileParentDirectly(manager, tilesContainer);

        Debug.Log("SUCCESS: TileManager created");
    }

    private static void SetTileParentDirectly(TileManager manager, Transform tilesContainer)
    {
        // Use reflection to set private field safely
        var field = typeof(TileManager).GetField("tileParent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(manager, tilesContainer);
            Debug.Log("TileParent set via reflection");
        }
        else
        {
            Debug.LogWarning("Could not set tileParent - will need manual assignment");
        }
    }

    private static void SetupBasicUI()
    {
        // Find existing Canvas or skip UI creation
        Canvas canvas = null;
        Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
        if (canvases != null && canvases.Length > 0)
        {
            canvas = canvases[0];
        }

        if (canvas == null)
        {
            Debug.Log("No Canvas found - creating basic Canvas");
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject evtSystem = new GameObject("EventSystem");
                evtSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                evtSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        // Create simple UI without complex operations
        CreateMinimalNavigationUI(canvas.transform);
    }

    private static void CreateMinimalNavigationUI(Transform canvasParent)
    {
        // Check if UI already exists
        if (canvasParent.Find("NavigationPanel") != null)
        {
            Debug.Log("Navigation UI already exists");
            return;
        }

        // Create minimal UI structure
        GameObject panel = new GameObject("NavigationPanel");
        panel.transform.SetParent(canvasParent, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(150, -60);
        panelRect.sizeDelta = new Vector2(280, 100);

        // Background
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);

        // Text elements - minimal setup
        CreateSimpleText("DirectionText", panel.transform, "Nord", new Vector2(10, -15));
        CreateSimpleText("DistanceText", panel.transform, "0m", new Vector2(120, -15));
        CreateSimpleText("KeyTileCountText", panel.transform, "Ziele: 0", new Vector2(10, -45));

        // Arrow placeholder
        GameObject arrow = new GameObject("DirectionArrow");
        arrow.transform.SetParent(panel.transform, false);
        RectTransform arrowRect = arrow.AddComponent<RectTransform>();
        arrowRect.anchoredPosition = new Vector2(220, -30);
        arrowRect.sizeDelta = new Vector2(20, 20);
        Image arrowImg = arrow.AddComponent<Image>();
        arrowImg.color = Color.white;

        // Add NavigationUI component without SerializedObject setup
        TileNavigationUI navUI = panel.AddComponent<TileNavigationUI>();

        Debug.Log("SUCCESS: Basic Navigation UI created");
        Debug.Log("INFO: UI component references need manual assignment in Inspector");
    }

    private static void CreateSimpleText(string name, Transform parent, string text, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(100, 30);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 16;
        tmp.color = Color.white;
    }

    private static void ValidateBasicSetup(GameObject systemRoot)
    {
        bool valid = true;

        if (systemRoot == null)
        {
            Debug.LogError("ERROR: System root not created");
            valid = false;
        }

        TileManager manager = systemRoot.GetComponentInChildren<TileManager>();
        if (manager == null)
        {
            Debug.LogError("ERROR: TileManager not found");
            valid = false;
        }

        TileNavigationUI navUI = Object.FindObjectOfType<TileNavigationUI>();
        if (navUI == null)
        {
            Debug.LogWarning("WARNING: NavigationUI not found");
        }

        if (valid)
        {
            Debug.Log("SUCCESS: Basic validation passed");
        }
    }

    private static void ShowCompletionDialog()
    {
        EditorUtility.DisplayDialog("Setup Complete",
            "Tile System Grundsetup abgeschlossen!\n\n" +
            "WICHTIG - Manuelle Schritte:\n" +
            "1. TileManager Inspector: Tile-Prefabs zuweisen\n" +
            "2. NavigationUI Inspector: UI-Referenzen zuweisen\n" +
            "3. Play Mode: 'Generate New Layout' testen\n\n" +
            "Die UI-Referenzen muessen manuell im Inspector\n" +
            "verbunden werden (Direction Text, Distance Text, etc.)", "OK");
    }

    private static void ShowErrorDialog(string error)
    {
        EditorUtility.DisplayDialog("Setup Error",
            "Setup Fehler aufgetreten:\n\n" + error + "\n\n" +
            "Loesungsvorschlaege:\n" +
            "1. Unity Editor neustarten\n" +
            "2. Visual Scripting Package deaktivieren (temporaer)\n" +
            "3. Setup erneut versuchen", "OK");
    }

    // Safe runtime tools
    [MenuItem("Tools/Tile System/Generate New Layout")]
    public static void GenerateNewLayout()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Play Mode Required",
                "Funktion benoetigt Play Mode.", "OK");
            return;
        }

        TileManager manager = Object.FindObjectOfType<TileManager>();
        if (manager != null)
        {
            manager.GenerateNewTileLayout();
            Debug.Log("SUCCESS: New layout generated");
        }
        else
        {
            Debug.LogError("ERROR: TileManager not found");
        }
    }

    [MenuItem("Tools/Tile System/Manual Setup Instructions")]
    public static void ShowManualInstructions()
    {
        string instructions =
            "MANUELLE SETUP-SCHRITTE:\n\n" +
            "1. TILEMANAGER KONFIGURATION:\n" +
            "   - TileManager im Inspector oeffnen\n" +
            "   - Tile Parent: Generated_Tiles zuweisen\n" +
            "   - Standard Tiles Array: Boden-Prefabs zuweisen\n" +
            "   - Key Tiles Array: Ziel-Prefabs zuweisen\n" +
            "   - Multi Tiles Array: Grosse Prefabs zuweisen\n\n" +
            "2. NAVIGATION UI KONFIGURATION:\n" +
            "   - NavigationPanel im Inspector oeffnen\n" +
            "   - Direction Text: DirectionText GameObject zuweisen\n" +
            "   - Distance Text: DistanceText GameObject zuweisen\n" +
            "   - Key Tile Count Text: KeyTileCountText zuweisen\n" +
            "   - Direction Arrow: DirectionArrow Image zuweisen\n" +
            "   - Navigation Panel: NavigationPanel selbst zuweisen\n\n" +
            "3. TESTING:\n" +
            "   - Play Mode starten\n" +
            "   - Tools > Tile System > Generate New Layout\n" +
            "   - WASD-Bewegung testen\n\n" +
            "GRUND FUER MANUELLE SCHRITTE:\n" +
            "Unity Visual Scripting GraphObjects verhindern\n" +
            "automatische SerializedObject Operationen\n" +
            "waehrend Domain Reload.";

        EditorUtility.DisplayDialog("Manual Setup Instructions", instructions, "OK");
    }

    [MenuItem("Tools/Tile System/Troubleshooting")]
    public static void ShowTroubleshooting()
    {
        string troubleshooting =
            "TROUBLESHOOTING:\n\n" +
            "DOMAIN BACKUP ERROR:\n" +
            "- Ursache: Visual Scripting/Shader Graph Paket\n" +
            "- Loesung: Manuelles Setup verwenden\n\n" +
            "COMPONENTS MISSING:\n" +
            "- Inspector-Referenzen manuell zuweisen\n" +
            "- Generated_Tiles Transform verknuepfen\n\n" +
            "UI NOT WORKING:\n" +
            "- Canvas und EventSystem vorhanden pruefen\n" +
            "- NavigationUI Referenzen pruefen\n\n" +
            "TILES NOT GENERATING:\n" +
            "- Play Mode erforderlich\n" +
            "- TileManager Prefab Arrays zuweisen\n" +
            "- Console auf Fehlermeldungen pruefen\n\n" +
            "ALTERNATIVE LOESUNG:\n" +
            "Alle Komponenten manuell erstellen\n" +
            "ohne Editor-Automatisierung.";

        EditorUtility.DisplayDialog("Troubleshooting Guide", troubleshooting, "OK");
    }

    // Cleanup on domain reload
    private static void OnDisable()
    {
        EditorApplication.update -= SafeEditorUpdate;
    }
}

#endif

/*
====================================================================
SETUP-ANLEITUNG (MANUELL ERFORDERLICH):
====================================================================

GRUND FUER MANUELLES SETUP:
Unity Visual Scripting GraphObjects kollidieren mit automatischen
Editor-Operationen waehrend Domain Reload. Deshalb ist teilweise
manuelles Setup erforderlich.

AUTOMATISCHER TEIL:
Tools > Tile System > Setup Complete System
- Erstellt GameObject-Hierarchie
- Fuegt Komponenten hinzu
- Erstellt UI-Struktur

MANUELLER TEIL (ERFORDERLICH):
1. TileManager Inspector:
   - Tile Parent: Generated_Tiles GameObject
   - Standard/Multi/Key Tiles Arrays: Prefabs zuweisen

2. NavigationUI Inspector:
   - Direction/Distance/Count Text Referenzen
   - Arrow Image Referenz  
   - Navigation Panel Referenz

HILFE:
Tools > Tile System > Manual Setup Instructions
Tools > Tile System > Troubleshooting

GRUND: Unity 2023+ GraphObject Serialization-Konflikt
LOESUNG: Hybrides Setup (Auto + Manual)
====================================================================
*/