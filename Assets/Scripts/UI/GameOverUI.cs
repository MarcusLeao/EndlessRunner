using TMPro;
using UnityEngine;
using InfinityRunner.Core;
using InfinityRunner.Systems;

namespace InfinityRunner.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject panelRoot;

        [Header("Texts")]
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text bestScoreText;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text distanceText;

        [Header("Input")]
        [SerializeField] private bool allowKeyboardRestart = true;
        [SerializeField] private KeyCode restartKey = KeyCode.Return;

        private void Start()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Update()
        {
            if (!allowKeyboardRestart)
                return;

            if (GameManager.Instance == null || !GameManager.Instance.IsGameOver)
                return;

            if (Input.GetKeyDown(restartKey) || Input.GetKeyDown(KeyCode.Space))
            {
                RestartRun();
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                ShowGameOver();
            }
            else
            {
                HideGameOver();
            }
        }

        private void ShowGameOver()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (ScoreManager.Instance == null)
                return;

            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {ScoreManager.Instance.CurrentScore}";

            if (bestScoreText != null)
                bestScoreText.text = $"Best Score: {ScoreManager.Instance.BestScore}";

            if (coinsText != null)
                coinsText.text = $"Coins: {ScoreManager.Instance.CoinsCollected}";

            if (distanceText != null)
                distanceText.text = $"Distance: {Mathf.FloorToInt(ScoreManager.Instance.DistanceTravelled)}m";
        }

        private void HideGameOver()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        public void RestartRun()
        {
            GameManager.Instance?.RestartRun();
        }
    }
}