using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameplaySceneName = "GameScene";

    public void PlayGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}