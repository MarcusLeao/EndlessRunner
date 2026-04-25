using System;
using UnityEngine;
using InfinityRunner.Core;
using InfinityRunner.Data;

namespace InfinityRunner.Systems
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [SerializeField] private Transform player;
        [SerializeField] private RunConfig config;

        public float DistanceTravelled { get; private set; }
        public int CoinsCollected { get; private set; }
        public int CurrentScore { get; private set; }

        public event Action<int, int, float> OnScoreChanged;

        private float _lastPlayerZ;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (player != null)
                _lastPlayerZ = player.position.z;

            RecalculateScore();
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
                return;

            if (player == null || config == null)
                return;

            float deltaZ = player.position.z - _lastPlayerZ;
            if (deltaZ > 0f)
                DistanceTravelled += deltaZ;

            _lastPlayerZ = player.position.z;

            RecalculateScore();
        }

        public void AddCoins(int amount)
        {
            CoinsCollected += Mathf.Max(0, amount);
            RecalculateScore();
        }

        private void RecalculateScore()
        {
            if (config == null)
                return;

            int distanceScore = Mathf.FloorToInt(DistanceTravelled * config.scorePerMeter);
            int coinScore = CoinsCollected * config.scorePerCoin;

            CurrentScore = distanceScore + coinScore;
            OnScoreChanged?.Invoke(CurrentScore, CoinsCollected, DistanceTravelled);
        }
    }
}