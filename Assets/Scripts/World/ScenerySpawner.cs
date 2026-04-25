using System.Collections.Generic;
using UnityEngine;
using InfinityRunner.Core;

namespace InfinityRunner.World
{
    public class ScenerySpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform initialAttachPoint;

        [Header("Chunk Prefabs")]
        [SerializeField] private List<SceneryChunk> chunkPrefabs = new();

        [Header("Spawn Settings")]
        [SerializeField] private int initialChunkCount = 5;
        [SerializeField] private float minDistanceAhead = 80f;
        [SerializeField] private float despawnDistanceBehindPlayer = 40f;

        [Header("Variation")]
        [SerializeField] private bool randomizeChunks = true;
        [SerializeField] private bool avoidImmediateRepeat = true;

        [Header("Safety")]
        [SerializeField] private int maxSpawnPerFrame = 5;
        [SerializeField] private bool stopSpawnerOnInvalidChunk = true;

        private readonly Queue<SceneryChunk> _activeChunks = new();

        private Transform _nextAttachPoint;
        private SceneryChunk _lastSpawnedChunk;
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

            MaintainChunksAhead();
            DespawnOldChunks();
        }

        public void Initialize()
        {
            ClearAllChunks();

            if (initialAttachPoint == null)
            {
                Debug.LogError($"{name}: InitialAttachPoint não configurado.");
                enabled = false;
                return;
            }

            if (player == null)
            {
                Debug.LogError($"{name}: Player não configurado.");
                enabled = false;
                return;
            }

            if (chunkPrefabs == null || chunkPrefabs.Count == 0)
            {
                Debug.LogError($"{name}: Nenhum SceneryChunk prefab configurado.");
                enabled = false;
                return;
            }

            for (int i = 0; i < chunkPrefabs.Count; i++)
            {
                if (chunkPrefabs[i] == null)
                {
                    Debug.LogError($"{name}: Chunk prefab no índice {i} está nulo.");
                    enabled = false;
                    return;
                }

                if (!chunkPrefabs[i].IsValid())
                {
                    Debug.LogError($"{name}: Chunk prefab '{chunkPrefabs[i].name}' está inválido.");
                    enabled = false;
                    return;
                }
            }

            _nextAttachPoint = initialAttachPoint;

            for (int i = 0; i < initialChunkCount; i++)
            {
                if (!TrySpawnNextChunk())
                {
                    Debug.LogError($"{name}: Falha ao spawnar chunks iniciais.");
                    enabled = false;
                    return;
                }
            }

            _initialized = true;
        }

        private void MaintainChunksAhead()
        {
            int spawnCountThisFrame = 0;

            while (DistanceAheadToLastChunkEnd() < minDistanceAhead)
            {
                float previousAttachZ = _nextAttachPoint != null ? _nextAttachPoint.position.z : float.MinValue;

                if (!TrySpawnNextChunk())
                {
                    Debug.LogError($"{name}: Falha ao spawnar chunk.");
                    if (stopSpawnerOnInvalidChunk)
                        enabled = false;
                    return;
                }

                float newAttachZ = _nextAttachPoint != null ? _nextAttachPoint.position.z : float.MinValue;

                if (newAttachZ <= previousAttachZ + 0.001f)
                {
                    Debug.LogError($"{name}: O novo chunk não avançou o attach point.");
                    if (stopSpawnerOnInvalidChunk)
                        enabled = false;
                    return;
                }

                spawnCountThisFrame++;

                if (spawnCountThisFrame >= maxSpawnPerFrame)
                {
                    Debug.LogWarning($"{name}: limite de spawn por frame atingido.");
                    break;
                }
            }
        }

        private void DespawnOldChunks()
        {
            while (_activeChunks.Count > 0)
            {
                SceneryChunk oldestChunk = _activeChunks.Peek();

                if (oldestChunk == null)
                {
                    _activeChunks.Dequeue();
                    continue;
                }

                float distanceBehind = player.position.z - oldestChunk.EndPoint.position.z;

                if (distanceBehind > despawnDistanceBehindPlayer)
                {
                    SceneryChunk chunkToRemove = _activeChunks.Dequeue();
                    Destroy(chunkToRemove.gameObject);
                }
                else
                {
                    break;
                }
            }
        }

        private bool TrySpawnNextChunk()
        {
            if (_nextAttachPoint == null)
            {
                Debug.LogError($"{name}: _nextAttachPoint é nulo.");
                return false;
            }

            int prefabIndex = GetNextPrefabIndex();
            SceneryChunk prefab = chunkPrefabs[prefabIndex];

            if (prefab == null || !prefab.IsValid())
            {
                Debug.LogError($"{name}: prefab inválido.");
                return false;
            }

            SceneryChunk spawnedChunk = Instantiate(prefab, transform);
            spawnedChunk.AlignTo(_nextAttachPoint);

            _activeChunks.Enqueue(spawnedChunk);
            _lastSpawnedChunk = spawnedChunk;
            _nextAttachPoint = spawnedChunk.EndPoint;
            _lastPrefabIndex = prefabIndex;

            return true;
        }

        private int GetNextPrefabIndex()
        {
            if (chunkPrefabs.Count == 1)
                return 0;

            if (!randomizeChunks)
                return 0;

            int nextIndex = Random.Range(0, chunkPrefabs.Count);

            if (!avoidImmediateRepeat)
                return nextIndex;

            int safety = 0;
            while (nextIndex == _lastPrefabIndex && safety < 10)
            {
                nextIndex = Random.Range(0, chunkPrefabs.Count);
                safety++;
            }

            return nextIndex;
        }

        private float DistanceAheadToLastChunkEnd()
        {
            if (_lastSpawnedChunk == null || player == null)
                return 0f;

            return _lastSpawnedChunk.EndPoint.position.z - player.position.z;
        }

        private void ClearAllChunks()
        {
            while (_activeChunks.Count > 0)
            {
                SceneryChunk chunk = _activeChunks.Dequeue();
                if (chunk != null)
                    Destroy(chunk.gameObject);
            }

            _lastSpawnedChunk = null;
            _lastPrefabIndex = -1;
            _initialized = false;
        }
    }
}