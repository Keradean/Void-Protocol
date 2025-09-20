/*
====================================================================
* TileManager.cs - Enhanced Key Tile Distribution System
====================================================================
* Project: Space Colony Game
* Course: PIP
* Script-Developer: Julian
* Date: 20.09.2025
* Version: Distribution Enhanced - Academic Attribution
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUTHORSHIP CLASSIFICATION:
* [HUMAN-AUTHORED] - Grid logic, tile placement, key tile distribution concept, resolution ordering
* [AI-ASSISTED] - Distribution algorithms, Poisson disk sampling, performance optimization, inspector controls
====================================================================
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Distribution method enumeration
public enum KeyTileDistributionMethod
{
    PercentageBased = 0,    // Distance as percentage of grid size
    FixedMeterDistance = 1, // Absolute world space distance
    GridCellSpacing = 2,    // Distance in grid cells
    PoissonDiskSampling = 3 // Even distribution with natural variation
}

// Resolution order system
public enum ResolutionOrderMode
{
    DistanceBased = 0,      // Resolve nearest zones first (automatic)
    PriorityBased = 1,      // Manual int priority assignment
    MixedMode = 2           // Priority override with distance fallback
}

// PHASE 1: Simplified tile type enum
public enum TileType
{
    Ground = 0,      // 70% distribution - Grund-Elemente
    Specific = 1,    // 20% distribution - Spezifische Elemente  
    Composite = 2,   // 8% distribution  - Zusammengesetzte Elemente
    KeyPoint = 3     // 2% distribution  - Key-Points (3-6 total)
}

// ZONE CLAIMING: Zone state enumeration
public enum ZoneState
{
    Undiscovered = 0,    // Player hasn't found zone yet
    Discovered = 1,      // Zone found, terminal visible
    Activating = 2,      // Microchip inserted, countdown active
    Claimed = 3,         // Activation complete, beacon online
    Defended = 4         // Under attack during activation
}

// ZONE CLAIMING: Enhanced zone data structure with resolution order
[System.Serializable]
public struct ZoneData
{
    public Vector2Int gridPosition;     // Grid coordinates
    public Vector3 worldPosition;      // World coordinates
    public ZoneState currentState;     // Current zone status
    public float activationProgress;   // 0.0f to 1.0f completion
    public GameObject terminalObject;  // Reference to terminal prefab
    public GameObject beaconObject;    // Reference to beacon prefab
    public bool hasRequiredMicrochip;  // Inventory check result
    public float lastActivationTime;   // Time tracking for cooldowns
    public int resolutionPriority;     // Manual resolution order (1-6)
    public float distanceFromStart;    // Distance from spawn point
}

// PHASE 1: LOD-optimized tile configuration
[System.Serializable]
public struct TileConfiguration
{
    [Header("Basic Setup")]
    public GameObject prefab;              // Tile prefab with LOD Group
    public string displayName;             // Inspector display name

    [Header("Size & Position")]
    public Vector2Int size;                // Default (1,1) for single tiles
    public Vector2Int anchorOffset;        // Anchor point within tile

    [Header("Category")]
    public TileType tileType;              // 4-category system
    public bool isKeyPoint;                // Key point integration

    [Header("Distribution")]
    public int spawnWeight;                // Simple integer weight (1-100)

    [Header("LOD System Integration")]
    public bool hasLODGroup;               // Prefab contains LOD Group
    public float lodBias;                  // LOD bias multiplier (0.5-2.0)

    [Header("Enemy AI Integration")]
    public bool isWalkable;                // For enemy pathfinding
    public bool blocksLineOfSight;         // For enemy detection
}

public class TileManager : MonoBehaviour
{
    public static TileManager Instance { get; private set; }

    [Header("Grid Configuration")]
    [SerializeField] private int gridSize = 5;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private Transform tilesContainer;

    [Header("Tile System - Phase 1")]
    [SerializeField] private TileConfiguration[] tileConfigurations;
    [SerializeField] private int keyTileCount = 5;

    [Header("Key Tile Distribution System")]
    [SerializeField] private KeyTileDistributionMethod distributionMethod = KeyTileDistributionMethod.PercentageBased;

    [Header("Distribution Parameters")]
    [SerializeField, Range(5f, 50f)] private float percentageDistance = 25f; // 25% of grid size
    [SerializeField, Range(1f, 50f)] private float fixedMeterDistance = 15f; // 15 Unity units
    [SerializeField, Range(2, 8)] private int gridCellSpacing = 4; // 4 grid cells apart
    [SerializeField, Range(1, 10)] private int poissonSamples = 5; // Poisson disk sampling attempts

    [Header("Resolution Order System")]
    [SerializeField] private ResolutionOrderMode resolutionMode = ResolutionOrderMode.DistanceBased;
    [SerializeField] private bool enforceDistributionInTerrain = true; // Always distribute in terrain
    [SerializeField] private Vector3 playerStartPosition = Vector3.zero; // For distance calculations

    [Header("Zone Discovery Configuration")]
    [SerializeField] private float discoveryRange = 2.0f;
    [SerializeField] private float interactionRange = 1.5f;

    [Header("Performance")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private float generationDelay = 0.1f;

    [Header("Debug Visualization")]
    [SerializeField] private bool showDistributionGizmos = true;
    [SerializeField] private bool showResolutionOrder = true;

    // Grid Management
    private HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
    private List<Vector2Int> keyTilePositions = new List<Vector2Int>();
    private List<GameObject> spawnedTiles = new List<GameObject>();

    // ZONE CLAIMING: Zone management
    private Dictionary<Vector2Int, ZoneData> zoneDataMap = new Dictionary<Vector2Int, ZoneData>();

    // Distribution system
    private List<Vector2Int> distributedKeyPositions = new List<Vector2Int>();

    // Existing Events
    public System.Action OnKeyTilesUpdated;
    public System.Action<Vector2Int> OnKeyTileReached;

    // ZONE CLAIMING: New zone events
    public System.Action<Vector2Int> OnZoneDiscovered;
    public System.Action<Vector2Int> OnZoneActivationStarted;
    public System.Action<Vector2Int, float> OnZoneActivationProgress;
    public System.Action<Vector2Int> OnZoneActivationComplete;
    public System.Action<Vector2Int> OnZoneUnderAttack;
    public System.Action<Vector2Int> OnBeaconActivated;

    private int failSafeAmount = 1000;

    private void Awake()
    {
        // Singleton pattern
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
            StartCoroutine(GenerateTerrainCoroutine());
        }
    }

    void Update()
    {
        // Enhanced player proximity checking
        CheckPlayerProximity();
    }

    // ENHANCED: Terrain generation with guaranteed key tile distribution
    private IEnumerator GenerateTerrainCoroutine()
    {
        ClearExistingTiles();
        occupiedPositions.Clear();
        keyTilePositions.Clear();
        zoneDataMap.Clear();
        distributedKeyPositions.Clear();

        yield return new WaitForSeconds(generationDelay);

        // STEP 1: Calculate optimal key tile positions using selected distribution method
        CalculateKeyTileDistribution();

        // STEP 2: Generate terrain with guaranteed key tile placement
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);

                if (!occupiedPositions.Contains(gridPos))
                {
                    TileConfiguration selectedTile;

                    // Force key tile placement at distributed positions
                    if (distributedKeyPositions.Contains(gridPos))
                    {
                        selectedTile = GetKeyTileConfiguration();
                        Debug.Log($"Placing guaranteed key tile at {gridPos}");
                    }
                    else
                    {
                        // Normal tile selection (excluding key tiles)
                        selectedTile = SelectRandomNonKeyTile();
                    }

                    if (CanPlaceTile(gridPos, selectedTile.size))
                    {
                        PlaceTile(gridPos, selectedTile);
                    }
                }
            }

            // Yield every row for smooth generation
            yield return null;
        }

        // STEP 3: Initialize zone data with resolution order
        InitializeZoneDataWithOrder();

        OnKeyTilesUpdated?.Invoke();

        Debug.Log($"Terrain generation complete: {spawnedTiles.Count} tiles, {keyTilePositions.Count} key points");
        Debug.Log($"Key tile distribution method: {distributionMethod}");
        Debug.Log($"Resolution order mode: {resolutionMode}");
        Debug.Log($"Zone data initialized: {zoneDataMap.Count} zones");
    }

    // KEY TILE DISTRIBUTION: Calculate optimal positions using selected method
    private void CalculateKeyTileDistribution()
    {
        distributedKeyPositions.Clear();

        switch (distributionMethod)
        {
            case KeyTileDistributionMethod.PercentageBased:
                CalculatePercentageBasedDistribution();
                break;
            case KeyTileDistributionMethod.FixedMeterDistance:
                CalculateFixedMeterDistribution();
                break;
            case KeyTileDistributionMethod.GridCellSpacing:
                CalculateGridCellDistribution();
                break;
            case KeyTileDistributionMethod.PoissonDiskSampling:
                CalculatePoissonDiskDistribution();
                break;
        }

        Debug.Log($"Calculated {distributedKeyPositions.Count} key tile positions using {distributionMethod}");
    }

    private void CalculatePercentageBasedDistribution()
    {
        float minDistanceGrid = (percentageDistance / 100f) * gridSize;

        for (int attempt = 0; attempt < keyTileCount * 50 && distributedKeyPositions.Count < keyTileCount; attempt++)
        {
            Vector2Int candidate = new Vector2Int(
                Random.Range(0, gridSize),
                Random.Range(0, gridSize)
            );

            if (IsValidKeyTilePosition(candidate, minDistanceGrid))
            {
                distributedKeyPositions.Add(candidate);
            }
        }
    }

    private void CalculateFixedMeterDistribution()
    {
        float minDistanceGrid = fixedMeterDistance / tileSize;

        for (int attempt = 0; attempt < keyTileCount * 50 && distributedKeyPositions.Count < keyTileCount; attempt++)
        {
            Vector2Int candidate = new Vector2Int(
                Random.Range(0, gridSize),
                Random.Range(0, gridSize)
            );

            if (IsValidKeyTilePosition(candidate, minDistanceGrid))
            {
                distributedKeyPositions.Add(candidate);
            }
        }
    }

    private void CalculateGridCellDistribution()
    {
        for (int attempt = 0; attempt < keyTileCount * 50 && distributedKeyPositions.Count < keyTileCount; attempt++)
        {
            Vector2Int candidate = new Vector2Int(
                Random.Range(0, gridSize),
                Random.Range(0, gridSize)
            );

            if (IsValidKeyTilePosition(candidate, gridCellSpacing))
            {
                distributedKeyPositions.Add(candidate);
            }
        }
    }

    private void CalculatePoissonDiskDistribution()
    {
        // Poisson disk sampling for even distribution
        float minDistance = (25f / 100f) * gridSize; // Default to 25% spacing
        List<Vector2Int> activeList = new List<Vector2Int>();
        bool[,] grid = new bool[gridSize, gridSize];

        // Start with random first point
        Vector2Int firstPoint = new Vector2Int(
            Random.Range(0, gridSize),
            Random.Range(0, gridSize)
        );

        distributedKeyPositions.Add(firstPoint);
        activeList.Add(firstPoint);
        grid[firstPoint.x, firstPoint.y] = true;

        while (activeList.Count > 0 && distributedKeyPositions.Count < keyTileCount)
        {
            int randomIndex = Random.Range(0, activeList.Count);
            Vector2Int currentPoint = activeList[randomIndex];
            bool foundValid = false;

            for (int sample = 0; sample < poissonSamples; sample++)
            {
                Vector2Int candidate = GenerateRandomPointAround(currentPoint, minDistance);

                if (IsInGrid(candidate) && !grid[candidate.x, candidate.y] &&
                    IsValidKeyTilePosition(candidate, minDistance))
                {
                    distributedKeyPositions.Add(candidate);
                    activeList.Add(candidate);
                    grid[candidate.x, candidate.y] = true;
                    foundValid = true;
                    break;
                }
            }

            if (!foundValid)
            {
                activeList.RemoveAt(randomIndex);
            }
        }
    }

    private Vector2Int GenerateRandomPointAround(Vector2Int center, float minDistance)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(minDistance, minDistance * 2f);

        int x = Mathf.RoundToInt(center.x + Mathf.Cos(angle) * distance);
        int y = Mathf.RoundToInt(center.y + Mathf.Sin(angle) * distance);

        return new Vector2Int(x, y);
    }

    private bool IsInGrid(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridSize &&
               position.y >= 0 && position.y < gridSize;
    }

    private bool IsValidKeyTilePosition(Vector2Int candidate, float minDistance)
    {
        // Check if position is within grid bounds
        if (!IsInGrid(candidate)) return false;

        // Check minimum distance from existing key tiles
        foreach (Vector2Int existing in distributedKeyPositions)
        {
            float distance = Vector2Int.Distance(candidate, existing);
            if (distance < minDistance)
            {
                return false;
            }
        }

        return true;
    }

    // ZONE CLAIMING: Initialize zone data with resolution order
    private void InitializeZoneDataWithOrder()
    {
        List<Vector2Int> orderedPositions = new List<Vector2Int>(keyTilePositions);

        // Apply resolution ordering
        switch (resolutionMode)
        {
            case ResolutionOrderMode.DistanceBased:
                OrderByDistanceFromStart(orderedPositions);
                break;
            case ResolutionOrderMode.PriorityBased:
                // Manual priorities will be set in inspector or via separate method
                break;
            case ResolutionOrderMode.MixedMode:
                OrderByDistanceFromStart(orderedPositions); // Default order, can be overridden
                break;
        }

        // Initialize zone data with calculated order
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
                resolutionPriority = i + 1, // 1-based priority
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

    // ENHANCED: Non-key tile selection (excludes key tiles from random generation)
    private TileConfiguration SelectRandomNonKeyTile()
    {
        // Step 1: Select category (excluding KeyPoint)
        TileType selectedCategory = SelectNonKeyTileCategory();

        // Step 2: Find tiles of selected category
        List<TileConfiguration> categoryTiles = new List<TileConfiguration>();
        foreach (TileConfiguration tile in tileConfigurations)
        {
            if (tile.tileType == selectedCategory && !tile.isKeyPoint)
                categoryTiles.Add(tile);
        }

        // Step 3: Select random tile from category
        if (categoryTiles.Count == 0)
        {
            // Fallback to any non-key tile
            foreach (TileConfiguration tile in tileConfigurations)
            {
                if (!tile.isKeyPoint)
                {
                    return tile;
                }
            }
            return tileConfigurations[0]; // Ultimate fallback
        }

        return SelectWeightedRandom(categoryTiles);
    }

    private TileType SelectNonKeyTileCategory()
    {
        // Redistributed percentages without KeyPoint
        int randomValue = Random.Range(0, 98);
        if (randomValue < 71) return TileType.Ground;      // ~72%
        if (randomValue < 92) return TileType.Specific;    // ~21% 
        return TileType.Composite;                         // ~7%
    }

    // Original tile selection (kept for compatibility)
    private TileConfiguration SelectRandomTile()
    {
        TileType selectedCategory = SelectTileCategory();

        List<TileConfiguration> categoryTiles = new List<TileConfiguration>();
        foreach (TileConfiguration tile in tileConfigurations)
        {
            if (tile.tileType == selectedCategory)
                categoryTiles.Add(tile);
        }

        if (categoryTiles.Count == 0)
            return tileConfigurations[0];

        return SelectWeightedRandom(categoryTiles);
    }

    private TileType SelectTileCategory()
    {
        // This method is now primarily for fallback use
        // Normal generation uses guaranteed key tile placement
        if (keyTilePositions.Count >= keyTileCount)
        {
            return SelectNonKeyTileCategory();
        }

        int randomValue2 = Random.Range(0, 100);
        if (randomValue2 < 70) return TileType.Ground;
        if (randomValue2 < 90) return TileType.Specific;
        if (randomValue2 < 98) return TileType.Composite;
        return TileType.KeyPoint;
    }

    private TileConfiguration SelectWeightedRandom(List<TileConfiguration> tiles)
    {
        if (tiles.Count == 1) return tiles[0];

        int totalWeight = 0;
        foreach (var tile in tiles) totalWeight += tile.spawnWeight;

        if (totalWeight == 0) return tiles[0];

        int randomWeight = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var tile in tiles)
        {
            currentWeight += tile.spawnWeight;
            if (randomWeight < currentWeight) return tile;
        }

        return tiles[0];
    }

    private bool CanPlaceTile(Vector2Int position, Vector2Int size)
    {
        if (position.x + size.x > gridSize || position.y + size.y > gridSize)
            return false;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int checkPos = new Vector2Int(position.x + x, position.y + y);
                if (occupiedPositions.Contains(checkPos))
                    return false;
            }
        }

        return true;
    }

    private void PlaceTile(Vector2Int gridPosition, TileConfiguration tileConfig)
    {
        Vector3 worldPos = GridToWorldPosition(gridPosition + tileConfig.anchorOffset);

        GameObject tileInstance = Instantiate(tileConfig.prefab, worldPos, Quaternion.identity, tilesContainer);
        spawnedTiles.Add(tileInstance);

        if (tileConfig.hasLODGroup)
        {
            ConfigureTileLOD(tileInstance, tileConfig);
        }

        if (tileConfig.isKeyPoint)
        {
            keyTilePositions.Add(gridPosition);
        }

        MarkOccupiedPositions(gridPosition, tileConfig.size);
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
        return new Vector3(gridPos.x * tileSize, 0f, gridPos.y * tileSize);
    }

    private void EnsureKeyTileCount()
    {
        // This method is now less critical since we guarantee placement
        // But kept for edge case handling
        var failSafeCounter = 0;

        while (keyTilePositions.Count < 3)
        {
            failSafeCounter++;
            Vector2Int randomPos = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));
            if (!occupiedPositions.Contains(randomPos))
            {
                TileConfiguration keyTileConfig = GetKeyTileConfiguration();
                if (keyTileConfig.prefab != null)
                {
                    PlaceTile(randomPos, keyTileConfig);
                }
            }

            if (failSafeCounter > failSafeAmount)
            {
                Debug.LogError("FailSafeTriggered!!! Using guaranteed distribution system instead.");
                break;
            }
        }
    }

    private TileConfiguration GetKeyTileConfiguration()
    {
        foreach (var tile in tileConfigurations)
        {
            if (tile.isKeyPoint) return tile;
        }
        return tileConfigurations[0];
    }

    // ENHANCED: Player proximity checking with zone discovery
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

    private Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / tileSize),
            Mathf.RoundToInt(worldPos.z / tileSize)
        );
    }

    private void ClearExistingTiles()
    {
        foreach (GameObject tile in spawnedTiles)
        {
            if (tile != null)
                DestroyImmediate(tile);
        }
        spawnedTiles.Clear();
    }

    // ZONE CLAIMING: Zone state management methods
    public void UpdateZoneState(Vector2Int position, ZoneState newState)
    {
        if (zoneDataMap.ContainsKey(position))
        {
            ZoneData currentData = zoneDataMap[position];
            currentData.currentState = newState;
            zoneDataMap[position] = currentData;

            Debug.Log($"Zone {position} state updated to {newState}");
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

    public List<ZoneData> GetZonesInState(ZoneState targetState)
    {
        List<ZoneData> result = new List<ZoneData>();
        foreach (var kvp in zoneDataMap)
        {
            if (kvp.Value.currentState == targetState)
            {
                result.Add(kvp.Value);
            }
        }
        return result;
    }

    public ZoneData GetNearestZone(Vector3 playerPosition, ZoneState targetState)
    {
        Vector2Int playerGrid = WorldToGridPosition(playerPosition);
        float nearestDistance = float.MaxValue;
        ZoneData nearestZone = default(ZoneData);
        bool foundAny = false;

        foreach (var kvp in zoneDataMap)
        {
            if (kvp.Value.currentState == targetState)
            {
                float distance = Vector2Int.Distance(playerGrid, kvp.Key);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestZone = kvp.Value;
                    foundAny = true;
                }
            }
        }

        if (!foundAny && zoneDataMap.Count > 0)
        {
            var firstZone = new List<ZoneData>(zoneDataMap.Values)[0];
            return firstZone;
        }

        return nearestZone;
    }

    public void DiscoverZone(Vector2Int position)
    {
        if (zoneDataMap.ContainsKey(position))
        {
            UpdateZoneState(position, ZoneState.Discovered);
            Debug.Log($"Zone discovered at {position}");
        }
    }

    public bool IsZoneDiscovered(Vector2Int position)
    {
        ZoneState state = GetZoneState(position);
        return state != ZoneState.Undiscovered;
    }

    public void SpawnTerminal(Vector2Int position)
    {
        if (zoneDataMap.ContainsKey(position))
        {
            ZoneData currentData = zoneDataMap[position];
            Debug.Log($"Terminal spawned at {position}");
            zoneDataMap[position] = currentData;
        }
    }

    public void ActivateBeacon(Vector2Int position)
    {
        if (zoneDataMap.ContainsKey(position))
        {
            ZoneData currentData = zoneDataMap[position];
            Debug.Log($"Beacon activated at {position}");
            zoneDataMap[position] = currentData;
            OnBeaconActivated?.Invoke(position);
        }
    }

    public GameObject GetTerminalAtPosition(Vector2Int position)
    {
        if (zoneDataMap.ContainsKey(position))
        {
            return zoneDataMap[position].terminalObject;
        }
        return null;
    }

    // ZONE CLAIMING: Progress tracking
    public float GetActivationProgress(Vector2Int position)
    {
        if (zoneDataMap.ContainsKey(position))
        {
            return zoneDataMap[position].activationProgress;
        }
        return 0f;
    }

    public void SetActivationProgress(Vector2Int position, float progress)
    {
        if (zoneDataMap.ContainsKey(position))
        {
            ZoneData currentData = zoneDataMap[position];
            currentData.activationProgress = Mathf.Clamp01(progress);
            zoneDataMap[position] = currentData;
        }
    }

    public int GetClaimedZoneCount()
    {
        int count = 0;
        foreach (var kvp in zoneDataMap)
        {
            if (kvp.Value.currentState == ZoneState.Claimed)
            {
                count++;
            }
        }
        return count;
    }

    public int GetTotalZoneCount()
    {
        return zoneDataMap.Count;
    }

    // RESOLUTION ORDER: Manual priority management
    public void SetZoneResolutionPriority(Vector2Int position, int priority)
    {
        if (zoneDataMap.ContainsKey(position))
        {
            ZoneData currentData = zoneDataMap[position];
            currentData.resolutionPriority = priority;
            zoneDataMap[position] = currentData;
            Debug.Log($"Zone {position} resolution priority set to {priority}");
        }
    }

    public int GetZoneResolutionPriority(Vector2Int position)
    {
        if (zoneDataMap.ContainsKey(position))
        {
            return zoneDataMap[position].resolutionPriority;
        }
        return 0;
    }

    public List<ZoneData> GetZonesByResolutionOrder()
    {
        List<ZoneData> zones = new List<ZoneData>(zoneDataMap.Values);

        switch (resolutionMode)
        {
            case ResolutionOrderMode.PriorityBased:
                zones.Sort((a, b) => a.resolutionPriority.CompareTo(b.resolutionPriority));
                break;
            case ResolutionOrderMode.DistanceBased:
                zones.Sort((a, b) => a.distanceFromStart.CompareTo(b.distanceFromStart));
                break;
            case ResolutionOrderMode.MixedMode:
                // Priority first, then distance for ties
                zones.Sort((a, b) => {
                    int priorityCompare = a.resolutionPriority.CompareTo(b.resolutionPriority);
                    return priorityCompare != 0 ? priorityCompare : a.distanceFromStart.CompareTo(b.distanceFromStart);
                });
                break;
        }

        return zones;
    }

    // DISTRIBUTION VALIDATION: Inspector helper methods
    [ContextMenu("Validate Distribution Settings")]
    public void ValidateDistributionSettings()
    {
        bool isValid = true;

        // Check if distribution parameters are reasonable
        switch (distributionMethod)
        {
            case KeyTileDistributionMethod.PercentageBased:
                if (percentageDistance < 10f)
                {
                    Debug.LogWarning("Percentage distance too small - may cause clustering");
                    isValid = false;
                }
                break;
            case KeyTileDistributionMethod.FixedMeterDistance:
                float maxDistance = gridSize * tileSize;
                if (fixedMeterDistance > maxDistance * 0.5f)
                {
                    Debug.LogWarning($"Fixed meter distance too large for grid size. Max recommended: {maxDistance * 0.5f}");
                    isValid = false;
                }
                break;
            case KeyTileDistributionMethod.GridCellSpacing:
                float maxSpacing = gridSize * 0.5f;
                if (gridCellSpacing > maxSpacing)
                {
                    Debug.LogWarning($"Grid cell spacing too large. Max recommended: {maxSpacing}");
                    isValid = false;
                }
                break;
        }

        // Check key tile count vs grid size
        float gridArea = gridSize * gridSize;
        float requiredArea = keyTileCount * (gridCellSpacing * gridCellSpacing);
        if (requiredArea > gridArea * 0.5f)
        {
            Debug.LogWarning("Too many key tiles for current distribution settings and grid size");
            isValid = false;
        }

        if (isValid)
        {
            Debug.Log("Distribution settings validation PASSED");
        }
        else
        {
            Debug.LogError("Distribution settings validation FAILED - adjust parameters");
        }
    }

    [ContextMenu("Recalculate Key Tile Distribution")]
    public void RecalculateDistribution()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Recalculation only available during play mode");
            return;
        }

        // Clear existing distribution
        distributedKeyPositions.Clear();

        // Recalculate with current settings
        CalculateKeyTileDistribution();

        Debug.Log($"Recalculated distribution: {distributedKeyPositions.Count} positions");
    }

    // DISTRIBUTION INFO: Get distribution statistics
    public void LogDistributionStatistics()
    {
        if (distributedKeyPositions.Count == 0)
        {
            Debug.Log("No key tile distribution calculated yet");
            return;
        }

        // Calculate average distance between key tiles
        float totalDistance = 0f;
        int pairCount = 0;

        for (int i = 0; i < distributedKeyPositions.Count; i++)
        {
            for (int j = i + 1; j < distributedKeyPositions.Count; j++)
            {
                totalDistance += Vector2Int.Distance(distributedKeyPositions[i], distributedKeyPositions[j]);
                pairCount++;
            }
        }

        float averageDistance = pairCount > 0 ? totalDistance / pairCount : 0f;
        float averageWorldDistance = averageDistance * tileSize;

        Debug.Log($"=== KEY TILE DISTRIBUTION STATISTICS ===");
        Debug.Log($"Distribution Method: {distributionMethod}");
        Debug.Log($"Total Key Tiles: {distributedKeyPositions.Count}");
        Debug.Log($"Average Grid Distance: {averageDistance:F2} cells");
        Debug.Log($"Average World Distance: {averageWorldDistance:F2} units");
        Debug.Log($"Grid Coverage: {(distributedKeyPositions.Count / (float)(gridSize * gridSize)) * 100f:F1}%");

        // Resolution order info
        Debug.Log($"Resolution Mode: {resolutionMode}");
        if (resolutionMode != ResolutionOrderMode.DistanceBased)
        {
            List<ZoneData> orderedZones = GetZonesByResolutionOrder();
            Debug.Log("Resolution Order:");
            for (int i = 0; i < orderedZones.Count; i++)
            {
                ZoneData zone = orderedZones[i];
                Debug.Log($"  {i + 1}. Grid({zone.gridPosition.x},{zone.gridPosition.y}) Priority:{zone.resolutionPriority} Distance:{zone.distanceFromStart:F1}");
            }
        }
    }

    // Public interface for navigation UI (enhanced)
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

    // ZONE CLAIMING: Get zone data by position
    public ZoneData? GetZoneData(Vector2Int position)
    {
        if (zoneDataMap.ContainsKey(position))
        {
            return zoneDataMap[position];
        }
        return null;
    }

    // DISTRIBUTION: Get distributed positions for external access
    public List<Vector2Int> GetDistributedKeyPositions()
    {
        return new List<Vector2Int>(distributedKeyPositions);
    }

    public float GetActualAverageKeyTileDistance()
    {
        if (keyTilePositions.Count < 2) return 0f;

        float totalDistance = 0f;
        int pairCount = 0;

        for (int i = 0; i < keyTilePositions.Count; i++)
        {
            for (int j = i + 1; j < keyTilePositions.Count; j++)
            {
                totalDistance += Vector2Int.Distance(keyTilePositions[i], keyTilePositions[j]);
                pairCount++;
            }
        }

        return pairCount > 0 ? (totalDistance / pairCount) * tileSize : 0f;
    }

    // Enhanced debug visualization
    private void OnDrawGizmosSelected()
    {
        if (!showDistributionGizmos) return;

        // Draw grid bounds
        Gizmos.color = Color.white;
        Vector3 gridCenter = new Vector3((gridSize - 1) * tileSize * 0.5f, 0f, (gridSize - 1) * tileSize * 0.5f);
        Vector3 gridSize3D = new Vector3(gridSize * tileSize, 0.1f, gridSize * tileSize);
        Gizmos.DrawWireCube(gridCenter, gridSize3D);

        if (!Application.isPlaying) return;

        // Draw distributed key positions (planned)
        if (distributedKeyPositions != null && distributedKeyPositions.Count > 0)
        {
            Gizmos.color = Color.cyan;
            foreach (Vector2Int pos in distributedKeyPositions)
            {
                Vector3 worldPos = GridToWorldPosition(pos);
                Gizmos.DrawWireCube(worldPos + Vector3.up * 0.5f, Vector3.one * 0.8f);
            }
        }

        // Draw actual key tile positions
        Gizmos.color = Color.green;
        foreach (Vector2Int keyPos in keyTilePositions)
        {
            Vector3 worldPos = GridToWorldPosition(keyPos);
            Gizmos.DrawWireCube(worldPos + Vector3.up * 0.5f, Vector3.one);
        }

        // Draw zone states with resolution order
        foreach (var kvp in zoneDataMap)
        {
            Vector3 worldPos = GridToWorldPosition(kvp.Key);

            // Color based on zone state
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

            // Draw resolution order numbers
            if (showResolutionOrder)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.white;
                UnityEditor.Handles.Label(worldPos + Vector3.up * 2f, kvp.Value.resolutionPriority.ToString());
#endif
            }
        }

        // Draw distribution lines between key tiles
        if (distributionMethod == KeyTileDistributionMethod.PoissonDiskSampling ||
            distributionMethod == KeyTileDistributionMethod.PercentageBased)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            for (int i = 0; i < keyTilePositions.Count; i++)
            {
                for (int j = i + 1; j < keyTilePositions.Count; j++)
                {
                    Vector3 posA = GridToWorldPosition(keyTilePositions[i]) + Vector3.up * 0.5f;
                    Vector3 posB = GridToWorldPosition(keyTilePositions[j]) + Vector3.up * 0.5f;
                    Gizmos.DrawLine(posA, posB);
                }
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
    }

    // INSPECTOR HELPER: Runtime distribution adjustment
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Clamp values to reasonable ranges
        percentageDistance = Mathf.Clamp(percentageDistance, 5f, 50f);
        fixedMeterDistance = Mathf.Clamp(fixedMeterDistance, 1f, 50f);
        gridCellSpacing = Mathf.Clamp(gridCellSpacing, 2, 8);
        poissonSamples = Mathf.Clamp(poissonSamples, 1, 10);
        keyTileCount = Mathf.Clamp(keyTileCount, 3, 10);

        // Auto-validation during editor changes
        if (Application.isPlaying)
        {
            ValidateDistributionSettings();
        }
    }
#endif
}