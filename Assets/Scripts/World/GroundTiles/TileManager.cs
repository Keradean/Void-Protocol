/*
====================================================================
* TileManager.cs - Phase 1: Multi-Tile System Foundation
====================================================================
* Project: Space Colony Game
* Course: PIP
* Script-Developer: Julian
* Date: 18.09.2025
* Version: Phase 1 - Academic Attribution
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUTHORSHIP CLASSIFICATION:
* [HUMAN-AUTHORED] - Grid logic, tile placement, category system design
* [AI-ASSISTED] - LOD integration, struct optimization, performance patterns
====================================================================
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PHASE 1: Simplified tile type enum
public enum TileType
{
    Ground = 0,      // 70% distribution - Grund-Elemente
    Specific = 1,    // 20% distribution - Spezifische Elemente  
    Composite = 2,   // 8% distribution  - Zusammengesetzte Elemente
    KeyPoint = 3     // 2% distribution  - Key-Points (3-6 total)
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
    [Header("Grid Configuration")]
    [SerializeField] private int gridSize = 20;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private Transform tilesContainer;

    [Header("Tile System - Phase 1")]
    [SerializeField] private TileConfiguration[] tileConfigurations;
    [SerializeField] private int keyTileCount = 5;

    [Header("Performance")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private float generationDelay = 0.1f;

    // Grid Management
    private HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
    private List<Vector2Int> keyTilePositions = new List<Vector2Int>();
    private List<GameObject> spawnedTiles = new List<GameObject>();

    // Events
    public System.Action OnKeyTilesUpdated;
    public System.Action<Vector2Int> OnKeyTileReached;

    void Start()
    {
        if (generateOnStart)
        {
            StartCoroutine(GenerateTerrainCoroutine());
        }
    }

    void Update()
    {
        CheckPlayerProximity();
    }

    // PHASE 1: LOD-optimized terrain generation
    private IEnumerator GenerateTerrainCoroutine()
    {
        ClearExistingTiles();
        occupiedPositions.Clear();
        keyTilePositions.Clear();

        yield return new WaitForSeconds(generationDelay);

        // Generate terrain with category-based distribution
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);

                if (!occupiedPositions.Contains(gridPos))
                {
                    TileConfiguration selectedTile = SelectRandomTile();

                    if (CanPlaceTile(gridPos, selectedTile.size))
                    {
                        PlaceTile(gridPos, selectedTile);
                    }
                }
            }

            // Yield every row for smooth generation
            yield return null;
        }

        // Ensure minimum key tiles
        EnsureKeyTileCount();
        OnKeyTilesUpdated?.Invoke();

        Debug.Log($"Terrain generation complete: {spawnedTiles.Count} tiles, {keyTilePositions.Count} key points");
    }

    // PHASE 1: Simple category-based tile selection
    private TileConfiguration SelectRandomTile()
    {
        // Step 1: Select category based on distribution
        TileType selectedCategory = SelectTileCategory();

        // Step 2: Find tiles of selected category
        List<TileConfiguration> categoryTiles = new List<TileConfiguration>();
        foreach (TileConfiguration tile in tileConfigurations)
        {
            if (tile.tileType == selectedCategory)
                categoryTiles.Add(tile);
        }

        // Step 3: Select random tile from category
        if (categoryTiles.Count == 0)
            return tileConfigurations[0]; // Fallback

        return SelectWeightedRandom(categoryTiles);
    }

    // Simple distribution matching Julian's direct style
    private TileType SelectTileCategory()
    {
        // Handle key tile count limit
        if (keyTilePositions.Count >= keyTileCount)
        {
            // No more key tiles allowed
            int randomValue = Random.Range(0, 98);
            if (randomValue < 71) return TileType.Ground;      // ~72%
            if (randomValue < 92) return TileType.Specific;    // ~21% 
            return TileType.Composite;                         // ~7%
        }

        // Normal distribution
        int randomValue2 = Random.Range(0, 100);
        if (randomValue2 < 70) return TileType.Ground;      // 70%
        if (randomValue2 < 90) return TileType.Specific;    // 20% 
        if (randomValue2 < 98) return TileType.Composite;   // 8%
        return TileType.KeyPoint;                           // 2%
    }

    private TileConfiguration SelectWeightedRandom(List<TileConfiguration> tiles)
    {
        if (tiles.Count == 1) return tiles[0];

        // Simple weighted selection
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

        return tiles[0]; // Fallback
    }

    // PHASE 1: Multi-tile placement with basic overlap prevention
    private bool CanPlaceTile(Vector2Int position, Vector2Int size)
    {
        // Boundary check
        if (position.x + size.x > gridSize || position.y + size.y > gridSize)
            return false;

        // Overlap check
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
        // Calculate world position
        Vector3 worldPos = GridToWorldPosition(gridPosition + tileConfig.anchorOffset);

        // Instantiate tile
        GameObject tileInstance = Instantiate(tileConfig.prefab, worldPos, Quaternion.identity, tilesContainer);
        spawnedTiles.Add(tileInstance);

        // Configure LOD if present
        if (tileConfig.hasLODGroup)
        {
            ConfigureTileLOD(tileInstance, tileConfig);
        }

        // Register key point
        if (tileConfig.isKeyPoint)
        {
            keyTilePositions.Add(gridPosition);
        }

        // Mark occupied positions
        MarkOccupiedPositions(gridPosition, tileConfig.size);
    }

    // LOD configuration per tile
    private void ConfigureTileLOD(GameObject tileInstance, TileConfiguration config)
    {
        LODGroup lodGroup = tileInstance.GetComponent<LODGroup>();
        if (lodGroup != null)
        {
            // Key tiles get higher LOD priority
            float lodBias = config.isKeyPoint ? config.lodBias * 1.5f : config.lodBias;

            // Apply LOD bias (simplified)
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

    private Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * tileSize, 0f, gridPos.y * tileSize);
    }

    // Ensure minimum key tile count
    private void EnsureKeyTileCount()
    {
        while (keyTilePositions.Count < 3)
        {
            // Find random empty position for additional key tile
            Vector2Int randomPos = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));
            if (!occupiedPositions.Contains(randomPos))
            {
                // Find a key tile configuration
                TileConfiguration keyTileConfig = GetKeyTileConfiguration();
                if (keyTileConfig.prefab != null)
                {
                    PlaceTile(randomPos, keyTileConfig);
                }
            }
        }
    }

    private TileConfiguration GetKeyTileConfiguration()
    {
        foreach (var tile in tileConfigurations)
        {
            if (tile.isKeyPoint) return tile;
        }
        return tileConfigurations[0]; // Fallback
    }

    // Player proximity checking (optimized from original)
    private float lastProximityCheck = 0f;
    private float proximityCheckInterval = 0.1f;

    private void CheckPlayerProximity()
    {
        if (Time.time - lastProximityCheck > proximityCheckInterval)
        {
            Vector2Int playerGridPos = WorldToGridPosition(GetPlayerPosition());

            foreach (Vector2Int keyPos in keyTilePositions)
            {
                if (Vector2Int.Distance(playerGridPos, keyPos) < 1.5f)
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

    // Public interface for navigation UI
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

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        foreach (Vector2Int keyPos in keyTilePositions)
        {
            Vector3 worldPos = GridToWorldPosition(keyPos);
            Gizmos.DrawWireCube(worldPos + Vector3.up * 0.5f, Vector3.one);
        }
    }
}