/*
====================================================================
* TileManager.cs - Sequential Grid Generation System v3.2
====================================================================
* Project: Space Colony Game
* Course: PIP
* Script-Developer: Julian
* Date: 23.09.2025
* Version: v3.2 - PivotPoint System Integration Complete
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUTHORSHIP CLASSIFICATION:
* [HUMAN-AUTHORED] - Grid logic concept, tile placement requirements, PivotPoint integration concept
* [AI-ASSISTED] - Sequential workflow algorithm, PivotPoint positioning system, constraint validation
====================================================================
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Resolution order system
public enum ResolutionOrderMode
{
    DistanceBased = 0,
    PriorityBased = 1,
    MixedMode = 2
}

// Simplified tile type enum
public enum TileType
{
    Ground = 0,
    Specific = 1,
    Composite = 2,
    Entry_Exit = 3
}

// Zone state enumeration
public enum ZoneState
{
    Undiscovered = 0,
    Discovered = 1,
    Activating = 2,
    Claimed = 3,
    Defended = 4
}

// Enhanced zone data structure
[System.Serializable]
public struct ZoneData
{
    public Vector2Int gridPosition;
    public Vector3 worldPosition;
    public ZoneState currentState;
    public float activationProgress;
    public GameObject terminalObject;
    public GameObject beaconObject;
    public bool hasRequiredMicrochip;
    public float lastActivationTime;
    public int resolutionPriority;
    public float distanceFromStart;
}

// Updated tile configuration with constraint system
[System.Serializable]
public struct TileConfiguration
{
    [Header("Basic Setup")]
    public GameObject prefab;
    public string displayName;

    [Header("Size & Position")]
    public Vector2Int size;
    public Vector2Int anchorOffset;

    [Header("Category")]
    public TileType tileType;
    public bool isKeyPoint;

    [Header("Generation Constraints")]
    public int minTileCount;
    public int maxTileCount;

    [Header("LOD System Integration")]
    public bool hasLODGroup;
    public float lodBias;

    [Header("Enemy AI Integration")]
    public bool isWalkable;
    public bool blocksLineOfSight;
}

// Border tile configuration
[System.Serializable]
public struct BorderTileConfiguration
{
    [Header("Border Setup")]
    public GameObject borderPrefab;
    public string displayName;

    [Header("Border Properties")]
    public Vector2Int borderSize;
    public float outerExtension;
    public bool hasLODGroup;
}

public class TileManager : MonoBehaviour
{
    public static TileManager Instance { get; private set; }

    [Header("Grid Configuration - FIXED")]
    private const int FIXED_GRID_SIZE = 18;
    private const float TILE_SIZE = 20f;
    private const int TOTAL_GRID_POSITIONS = FIXED_GRID_SIZE * FIXED_GRID_SIZE; // 324

    [SerializeField] private Transform tilesContainer;

    [Header("Border System")]
    [SerializeField] private BorderTileConfiguration borderConfiguration;

    [Header("Tile System - Constraint Based")]
    [SerializeField] private TileConfiguration[] tileConfigurations;

    [Header("Key Tile Distribution System - Simplified")]
    [SerializeField, Range(5f, 50f)] private float percentageDistance = 25f;

    [Header("Resolution Order System")]
    [SerializeField] private ResolutionOrderMode resolutionMode = ResolutionOrderMode.DistanceBased;
    [SerializeField] private Vector3 playerStartPosition = Vector3.zero;

    [Header("Zone Discovery Configuration")]
    [SerializeField] private float discoveryRange = 2.0f;
    [SerializeField] private float interactionRange = 1.5f;

    [Header("Performance")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private float generationDelay = 0.1f;

    [Header("Debug Visualization")]
    [SerializeField] private bool showDistributionGizmos = true;
    [SerializeField] private bool showResolutionOrder = true;
    [SerializeField] private bool showWarnings = true;

    // Grid Management
    private HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
    private List<Vector2Int> keyTilePositions = new List<Vector2Int>();
    private List<Vector2Int> compositeTilesGenerated = new List<Vector2Int>();
    private List<GameObject> spawnedTiles = new List<GameObject>();
    private GameObject borderTileInstance;

    // Zone management
    private Dictionary<Vector2Int, ZoneData> zoneDataMap = new Dictionary<Vector2Int, ZoneData>();

    // Sequential generation tracking
    private Dictionary<TileType, int> tilePlacementCount = new Dictionary<TileType, int>();

    // Warning system
    private List<string> generationWarnings = new List<string>();

    // Events
    public System.Action OnKeyTilesUpdated;
    public System.Action<Vector2Int> OnKeyTileReached;
    public System.Action<Vector2Int> OnZoneDiscovered;
    public System.Action<Vector2Int> OnZoneActivationStarted;
    public System.Action<Vector2Int, float> OnZoneActivationProgress;
    public System.Action<Vector2Int> OnZoneActivationComplete;
    public System.Action<Vector2Int> OnZoneUnderAttack;
    public System.Action<Vector2Int> OnBeaconActivated;
    public System.Action<List<string>> OnGenerationWarnings;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple TileManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (generateOnStart)
        {
            StartCoroutine(GenerateSequentialTerrain());
        }
    }

    void Update()
    {
        CheckPlayerProximity();
    }

    // SEQUENTIAL TERRAIN GENERATION WORKFLOW
    private IEnumerator GenerateSequentialTerrain()
    {
        ClearExistingTiles();
        InitializeGenerationState();

        yield return new WaitForSeconds(generationDelay);

        // PHASE 1: Constraint Validation
        if (!ValidateConstraints())
        {
            ReportWarnings();
            yield break;
        }

        // PHASE 2: Border Placement
        PlaceBorderTile();
        yield return null;

        // PHASE 3: Key Tile Placement
        yield return StartCoroutine(PlaceKeyTiles());

        // PHASE 4: Composite Tile Placement
        yield return StartCoroutine(PlaceCompositeTiles());

        // PHASE 5: Fill Remaining Positions
        yield return StartCoroutine(FillRemainingGrid());

        // PHASE 6: Final Validation
        PerformFinalValidation();

        // PHASE 7: Initialize Zone Data
        InitializeZoneDataWithOrder();

        OnKeyTilesUpdated?.Invoke();

        if (generationWarnings.Count > 0)
        {
            OnGenerationWarnings?.Invoke(generationWarnings);
        }
    }

    private void InitializeGenerationState()
    {
        occupiedPositions.Clear();
        keyTilePositions.Clear();
        compositeTilesGenerated.Clear();
        zoneDataMap.Clear();
        tilePlacementCount.Clear();
        generationWarnings.Clear();

        // Initialize tile counters
        foreach (TileConfiguration config in tileConfigurations)
        {
            tilePlacementCount[config.tileType] = 0;
        }
    }

    private bool ValidateConstraints()
    {
        bool isValid = true;
        int totalMinTiles = 0;
        int keyTileCount = 0;

        foreach (TileConfiguration config in tileConfigurations)
        {
            totalMinTiles += config.minTileCount;

            if (config.isKeyPoint)
            {
                keyTileCount++;
                if (config.maxTileCount != 0)
                {
                    generationWarnings.Add($"Key tile '{config.displayName}' should have maxTileCount = 0");
                }
            }
        }

        if (totalMinTiles > TOTAL_GRID_POSITIONS)
        {
            generationWarnings.Add($"Grid 18x18 nicht füllbar: minTileCount Summe ({totalMinTiles}) übersteigt {TOTAL_GRID_POSITIONS}");
            isValid = false;
        }

        if (keyTileCount == 0)
        {
            generationWarnings.Add("Keine Key-Tiles definiert (isKeyPoint = true)");
            isValid = false;
        }

        if (keyTileCount > 5)
        {
            generationWarnings.Add($"Zu viele Key-Tiles definiert ({keyTileCount}). Empfohlen: 3 für Zone Claiming");
        }

        return isValid;
    }

    private void PlaceBorderTile()
    {
        if (borderConfiguration.borderPrefab == null) return;

        Vector3 centerPosition = new Vector3(
            (FIXED_GRID_SIZE - 1) * TILE_SIZE * 0.5f,
            0f,
            (FIXED_GRID_SIZE - 1) * TILE_SIZE * 0.5f
        );

        borderTileInstance = Instantiate(borderConfiguration.borderPrefab, centerPosition, Quaternion.identity, tilesContainer);

        if (borderConfiguration.hasLODGroup)
        {
            ConfigureBorderLOD();
        }
    }

    private void ConfigureBorderLOD()
    {
        if (borderTileInstance == null) return;

        LODGroup lodGroup = borderTileInstance.GetComponent<LODGroup>();
        if (lodGroup != null)
        {
            // Border tiles get special LOD treatment for large scale visibility
            LOD[] lods = lodGroup.GetLODs();
            for (int i = 0; i < lods.Length; i++)
            {
                lods[i].screenRelativeTransitionHeight *= 2.0f; // Extended visibility
            }
            lodGroup.SetLODs(lods);
        }
    }

    private IEnumerator PlaceKeyTiles()
    {
        List<TileConfiguration> keyTileConfigs = GetKeyTileConfigurations();

        if (keyTileConfigs.Count == 0)
        {
            generationWarnings.Add("Keine Key-Tiles für Platzierung gefunden");
            yield break;
        }

        List<Vector2Int> keyPositions = CalculateKeyTilePositions(keyTileConfigs.Count);

        for (int i = 0; i < keyPositions.Count && i < keyTileConfigs.Count; i++)
        {
            Vector2Int position = keyPositions[i];
            TileConfiguration config = keyTileConfigs[i];

            if (CanPlaceTile(position, config.size))
            {
                PlaceTile(position, config);
                keyTilePositions.Add(position);
                tilePlacementCount[config.tileType]++;
            }
            else
            {
                generationWarnings.Add($"Key tile '{config.displayName}' konnte nicht bei {position} platziert werden - Kollision erkannt");

                Vector2Int fallback = FindNearestValidPosition(position, config.size, 3);
                if (fallback.x != -1)
                {
                    PlaceTile(fallback, config);
                    keyTilePositions.Add(fallback);
                    tilePlacementCount[config.tileType]++;
                }
            }

            yield return null;
        }
    }

    private Vector2Int FindNearestValidPosition(Vector2Int originalPosition, Vector2Int size, int searchRadius)
    {
        for (int radius = 1; radius <= searchRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius) continue;

                    Vector2Int candidate = originalPosition + new Vector2Int(x, y);

                    if (IsInGrid(candidate) && CanPlaceTile(candidate, size))
                    {
                        return candidate;
                    }
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    private List<TileConfiguration> GetKeyTileConfigurations()
    {
        List<TileConfiguration> keyConfigs = new List<TileConfiguration>();

        foreach (TileConfiguration config in tileConfigurations)
        {
            if (config.isKeyPoint)
            {
                keyConfigs.Add(config);
            }
        }

        return keyConfigs;
    }

    private List<Vector2Int> CalculateKeyTilePositions(int keyTileCount)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        float minDistance = CalculateMinimumDistance();

        for (int attempt = 0; attempt < 2000 && positions.Count < keyTileCount; attempt++)
        {
            Vector2Int candidate = new Vector2Int(
                Random.Range(2, FIXED_GRID_SIZE - 2),
                Random.Range(2, FIXED_GRID_SIZE - 2)
            );

            if (IsValidKeyTilePosition(candidate, positions, minDistance))
            {
                positions.Add(candidate);
            }
        }

        if (positions.Count < keyTileCount)
        {
            generationWarnings.Add($"Nur {positions.Count} von {keyTileCount} Key-Tiles konnten platziert werden - MinDistance {minDistance:F1} zu restriktiv");
        }

        return positions;
    }

    private float CalculateMinimumDistance()
    {
        return (percentageDistance / 100f) * FIXED_GRID_SIZE;
    }

    private bool IsValidKeyTilePosition(Vector2Int candidate, List<Vector2Int> existingPositions, float minDistance)
    {
        if (!IsInGrid(candidate)) return false;
        if (occupiedPositions.Contains(candidate)) return false;

        foreach (Vector2Int existing in existingPositions)
        {
            if (Vector2Int.Distance(candidate, existing) < minDistance)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsInGrid(Vector2Int position)
    {
        return position.x >= 0 && position.x < FIXED_GRID_SIZE &&
               position.y >= 0 && position.y < FIXED_GRID_SIZE;
    }

    private IEnumerator PlaceCompositeTiles()
    {
        List<TileConfiguration> compositeConfigs = GetCompositeTileConfigurations();

        foreach (TileConfiguration config in compositeConfigs)
        {
            int placedCount = 0;
            int attempts = 0;
            int maxAttempts = TOTAL_GRID_POSITIONS;

            while (placedCount < config.minTileCount && attempts < maxAttempts)
            {
                Vector2Int candidate = GetRandomAvailablePosition();

                if (candidate.x == -1) break;

                if (CanPlaceTile(candidate, config.size))
                {
                    PlaceTile(candidate, config);
                    compositeTilesGenerated.Add(candidate);
                    tilePlacementCount[config.tileType]++;
                    placedCount++;
                    yield return null;
                }

                attempts++;
            }

            if (placedCount < config.minTileCount)
            {
                generationWarnings.Add($"Composite tile '{config.displayName}': Nur {placedCount} von {config.minTileCount} platziert - nicht genug freie {config.size} Bereiche");
            }
        }
    }

    private List<TileConfiguration> GetCompositeTileConfigurations()
    {
        List<TileConfiguration> compositeConfigs = new List<TileConfiguration>();

        foreach (TileConfiguration config in tileConfigurations)
        {
            if (config.tileType == TileType.Composite && !config.isKeyPoint)
            {
                compositeConfigs.Add(config);
            }
        }

        return compositeConfigs;
    }

    private IEnumerator FillRemainingGrid()
    {
        List<TileConfiguration> fillerConfigs = GetFillerTileConfigurations();
        int batchSize = 20;
        int tilesProcessed = 0;

        while (occupiedPositions.Count < TOTAL_GRID_POSITIONS && fillerConfigs.Count > 0)
        {
            Vector2Int position = GetRandomAvailablePosition();

            if (position.x == -1) break;

            TileConfiguration selectedConfig = SelectFillerTile(fillerConfigs);

            if (CanPlaceTile(position, selectedConfig.size))
            {
                PlaceTile(position, selectedConfig);
                tilePlacementCount[selectedConfig.tileType]++;
                tilesProcessed++;

                if (tilesProcessed % batchSize == 0)
                {
                    yield return null;
                }
            }
        }
    }

    private List<TileConfiguration> GetFillerTileConfigurations()
    {
        List<TileConfiguration> fillerConfigs = new List<TileConfiguration>();

        foreach (TileConfiguration config in tileConfigurations)
        {
            if (!config.isKeyPoint && config.tileType != TileType.Composite)
            {
                fillerConfigs.Add(config);
            }
        }

        return fillerConfigs;
    }

    private TileConfiguration SelectFillerTile(List<TileConfiguration> availableConfigs)
    {
        List<TileConfiguration> validConfigs = new List<TileConfiguration>();

        foreach (TileConfiguration config in availableConfigs)
        {
            int currentCount = tilePlacementCount.GetValueOrDefault(config.tileType, 0);

            if (currentCount < config.maxTileCount || config.maxTileCount == 0)
            {
                validConfigs.Add(config);
            }
        }

        if (validConfigs.Count == 0) return availableConfigs[0];

        return validConfigs[Random.Range(0, validConfigs.Count)];
    }

    private Vector2Int GetRandomAvailablePosition()
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>();

        for (int x = 0; x < FIXED_GRID_SIZE; x++)
        {
            for (int y = 0; y < FIXED_GRID_SIZE; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!occupiedPositions.Contains(pos))
                {
                    availablePositions.Add(pos);
                }
            }
        }

        return availablePositions.Count > 0 ? availablePositions[Random.Range(0, availablePositions.Count)] : new Vector2Int(-1, -1);
    }

    private bool CanPlaceTile(Vector2Int position, Vector2Int size)
    {
        if (position.x + size.x > FIXED_GRID_SIZE || position.y + size.y > FIXED_GRID_SIZE)
            return false;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int checkPos = new Vector2Int(position.x + x, position.y + y);
                if (occupiedPositions.Contains(checkPos))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void PlaceTile(Vector2Int gridPosition, TileConfiguration tileConfig)
    {
        // CRITICAL: Mark positions BEFORE instantiation
        MarkOccupiedPositions(gridPosition, tileConfig.size);

        // PIVOTPOINT-AWARE COORDINATE SYSTEM
        Vector3 worldPos = GridToWorldPosition(gridPosition);

        GameObject tileInstance = Instantiate(tileConfig.prefab, worldPos, Quaternion.identity, tilesContainer);

        // PIVOTPOINT INTEGRATION: Check for PivotPoint GameObject and adjust position
        Transform pivotPoint = tileInstance.transform.Find("PivotPoint");
        if (pivotPoint != null)
        {
            // Calculate offset from PivotPoint to desired grid position
            Vector3 pivotOffset = pivotPoint.position - tileInstance.transform.position;
            Vector3 correctedPosition = worldPos - pivotOffset;

            // Apply corrected position to align PivotPoint with grid position
            tileInstance.transform.position = correctedPosition;
        }
        // Fallback: Use Transform.position for tiles without PivotPoint (existing behavior)

        spawnedTiles.Add(tileInstance);

        if (tileConfig.hasLODGroup)
        {
            ConfigureTileLOD(tileInstance, tileConfig);
        }

        // Runtime bounds validation - only in debug builds
#if UNITY_EDITOR
        if (showDistributionGizmos)
        {
            StartCoroutine(ValidatePlacedTileBounds(tileInstance, gridPosition, tileConfig));
        }
#endif
    }

    private IEnumerator ValidatePlacedTileBounds(GameObject tileInstance, Vector2Int gridPosition, TileConfiguration config)
    {
        yield return null;

        Renderer renderer = tileInstance.GetComponent<Renderer>();
        if (renderer != null)
        {
            Bounds actualBounds = renderer.bounds;
            Vector2 gridBounds = new Vector2(config.size.x * TILE_SIZE, config.size.y * TILE_SIZE);
            Vector2 actualSize = new Vector2(actualBounds.size.x, actualBounds.size.z);

            if (actualSize.x > gridBounds.x * 1.1f || actualSize.y > gridBounds.y * 1.1f)
            {
                generationWarnings.Add($"Tile '{config.displayName}' at {gridPosition}: Actual size {actualSize:F1} exceeds grid size {gridBounds:F1}");
            }
        }
    }

    private void ConfigureTileLOD(GameObject tileInstance, TileConfiguration config)
    {
        LODGroup lodGroup = tileInstance.GetComponent<LODGroup>();
        if (lodGroup != null)
        {
            float lodBias = config.isKeyPoint ? config.lodBias * 1.5f : config.lodBias;

            if (lodBias != 1.0f)
            {
                LOD[] lods = lodGroup.GetLODs();
                for (int i = 0; i < lods.Length; i++)
                {
                    lods[i].screenRelativeTransitionHeight *= lodBias;
                }
                lodGroup.SetLODs(lods);
            }
        }
    }

    private void MarkOccupiedPositions(Vector2Int position, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                occupiedPositions.Add(new Vector2Int(position.x + x, position.y + y));
            }
        }
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * TILE_SIZE, 0f, gridPos.y * TILE_SIZE);
    }

    private Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / TILE_SIZE),
            Mathf.RoundToInt(worldPos.z / TILE_SIZE)
        );
    }

    private void PerformFinalValidation()
    {
        foreach (TileConfiguration config in tileConfigurations)
        {
            int placedCount = tilePlacementCount.GetValueOrDefault(config.tileType, 0);

            if (placedCount < config.minTileCount)
            {
                generationWarnings.Add($"'{config.displayName}': {placedCount} platziert, {config.minTileCount} minimum erforderlich");
            }

            if (config.maxTileCount > 0 && placedCount > config.maxTileCount)
            {
                generationWarnings.Add($"'{config.displayName}': {placedCount} platziert, {config.maxTileCount} maximum überschritten");
            }
        }

        if (keyTilePositions.Count < 3)
        {
            generationWarnings.Add($"Nur {keyTilePositions.Count} Key-Tiles platziert. Empfohlen: 3 für Zone Claiming");
        }

        if (occupiedPositions.Count != TOTAL_GRID_POSITIONS)
        {
            generationWarnings.Add($"Grid nicht vollständig gefüllt: {occupiedPositions.Count}/{TOTAL_GRID_POSITIONS} Positionen");
        }
    }

    private void ReportWarnings()
    {
        if (generationWarnings.Count > 0 && showWarnings)
        {
            Debug.LogWarning("=== TILE GENERATION WARNINGS ===");
            foreach (string warning in generationWarnings)
            {
                Debug.LogWarning($"WARNING: {warning}");
            }
            Debug.LogWarning("=== END WARNINGS ===");
        }
    }

    // ZONE MANAGEMENT SYSTEM
    private void InitializeZoneDataWithOrder()
    {
        List<Vector2Int> orderedPositions = new List<Vector2Int>(keyTilePositions);

        switch (resolutionMode)
        {
            case ResolutionOrderMode.DistanceBased:
                OrderByDistanceFromStart(orderedPositions);
                break;
            case ResolutionOrderMode.PriorityBased:
                break;
            case ResolutionOrderMode.MixedMode:
                OrderByDistanceFromStart(orderedPositions);
                break;
        }

        for (int i = 0; i < orderedPositions.Count; i++)
        {
            Vector2Int keyPos = orderedPositions[i];

            ZoneData newZone = new ZoneData
            {
                gridPosition = keyPos,
                worldPosition = GridToWorldPosition(keyPos),
                currentState = ZoneState.Undiscovered,
                activationProgress = 0f,
                terminalObject = null,
                beaconObject = null,
                hasRequiredMicrochip = false,
                lastActivationTime = 0f,
                resolutionPriority = i + 1,
                distanceFromStart = Vector3.Distance(GridToWorldPosition(keyPos), playerStartPosition)
            };

            zoneDataMap[keyPos] = newZone;
        }
    }

    private void OrderByDistanceFromStart(List<Vector2Int> positions)
    {
        positions.Sort((a, b) => {
            float distanceA = Vector3.Distance(GridToWorldPosition(a), playerStartPosition);
            float distanceB = Vector3.Distance(GridToWorldPosition(b), playerStartPosition);
            return distanceA.CompareTo(distanceB);
        });
    }

    // PLAYER PROXIMITY CHECKING
    private float lastProximityCheck = 0f;
    private float proximityCheckInterval = 0.1f;

    private void CheckPlayerProximity()
    {
        if (Time.time - lastProximityCheck > proximityCheckInterval)
        {
            Vector2Int playerGridPos = WorldToGridPosition(GetPlayerPosition());

            foreach (Vector2Int keyPos in keyTilePositions)
            {
                float distance = Vector2Int.Distance(playerGridPos, keyPos);

                if (distance < discoveryRange && GetZoneState(keyPos) == ZoneState.Undiscovered)
                {
                    DiscoverZone(keyPos);
                    OnZoneDiscovered?.Invoke(keyPos);
                }

                if (distance < interactionRange)
                {
                    OnKeyTileReached?.Invoke(keyPos);
                }
            }

            lastProximityCheck = Time.time;
        }
    }

    private Vector3 GetPlayerPosition()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        return player != null ? player.transform.position : Vector3.zero;
    }

    // ZONE STATE MANAGEMENT
    public void UpdateZoneState(Vector2Int position, ZoneState newState)
    {
        if (zoneDataMap.ContainsKey(position))
        {
            ZoneData currentData = zoneDataMap[position];
            currentData.currentState = newState;
            zoneDataMap[position] = currentData;
        }
    }

    public ZoneState GetZoneState(Vector2Int position)
    {
        if (zoneDataMap.ContainsKey(position))
        {
            return zoneDataMap[position].currentState;
        }
        return ZoneState.Undiscovered;
    }

    public void DiscoverZone(Vector2Int position)
    {
        if (zoneDataMap.ContainsKey(position))
        {
            UpdateZoneState(position, ZoneState.Discovered);
        }
    }

    // PUBLIC API
    public List<Vector2Int> GetKeyTilePositions()
    {
        return keyTilePositions;
    }

    public Vector3 GetNearestKeyTileWorldPosition(Vector3 fromPosition)
    {
        Vector2Int fromGrid = WorldToGridPosition(fromPosition);
        float nearestDistance = float.MaxValue;
        Vector2Int nearestKeyTile = Vector2Int.zero;

        foreach (Vector2Int keyPos in keyTilePositions)
        {
            float distance = Vector2Int.Distance(fromGrid, keyPos);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestKeyTile = keyPos;
            }
        }

        return GridToWorldPosition(nearestKeyTile);
    }

    private void ClearExistingTiles()
    {
        foreach (GameObject tile in spawnedTiles)
        {
            if (tile != null)
                DestroyImmediate(tile);
        }
        spawnedTiles.Clear();

        if (borderTileInstance != null)
        {
            DestroyImmediate(borderTileInstance);
            borderTileInstance = null;
        }
    }

    // CONTEXT MENU FUNCTIONS
    [ContextMenu("Generate Sequential Test Grid")]
    public void GenerateTestGrid()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        StartCoroutine(GenerateSequentialTerrain());
    }

    [ContextMenu("Validate Tile Constraints")]
    public void ValidateTileConstraints()
    {
        generationWarnings.Clear();

        if (ValidateConstraints())
        {
            Debug.Log("PASSED: Tile constraints validation - Grid 18x18 kann sauber erstellt werden");
        }
        else
        {
            Debug.LogError("FAILED: Tile constraints validation:");
            ReportWarnings();
        }
    }

    [ContextMenu("Show Generation Statistics")]
    public void ShowGenerationStatistics()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Debug.Log("=== SEQUENTIAL GENERATION STATISTICS ===");
        Debug.Log($"Grid Size: {FIXED_GRID_SIZE}x{FIXED_GRID_SIZE} = {TOTAL_GRID_POSITIONS} positions");
        Debug.Log($"Occupied Positions: {occupiedPositions.Count}/{TOTAL_GRID_POSITIONS}");
        Debug.Log($"Key Tiles Placed: {keyTilePositions.Count}");
        Debug.Log($"Composite Tiles: {compositeTilesGenerated.Count}");
        Debug.Log($"Total Spawned Tiles: {spawnedTiles.Count}");
        Debug.Log($"Border Tile: {(borderTileInstance != null ? "Placed" : "Not Placed")}");

        foreach (var kvp in tilePlacementCount)
        {
            Debug.Log($"TileType.{kvp.Key}: {kvp.Value} placed");
        }

        if (generationWarnings.Count > 0)
        {
            Debug.Log($"Warnings Generated: {generationWarnings.Count}");
        }
    }

    // DEBUG VISUALIZATION
    private void OnDrawGizmosSelected()
    {
        if (!showDistributionGizmos) return;

        // Draw fixed 18x18 grid bounds
        Gizmos.color = Color.white;
        Vector3 gridCenter = new Vector3((FIXED_GRID_SIZE - 1) * TILE_SIZE * 0.5f, 0f, (FIXED_GRID_SIZE - 1) * TILE_SIZE * 0.5f);
        Vector3 gridSize3D = new Vector3(FIXED_GRID_SIZE * TILE_SIZE, 0.1f, FIXED_GRID_SIZE * TILE_SIZE);
        Gizmos.DrawWireCube(gridCenter, gridSize3D);

        if (!Application.isPlaying) return;

        // Draw key tile positions
        Gizmos.color = Color.green;
        foreach (Vector2Int keyPos in keyTilePositions)
        {
            Vector3 worldPos = GridToWorldPosition(keyPos);
            Gizmos.DrawWireCube(worldPos + Vector3.up * 0.5f, Vector3.one);
        }

        // Draw composite tiles
        Gizmos.color = Color.blue;
        foreach (Vector2Int compositePos in compositeTilesGenerated)
        {
            Vector3 worldPos = GridToWorldPosition(compositePos);
            Gizmos.DrawWireCube(worldPos + Vector3.up * 0.3f, Vector3.one * 0.6f);
        }

        // Draw zone states with resolution order
        foreach (var kvp in zoneDataMap)
        {
            Vector3 worldPos = GridToWorldPosition(kvp.Key);

            switch (kvp.Value.currentState)
            {
                case ZoneState.Undiscovered:
                    Gizmos.color = Color.gray;
                    break;
                case ZoneState.Discovered:
                    Gizmos.color = Color.yellow;
                    break;
                case ZoneState.Activating:
                    Gizmos.color = Color.red;
                    break;
                case ZoneState.Claimed:
                    Gizmos.color = Color.green;
                    break;
                case ZoneState.Defended:
                    Gizmos.color = Color.magenta;
                    break;
            }

            Gizmos.DrawSphere(worldPos + Vector3.up * 1.5f, 0.3f);

            if (showResolutionOrder)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.white;
                UnityEditor.Handles.Label(worldPos + Vector3.up * 2f, kvp.Value.resolutionPriority.ToString());
#endif
            }
        }

        // Draw player start position
        if (playerStartPosition != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerStartPosition, 1f);

#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.blue;
            UnityEditor.Handles.Label(playerStartPosition + Vector3.up * 1.5f, "START");
#endif
        }

        // Draw border tile bounds
        if (borderTileInstance != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 borderCenter = borderTileInstance.transform.position;
            Gizmos.DrawWireCube(borderCenter, Vector3.one * (FIXED_GRID_SIZE * TILE_SIZE + borderConfiguration.outerExtension * 2f));
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Clamp values to reasonable ranges
        percentageDistance = Mathf.Clamp(percentageDistance, 5f, 50f);

        // Validate tile configurations
        if (tileConfigurations != null)
        {
            foreach (TileConfiguration config in tileConfigurations)
            {
                if (config.isKeyPoint && config.maxTileCount > 0)
                {
                    Debug.LogWarning($"Key tile '{config.displayName}' should have maxTileCount = 0");
                }
            }
        }
    }
#endif
}