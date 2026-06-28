using UnityEngine;
using UnityEngine.SceneManagement;
using InfinityRunner.Core;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameplaySceneName = "Gameplay";

    public void PlayGame()
    {
        AudioManager.Instance?.PlayButton();
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
        AudioManager.Instance?.PlayButton();
        Debug.Log("Saindo do jogo...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
