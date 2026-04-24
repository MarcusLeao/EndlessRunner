using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfinityRunner.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        public bool IsPlaying => CurrentState == GameState.Playing;
        public bool IsGameOver => CurrentState == GameState.GameOver;

        public event Action<GameState> OnGameStateChanged;

        [SerializeField] private bool autoStartRun = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Time.timeScale = 1f;
        }

        private void Start()
        {
            if (autoStartRun)
            {
                StartRun();
            }
            else
            {
                SetState(GameState.MainMenu);
            }
        }

        public void StartRun()
        {
            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        public void PauseRun()
        {
            if (CurrentState != GameState.Playing) return;

            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        public void ResumeRun()
        {
            if (CurrentState != GameState.Paused) return;

            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        public void GameOver()
        {
            if (CurrentState == GameState.GameOver) return;

            Time.timeScale = 1f;
            SetState(GameState.GameOver);
        }

        public void RestartRun()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void SetState(GameState newState)
        {
            CurrentState = newState;
            OnGameStateChanged?.Invoke(CurrentState);
        }
    }
}