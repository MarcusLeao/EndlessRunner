using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfinityRunner.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private const string MenuSceneName = "Menu";

        [Header("Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.25f;

        [Header("SFX")]
        [SerializeField] private AudioClip jumpSfx;
        [SerializeField] private AudioClip coinSfx;
        [SerializeField] private AudioClip gameOverSfx;
        [SerializeField] private AudioClip collisionSfx;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.8f;

        private AudioSource _musicSource;
        private AudioSource _sfxSource;
        private GameManager _subscribedGameManager;
        private GameState? _lastHandledState;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null)
                return;

            AudioManager audioPrefab = Resources.Load<AudioManager>("AudioManager");
            if (audioPrefab != null)
            {
                Instantiate(audioPrefab);
                return;
            }

            GameObject audioObject = new GameObject("AudioManager");
            audioObject.AddComponent<AudioManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = musicVolume;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = sfxVolume;

            LoadResourceClips();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            UnsubscribeFromGameManager();
        }

        private void Start()
        {
            RefreshSceneAudio();
        }

        public void PlayJump() => PlaySfx(jumpSfx);

        public void PlayCoin() => PlaySfx(coinSfx);

        public void PlayCollision() => PlaySfx(collisionSfx);

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshSceneAudio();
        }

        private void RefreshSceneAudio()
        {
            SubscribeToGameManager();

            if (GameManager.Instance != null)
            {
                HandleGameStateChanged(GameManager.Instance.CurrentState);
                return;
            }

            if (SceneManager.GetActiveScene().name == MenuSceneName)
            {
                PlayMusic(backgroundMusic);
            }
        }

        private void SubscribeToGameManager()
        {
            if (_subscribedGameManager == GameManager.Instance)
                return;

            UnsubscribeFromGameManager();

            _subscribedGameManager = GameManager.Instance;
            if (_subscribedGameManager != null)
                _subscribedGameManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void UnsubscribeFromGameManager()
        {
            if (_subscribedGameManager != null)
                _subscribedGameManager.OnGameStateChanged -= HandleGameStateChanged;

            _subscribedGameManager = null;
        }

        private void HandleGameStateChanged(GameState state)
        {
            bool enteredGameOver = state == GameState.GameOver && _lastHandledState != GameState.GameOver;
            _lastHandledState = state;

            switch (state)
            {
                case GameState.MainMenu:
                    PlayMusic(backgroundMusic);
                    break;
                case GameState.Playing:
                    PlayMusic(backgroundMusic);
                    break;
                case GameState.Paused:
                    if (_musicSource.isPlaying)
                        _musicSource.Pause();
                    break;
                case GameState.GameOver:
                    _musicSource.Stop();
                    if (enteredGameOver)
                        PlaySfx(gameOverSfx);
                    break;
            }
        }

        private void PlayMusic(AudioClip clip)
        {
            if (clip == null)
            {
                _musicSource.Stop();
                _musicSource.clip = null;
                return;
            }

            _musicSource.volume = musicVolume;

            if (_musicSource.clip == clip)
            {
                if (!_musicSource.isPlaying)
                    _musicSource.UnPause();

                return;
            }

            _musicSource.clip = clip;
            _musicSource.Play();
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip == null)
                return;

            _sfxSource.PlayOneShot(clip, sfxVolume);
        }

        private void LoadResourceClips()
        {
            backgroundMusic ??= Resources.Load<AudioClip>("Audio/Music/Background");

            jumpSfx ??= Resources.Load<AudioClip>("Audio/SFX/Jump");
            coinSfx ??= Resources.Load<AudioClip>("Audio/SFX/Coin");
            gameOverSfx ??= Resources.Load<AudioClip>("Audio/SFX/GameOver");
            collisionSfx ??= Resources.Load<AudioClip>("Audio/SFX/Collision");
        }
    }
}
