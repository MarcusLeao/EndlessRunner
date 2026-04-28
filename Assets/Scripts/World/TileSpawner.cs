using System;
using System.Collections.Generic;
using UnityEngine;
using InfinityRunner.Core;
using InfinityRunner.Obstacles;
using InfinityRunner.Collectibles;

namespace InfinityRunner.World
{
    public enum ObstacleRoute
    {
        Left,
        Center,
        Right
    }

    [Serializable]
    public class ObstacleRoutePool
    {
        public ObstacleRoute route;
        [Range(0f, 1f)] public float chancePerSocket = 0.35f;
        public List<Obstacle> obstaclePrefabs = new();
    }

    public class TileSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform initialAttachPoint;

        [Header("Tile Prefabs")]
        [SerializeField] private List<TrackTile> tilePrefabs = new();

        [Header("Obstacle Pools By Route")]
        [SerializeField] private List<ObstacleRoutePool> obstaclePoolsByRoute = new();

        [Header("Collectible Prefabs")]
        [SerializeField] private List<Collectible> collectiblePrefabs = new();

        [Header("Spawn Settings")]
        [SerializeField] private int initialTileCount = 6;
        [SerializeField] private float minDistanceAhead = 60f;
        [SerializeField] private float despawnDistanceBehindPlayer = 30f;

        [Header("Content Spawn")]
        [SerializeField] private int safeStartTileCount = 2;
        [SerializeField, Range(0f, 1f)] private float collectibleChancePerSocket = 0.6f;

        [Header("Safety")]
        [SerializeField] private int maxSpawnPerFrame = 5;
        [SerializeField] private bool stopSpawnerOnInvalidTile = true;

        [Header("Variation")]
        [SerializeField] private bool randomizeTiles = true;
        [SerializeField] private bool avoidImmediateRepeat = true;

        private readonly Queue<TrackTile> _activeTiles = new();

        private Transform _nextAttachPoint;
        private TrackTile _lastSpawnedTile;
        private int _lastPrefabIndex = -1;
        private int _spawnedTileCount;
        private bool _initialized;

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (!_initialized)
                return;

            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
                return;

            MaintainTilesAhead();
            DespawnOldTiles();
        }

        public void Initialize()
        {
            ClearAllTiles();

            if (initialAttachPoint == null)
            {
                Debug.LogError("TileSpawner: InitialAttachPoint não configurado.");
                enabled = false;
                return;
            }

            if (player == null)
            {
                Debug.LogError("TileSpawner: Player não configurado.");
                enabled = false;
                return;
            }

            if (tilePrefabs == null || tilePrefabs.Count == 0)
            {
                Debug.LogError("TileSpawner: Nenhum tile prefab configurado.");
                enabled = false;
                return;
            }

            for (int i = 0; i < tilePrefabs.Count; i++)
            {
                if (tilePrefabs[i] == null)
                {
                    Debug.LogError($"TileSpawner: Tile prefab no índice {i} está nulo.");
                    enabled = false;
                    return;
                }

                if (!tilePrefabs[i].IsValid())
                {
                    Debug.LogError($"TileSpawner: Tile prefab '{tilePrefabs[i].name}' está inválido.");
                    enabled = false;
                    return;
                }
            }

            _nextAttachPoint = initialAttachPoint;

            for (int i = 0; i < initialTileCount; i++)
            {
                if (!TrySpawnNextTile())
                {
                    Debug.LogError("TileSpawner: Falha ao spawnar tiles iniciais.");
                    enabled = false;
                    return;
                }
            }

            _initialized = true;
        }

        private void MaintainTilesAhead()
        {
            int spawnCountThisFrame = 0;

            while (DistanceAheadToLastTileEnd() < minDistanceAhead)
            {
                float previousAttachZ = _nextAttachPoint != null ? _nextAttachPoint.position.z : float.MinValue;

                if (!TrySpawnNextTile())
                {
                    Debug.LogError("TileSpawner: Falha no spawn durante MaintainTilesAhead.");
                    if (stopSpawnerOnInvalidTile)
                        enabled = false;
                    return;
                }

                float newAttachZ = _nextAttachPoint != null ? _nextAttachPoint.position.z : float.MinValue;

                if (newAttachZ <= previousAttachZ + 0.001f)
                {
                    Debug.LogError("TileSpawner: O novo tile não avançou o attach point.");
                    if (stopSpawnerOnInvalidTile)
                        enabled = false;
                    return;
                }

                spawnCountThisFrame++;

                if (spawnCountThisFrame >= maxSpawnPerFrame)
                {
                    Debug.LogWarning("TileSpawner: limite de spawn por frame atingido.");
                    break;
                }
            }
        }

        private void DespawnOldTiles()
        {
            while (_activeTiles.Count > 0)
            {
                TrackTile oldestTile = _activeTiles.Peek();
                if (oldestTile == null)
                {
                    _activeTiles.Dequeue();
                    continue;
                }

                float distanceBehind = player.position.z - oldestTile.EndPoint.position.z;

                if (distanceBehind > despawnDistanceBehindPlayer)
                {
                    TrackTile tileToRemove = _activeTiles.Dequeue();
                    Destroy(tileToRemove.gameObject);
                }
                else
                {
                    break;
                }
            }
        }

        private bool TrySpawnNextTile()
        {
            if (_nextAttachPoint == null)
            {
                Debug.LogError("TileSpawner: _nextAttachPoint é nulo.");
                return false;
            }

            int prefabIndex = GetNextPrefabIndex();
            TrackTile prefab = tilePrefabs[prefabIndex];

            if (prefab == null || !prefab.IsValid())
            {
                Debug.LogError("TileSpawner: prefab inválido.");
                return false;
            }

            TrackTile spawnedTile = Instantiate(prefab, transform);
            spawnedTile.AlignTo(_nextAttachPoint);

            _activeTiles.Enqueue(spawnedTile);
            _lastSpawnedTile = spawnedTile;
            _nextAttachPoint = spawnedTile.EndPoint;
            _lastPrefabIndex = prefabIndex;

            bool shouldPopulateContent = _spawnedTileCount >= safeStartTileCount;
            if (shouldPopulateContent)
                PopulateTile(spawnedTile);

            _spawnedTileCount++;
            return true;
        }

        private void PopulateTile(TrackTile tile)
        {
            if (tile == null)
                return;

            PopulateObstacles(tile);
            PopulateCollectibles(tile);
        }

        private void PopulateObstacles(TrackTile tile)
        {
            foreach (Transform socket in tile.ObstacleSockets)
            {
                if (socket == null)
                    continue;

                ObstacleRoute route = ResolveObstacleRoute(socket);
                ObstacleRoutePool pool = GetObstaclePool(route);

                if (pool == null)
                    continue;

                if (pool.obstaclePrefabs == null || pool.obstaclePrefabs.Count == 0)
                    continue;

                if (UnityEngine.Random.value > pool.chancePerSocket)
                    continue;

                Obstacle prefab = GetRandomObstacle(pool.obstaclePrefabs);
                if (prefab == null)
                    continue;

                Instantiate(prefab, socket.position, socket.rotation, tile.transform);
            }
        }

        private void PopulateCollectibles(TrackTile tile)
        {
            foreach (Transform socket in tile.CollectibleSockets)
            {
                if (socket == null)
                    continue;

                if (UnityEngine.Random.value > collectibleChancePerSocket)
                    continue;

                if (collectiblePrefabs == null || collectiblePrefabs.Count == 0)
                    continue;

                Collectible prefab = collectiblePrefabs[UnityEngine.Random.Range(0, collectiblePrefabs.Count)];
                if (prefab == null)
                    continue;

                Instantiate(prefab, socket.position, socket.rotation, tile.transform);
            }
        }

        private ObstacleRoutePool GetObstaclePool(ObstacleRoute route)
        {
            if (obstaclePoolsByRoute == null || obstaclePoolsByRoute.Count == 0)
                return null;

            for (int i = 0; i < obstaclePoolsByRoute.Count; i++)
            {
                if (obstaclePoolsByRoute[i] != null && obstaclePoolsByRoute[i].route == route)
                    return obstaclePoolsByRoute[i];
            }

            return null;
        }

        private Obstacle GetRandomObstacle(List<Obstacle> prefabs)
        {
            if (prefabs == null || prefabs.Count == 0)
                return null;

            List<Obstacle> validPrefabs = new();

            for (int i = 0; i < prefabs.Count; i++)
            {
                if (prefabs[i] != null)
                    validPrefabs.Add(prefabs[i]);
            }

            if (validPrefabs.Count == 0)
                return null;

            int index = UnityEngine.Random.Range(0, validPrefabs.Count);
            return validPrefabs[index];
        }

        private ObstacleRoute ResolveObstacleRoute(Transform socket)
        {
            if (socket == null)
                return ObstacleRoute.Center;

            string socketName = socket.name.ToLowerInvariant();

            if (socketName.Contains("left") || socketName.Contains("esquerda"))
                return ObstacleRoute.Left;

            if (socketName.Contains("right") || socketName.Contains("direita"))
                return ObstacleRoute.Right;

            if (socketName.Contains("center") || socketName.Contains("centre") || socketName.Contains("centro") || socketName.Contains("middle"))
                return ObstacleRoute.Center;

            float localX = socket.localPosition.x;

            if (localX < -0.1f)
                return ObstacleRoute.Left;

            if (localX > 0.1f)
                return ObstacleRoute.Right;

            return ObstacleRoute.Center;
        }

        private int GetNextPrefabIndex()
        {
            if (tilePrefabs.Count == 1)
                return 0;

            if (!randomizeTiles)
                return 0;

            int nextIndex = UnityEngine.Random.Range(0, tilePrefabs.Count);

            if (!avoidImmediateRepeat)
                return nextIndex;

            int safety = 0;
            while (nextIndex == _lastPrefabIndex && safety < 10)
            {
                nextIndex = UnityEngine.Random.Range(0, tilePrefabs.Count);
                safety++;
            }

            return nextIndex;
        }

        private float DistanceAheadToLastTileEnd()
        {
            if (_lastSpawnedTile == null || player == null)
                return 0f;

            return _lastSpawnedTile.EndPoint.position.z - player.position.z;
        }

        private void ClearAllTiles()
        {
            while (_activeTiles.Count > 0)
            {
                TrackTile tile = _activeTiles.Dequeue();
                if (tile != null)
                    Destroy(tile.gameObject);
            }

            _lastSpawnedTile = null;
            _lastPrefabIndex = -1;
            _spawnedTileCount = 0;
            _initialized = false;
        }
    }
}