using TMPro;
using UnityEngine;
using InfinityRunner.Core;
using InfinityRunner.Systems;

namespace InfinityRunner.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Texts")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text distanceText;

        private void Reset()
        {
            root = gameObject;
        }

        private void Start()
        {
            if (root == null)
                root = gameObject;

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;

            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

            RefreshFromScoreManager();
            RefreshVisibility();
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;

            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleScoreChanged(int score, int coins, float distance)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";

            if (coinsText != null)
                coinsText.text = $"Coins: {coins}";

            if (distanceText != null)
                distanceText.text = $"Distance: {Mathf.FloorToInt(distance)}m";
        }

        private void HandleGameStateChanged(GameState state)
        {
            RefreshVisibility();
        }

        private void RefreshFromScoreManager()
        {
            if (ScoreManager.Instance == null)
                return;

            HandleScoreChanged(
                ScoreManager.Instance.CurrentScore,
                ScoreManager.Instance.CoinsCollected,
                ScoreManager.Instance.DistanceTravelled
            );
        }

        private void RefreshVisibility()
        {
            if (root == null)
                return;

            if (GameManager.Instance == null)
            {
                root.SetActive(true);
                return;
            }

            bool shouldShow = GameManager.Instance.CurrentState == GameState.Playing ||
                              GameManager.Instance.CurrentState == GameState.Paused;

            root.SetActive(shouldShow);
        }
    }
}