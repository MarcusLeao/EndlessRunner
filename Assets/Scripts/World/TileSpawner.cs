using System.Collections.Generic;
using UnityEngine;
using InfinityRunner.Core;

namespace InfinityRunner.World
{
    public class TileSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform initialAttachPoint;

        [Header("Tile Prefabs")]
        [SerializeField] private List<TrackTile> tilePrefabs = new();

        [Header("Spawn Settings")]
        [SerializeField] private int initialTileCount = 6;
        [SerializeField] private float minDistanceAhead = 60f;
        [SerializeField] private float despawnDistanceBehindPlayer = 30f;

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
                    Debug.LogError($"TileSpawner: Tile prefab '{tilePrefabs[i].name}' está inválido. Verifique StartPoint e EndPoint.");
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
                    Debug.LogError("TileSpawner: O novo tile não avançou o attach point. Verifique StartPoint/EndPoint do prefab.");
                    if (stopSpawnerOnInvalidTile)
                        enabled = false;
                    return;
                }

                spawnCountThisFrame++;

                if (spawnCountThisFrame >= maxSpawnPerFrame)
                {
                    Debug.LogWarning("TileSpawner: limite de spawn por frame atingido. Ajuste minDistanceAhead ou os prefabs.");
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

            if (prefab == null)
            {
                Debug.LogError($"TileSpawner: prefab nulo no índice {prefabIndex}.");
                return false;
            }

            if (!prefab.IsValid())
            {
                Debug.LogError($"TileSpawner: prefab '{prefab.name}' inválido.");
                return false;
            }

            TrackTile spawnedTile = Instantiate(prefab, transform);
            spawnedTile.AlignTo(_nextAttachPoint);

            if (!spawnedTile.IsValid())
            {
                Debug.LogError($"TileSpawner: tile instanciado '{spawnedTile.name}' inválido.");
                Destroy(spawnedTile.gameObject);
                return false;
            }

            _activeTiles.Enqueue(spawnedTile);
            _lastSpawnedTile = spawnedTile;
            _nextAttachPoint = spawnedTile.EndPoint;
            _lastPrefabIndex = prefabIndex;

            return true;
        }

        private int GetNextPrefabIndex()
        {
            if (tilePrefabs.Count == 1)
                return 0;

            if (!randomizeTiles)
                return 0;

            int nextIndex = Random.Range(0, tilePrefabs.Count);

            if (!avoidImmediateRepeat)
                return nextIndex;

            int safety = 0;
            while (nextIndex == _lastPrefabIndex && safety < 10)
            {
                nextIndex = Random.Range(0, tilePrefabs.Count);
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
            _initialized = false;
        }
    }
}