/*
====================================================================
* TILEMANAGER - Tile Grid Generation System
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
* [HUMAN-AUTHORED] - System architecture and game logic design
* [AI-ASSISTED] - Implementation optimization and error handling
* [AI-GENERATED] - Grid algorithms and placement calculations
====================================================================
*/

using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridSize = 20;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private Transform tileParent;

    [Header("Tile Prefabs")]
    [SerializeField] private TilePrefabData[] standardTiles;
    [SerializeField] private TilePrefabData[] multiTiles;
    [SerializeField] private TilePrefabData[] keyTiles;

    [Header("Key Tile Config")]
    [SerializeField] private int minKeyTiles = 3;
    [SerializeField] private int maxKeyTiles = 6;
    [SerializeField] private float keyTileMinDistance = 5f;

    [Header("Debug")]
    [SerializeField] private bool showGridGizmos = true;
    [SerializeField] private bool debugKeyTiles = true;

    // Events - Simple like Julian's pattern
    public System.Action<List<KeyTileInfo>> OnKeyTilesUpdated;
    public System.Action<KeyTileInfo> OnKeyTileReached;

    // Grid data
    private TileGridCell[,] grid;
    private List<KeyTileInfo> activeKeyTiles = new List<KeyTileInfo>();
    private List<GameObject> allSpawnedTiles = new List<GameObject>();

    // Cached reference - Julian's style
    private PlayerController player;

    private void Start()
    {
        InitializeSystem();
        GenerateNewTileLayout();
    }

    private void Update()
    {
        CheckPlayerProximity();
    }

    #region Initialization

    private void InitializeSystem()
    {
        player = FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogWarning("TileManager: PlayerController not found. Navigation will be limited.");
        }

        CreateTileParentIfNeeded();
        InitializeGrid();

        Debug.Log("TileManager: System initialized");
    }

    private void CreateTileParentIfNeeded()
    {
        if (tileParent == null)
        {
            GameObject parent = new GameObject("Generated_Tiles");
            parent.transform.SetParent(transform);
            tileParent = parent.transform;
        }
    }

    private void InitializeGrid()
    {
        grid = new TileGridCell[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                grid[x, z] = new TileGridCell
                {
                    worldPosition = new Vector3(x * tileSize, 0, z * tileSize),
                    isOccupied = false,
                    isKeyTile = false,
                    occupyingTile = null
                };
            }
        }
    }

    #endregion

    #region Public API

    public void GenerateNewTileLayout()
    {
        ClearExistingLayout();

        PlaceKeyTiles();
        PlaceMultiTiles();
        FillRemainingWithStandardTiles();

        NotifyKeyTilesUpdated();

        if (debugKeyTiles)
        {
            Debug.Log($"Generated layout: {activeKeyTiles.Count} key tiles placed");
        }
    }

    public KeyTileInfo GetNearestUnvisitedKeyTile(Vector3 fromPosition)
    {
        KeyTileInfo nearest = null;
        float shortestDistance = float.MaxValue;

        foreach (KeyTileInfo keyTile in activeKeyTiles)
        {
            if (keyTile.isVisited) continue;

            float distance = Vector3.Distance(fromPosition, keyTile.worldPosition);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearest = keyTile;
            }
        }

        return nearest;
    }

    public Vector3 GetDirectionToNearestKeyTile(Vector3 fromPosition)
    {
        KeyTileInfo nearest = GetNearestUnvisitedKeyTile(fromPosition);
        if (nearest == null) return Vector3.zero;

        return (nearest.worldPosition - fromPosition).normalized;
    }

    public int GetUnvisitedKeyTileCount()
    {
        int count = 0;
        foreach (KeyTileInfo keyTile in activeKeyTiles)
        {
            if (!keyTile.isVisited) count++;
        }
        return count;
    }

    #endregion

    #region Tile Placement

    private void PlaceKeyTiles()
    {
        if (keyTiles == null || keyTiles.Length == 0) return;

        int targetKeyTileCount = Random.Range(minKeyTiles, maxKeyTiles + 1);
        List<Vector2Int> usedPositions = new List<Vector2Int>();

        for (int i = 0; i < targetKeyTileCount; i++)
        {
            Vector2Int position = FindValidKeyTilePosition(usedPositions);
            if (position.x == -1) continue; // Invalid position

            TilePrefabData selectedKeyTile = keyTiles[Random.Range(0, keyTiles.Length)];
            GameObject keyTileObject = CreateTileAtPosition(selectedKeyTile, position);

            if (keyTileObject != null)
            {
                KeyTileInfo keyInfo = new KeyTileInfo
                {
                    tileObject = keyTileObject,
                    gridPosition = position,
                    worldPosition = grid[position.x, position.y].worldPosition,
                    tileType = selectedKeyTile.tileName,
                    isVisited = false
                };

                activeKeyTiles.Add(keyInfo);
                usedPositions.Add(position);
                SetCellAsKeyTile(position, selectedKeyTile.size);
            }
        }
    }

    private Vector2Int FindValidKeyTilePosition(List<Vector2Int> existingPositions)
    {
        int attempts = 0;
        const int maxAttempts = 50;

        while (attempts < maxAttempts)
        {
            int x = Random.Range(1, gridSize - 1); // Avoid edges
            int z = Random.Range(1, gridSize - 1);
            Vector2Int candidate = new Vector2Int(x, z);

            if (!IsCellAvailable(candidate))
            {
                attempts++;
                continue;
            }

            // Check distance to existing key tiles
            bool validDistance = true;
            foreach (Vector2Int existing in existingPositions)
            {
                float distance = Vector2Int.Distance(candidate, existing);
                if (distance < keyTileMinDistance)
                {
                    validDistance = false;
                    break;
                }
            }

            if (validDistance) return candidate;
            attempts++;
        }

        Debug.LogWarning("Could not find valid key tile position");
        return new Vector2Int(-1, -1); // Invalid marker
    }

    private void PlaceMultiTiles()
    {
        if (multiTiles == null || multiTiles.Length == 0) return;

        int multiTileCount = Random.Range(2, 5);
        for (int i = 0; i < multiTileCount; i++)
        {
            TilePrefabData selectedMultiTile = multiTiles[Random.Range(0, multiTiles.Length)];
            Vector2Int position = FindValidMultiTilePosition(selectedMultiTile);

            if (position.x != -1)
            {
                CreateTileAtPosition(selectedMultiTile, position);
            }
        }
    }

    private Vector2Int FindValidMultiTilePosition(TilePrefabData multiTileData)
    {
        int attempts = 0;
        const int maxAttempts = 30;

        while (attempts < maxAttempts)
        {
            int x = Random.Range(0, gridSize - multiTileData.size.x);
            int z = Random.Range(0, gridSize - multiTileData.size.y);
            Vector2Int position = new Vector2Int(x, z);

            if (CanPlaceMultiTile(position, multiTileData.size))
            {
                return position;
            }
            attempts++;
        }

        return new Vector2Int(-1, -1);
    }

    private bool CanPlaceMultiTile(Vector2Int startPosition, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                Vector2Int checkPosition = startPosition + new Vector2Int(x, z);
                if (!IsCellAvailable(checkPosition))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void FillRemainingWithStandardTiles()
    {
        if (standardTiles == null || standardTiles.Length == 0) return;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector2Int position = new Vector2Int(x, z);
                if (IsCellAvailable(position))
                {
                    TilePrefabData selectedTile = standardTiles[Random.Range(0, standardTiles.Length)];
                    CreateTileAtPosition(selectedTile, position);
                }
            }
        }
    }

    private GameObject CreateTileAtPosition(TilePrefabData tileData, Vector2Int gridPosition)
    {
        if (tileData.prefab == null || !IsValidGridPosition(gridPosition))
        {
            return null;
        }

        Vector3 worldPosition = grid[gridPosition.x, gridPosition.y].worldPosition;
        GameObject tileObject = Instantiate(tileData.prefab, worldPosition,
                                          tileData.prefab.transform.rotation, tileParent);

        allSpawnedTiles.Add(tileObject);
        MarkCellsAsOccupied(gridPosition, tileData.size, tileObject);

        return tileObject;
    }

    #endregion

    #region Grid Management

    private bool IsValidGridPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridSize &&
               position.y >= 0 && position.y < gridSize;
    }

    private bool IsCellAvailable(Vector2Int position)
    {
        return IsValidGridPosition(position) && !grid[position.x, position.y].isOccupied;
    }

    private void MarkCellsAsOccupied(Vector2Int startPosition, Vector2Int size, GameObject tileObject)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                Vector2Int cellPosition = startPosition + new Vector2Int(x, z);
                if (IsValidGridPosition(cellPosition))
                {
                    grid[cellPosition.x, cellPosition.y].isOccupied = true;
                    grid[cellPosition.x, cellPosition.y].occupyingTile = tileObject;
                }
            }
        }
    }

    private void SetCellAsKeyTile(Vector2Int startPosition, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                Vector2Int cellPosition = startPosition + new Vector2Int(x, z);
                if (IsValidGridPosition(cellPosition))
                {
                    grid[cellPosition.x, cellPosition.y].isKeyTile = true;
                }
            }
        }
    }

    private void ResetGrid()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                grid[x, z].isOccupied = false;
                grid[x, z].isKeyTile = false;
                grid[x, z].occupyingTile = null;
            }
        }
    }

    private void ClearExistingLayout()
    {
        // Destroy all spawned tiles
        foreach (GameObject tile in allSpawnedTiles)
        {
            if (tile != null)
            {
                DestroyImmediate(tile);
            }
        }

        allSpawnedTiles.Clear();
        activeKeyTiles.Clear();
        ResetGrid();
    }

    #endregion

    #region Player Interaction

    private void CheckPlayerProximity()
    {
        if (player == null) return;

        Vector3 playerPosition = player.transform.position;

        foreach (KeyTileInfo keyTile in activeKeyTiles)
        {
            if (keyTile.isVisited) continue;

            float distance = Vector3.Distance(playerPosition, keyTile.worldPosition);
            if (distance <= 2.5f) // Close enough to trigger
            {
                keyTile.isVisited = true;
                OnKeyTileReached?.Invoke(keyTile);
                NotifyKeyTilesUpdated();

                if (debugKeyTiles)
                {
                    Debug.Log($"Key tile reached: {keyTile.tileType}");
                }
                break; // Only trigger one per frame
            }
        }
    }

    private void NotifyKeyTilesUpdated()
    {
        OnKeyTilesUpdated?.Invoke(activeKeyTiles);
    }

    #endregion

    #region Debug Visualization

    private void OnDrawGizmos()
    {
        if (!showGridGizmos) return;

        DrawGridLines();
        DrawKeyTiles();
    }

    private void DrawGridLines()
    {
        if (grid == null) return;

        Gizmos.color = Color.gray;

        // Vertical lines
        for (int x = 0; x <= gridSize; x++)
        {
            Vector3 start = new Vector3(x * tileSize, 0, 0);
            Vector3 end = new Vector3(x * tileSize, 0, gridSize * tileSize);
            Gizmos.DrawLine(start, end);
        }

        // Horizontal lines
        for (int z = 0; z <= gridSize; z++)
        {
            Vector3 start = new Vector3(0, 0, z * tileSize);
            Vector3 end = new Vector3(gridSize * tileSize, 0, z * tileSize);
            Gizmos.DrawLine(start, end);
        }
    }

    private void DrawKeyTiles()
    {
        if (!debugKeyTiles || activeKeyTiles == null) return;

        foreach (KeyTileInfo keyTile in activeKeyTiles)
        {
            Gizmos.color = keyTile.isVisited ? Color.green : Color.yellow;
            Vector3 position = keyTile.worldPosition + Vector3.up * 0.5f;
            Gizmos.DrawWireCube(position, Vector3.one);
        }
    }

    #endregion
}

// Data structures - Julian's simple style
[System.Serializable]
public class TilePrefabData
{
    public string tileName;
    public GameObject prefab;
    public Vector2Int size = Vector2Int.one;
    [Range(0, 100)] public int spawnWeight = 50;
}

[System.Serializable]
public class KeyTileInfo
{
    public GameObject tileObject;
    public Vector2Int gridPosition;
    public Vector3 worldPosition;
    public string tileType;
    public bool isVisited;
}

[System.Serializable]
public class TileGridCell
{
    public Vector3 worldPosition;
    public bool isOccupied;
    public bool isKeyTile;
    public GameObject occupyingTile;
}