using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfinityRunner.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private const string MenuSceneName = "Menu";

        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip gameOverMusic;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.25f;

        [Header("SFX")]
        [SerializeField] private AudioClip coinSfx;
        [SerializeField] private AudioClip collisionSfx;
        [SerializeField] private AudioClip buttonSfx;
        [SerializeField] private AudioClip jumpSfx;
        [SerializeField] private AudioClip slideSfx;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.8f;
        [SerializeField] private bool useGeneratedFallbacks = true;

        private AudioSource _musicSource;
        private AudioSource _sfxSource;
        private GameManager _subscribedGameManager;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null)
                return;

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
            CreateFallbackClips();
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

        public void PlayCoin() => PlaySfx(coinSfx);

        public void PlayCollision() => PlaySfx(collisionSfx);

        public void PlayButton() => PlaySfx(buttonSfx);

        public void PlayJump() => PlaySfx(jumpSfx);

        public void PlaySlide() => PlaySfx(slideSfx);

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
                PlayMusic(menuMusic);
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
            switch (state)
            {
                case GameState.MainMenu:
                    PlayMusic(menuMusic);
                    break;
                case GameState.Playing:
                    PlayMusic(gameplayMusic);
                    break;
                case GameState.Paused:
                    if (_musicSource.isPlaying)
                        _musicSource.Pause();
                    break;
                case GameState.GameOver:
                    PlayMusic(gameOverMusic != null ? gameOverMusic : gameplayMusic);
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
            menuMusic ??= Resources.Load<AudioClip>("Audio/Music/Menu");
            gameplayMusic ??= Resources.Load<AudioClip>("Audio/Music/Gameplay");
            gameOverMusic ??= Resources.Load<AudioClip>("Audio/Music/GameOver");

            coinSfx ??= Resources.Load<AudioClip>("Audio/SFX/Coin");
            collisionSfx ??= Resources.Load<AudioClip>("Audio/SFX/Collision");
            buttonSfx ??= Resources.Load<AudioClip>("Audio/SFX/Button");
            jumpSfx ??= Resources.Load<AudioClip>("Audio/SFX/Jump");
            slideSfx ??= Resources.Load<AudioClip>("Audio/SFX/Slide");
        }

        private void CreateFallbackClips()
        {
            if (!useGeneratedFallbacks)
                return;

            gameplayMusic ??= CreateLoop("Generated Gameplay Music", 12f, 0.08f, 196f, 247f, 294f);
            menuMusic ??= CreateLoop("Generated Menu Music", 10f, 0.06f, 174.61f, 220f, 261.63f);
            gameOverMusic ??= CreateLoop("Generated Game Over Music", 6f, 0.05f, 146.83f, 174.61f, 220f);

            coinSfx ??= CreateTone("Generated Coin SFX", 0.14f, 0.45f, 880f, 1320f);
            collisionSfx ??= CreateTone("Generated Collision SFX", 0.24f, 0.55f, 110f, 70f);
            buttonSfx ??= CreateTone("Generated Button SFX", 0.08f, 0.35f, 440f, 660f);
            jumpSfx ??= CreateTone("Generated Jump SFX", 0.12f, 0.35f, 520f, 780f);
            slideSfx ??= CreateTone("Generated Slide SFX", 0.16f, 0.35f, 240f, 150f);
        }

        private static AudioClip CreateTone(string name, float duration, float amplitude, float startFrequency, float endFrequency)
        {
            int sampleRate = AudioSettings.outputSampleRate;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
                float envelope = Mathf.Sin(Mathf.PI * t);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * amplitude * envelope;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateLoop(string name, float duration, float amplitude, params float[] frequencies)
        {
            int sampleRate = AudioSettings.outputSampleRate;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float seconds = i / (float)sampleRate;
                float value = 0f;

                for (int f = 0; f < frequencies.Length; f++)
                {
                    value += Mathf.Sin(2f * Mathf.PI * frequencies[f] * seconds) / frequencies.Length;
                }

                float pulse = 0.65f + 0.35f * Mathf.Sin(2f * Mathf.PI * seconds / 1.5f);
                samples[i] = value * amplitude * pulse;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
