using System;
using UnityEngine;
using InfinityRunner.Core;
using InfinityRunner.Data;

namespace InfinityRunner.Systems
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private const string BestScoreKey = "BestScore";

        [SerializeField] private Transform player;
        [SerializeField] private RunConfig config;

        public float DistanceTravelled { get; private set; }
        public int CoinsCollected { get; private set; }
        public int CurrentScore { get; private set; }
        public int BestScore { get; private set; }

        public event Action<int, int, float> OnScoreChanged;
        public event Action<int> OnBestScoreChanged;

        private float _lastPlayerZ;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        }

        private void Start()
        {
            if (player != null)
                _lastPlayerZ = player.position.z;

            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

            RecalculateScore();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
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

        private void HandleGameStateChanged(GameState state)
        {
            if (state != GameState.GameOver)
                return;

            SaveBestScoreIfNeeded();
        }

        private void SaveBestScoreIfNeeded()
        {
            if (CurrentScore <= BestScore)
                return;

            BestScore = CurrentScore;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();

            OnBestScoreChanged?.Invoke(BestScore);
        }
    }
}