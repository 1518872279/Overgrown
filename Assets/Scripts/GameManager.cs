using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    
    [Header("Game State")]
    public bool gameOver = false;
    public bool isPaused = false;
    
    [Header("UI References")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    
    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start() {
        // Ensure UI elements are in correct initial state
        if (gameOverPanel != null) {
            gameOverPanel.SetActive(false);
        }
        
        if (pausePanel != null) {
            pausePanel.SetActive(false);
        }
        
        // Reset game state
        gameOver = false;
        isPaused = false;
        Time.timeScale = 1.0f;
    }
    
    public void GameOver(string reason) {
        if (gameOver) return; // Prevent multiple calls
        
        gameOver = true;
        Debug.Log("Game Over: " + reason);
        
        // Stop all ivy growth
        IvyNode.PauseAllGrowth();
        
        // Show game over UI
        if (gameOverPanel != null) {
            gameOverPanel.SetActive(true);
        }
    }
    
    public void RestartGame() {
        // Reload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
    
    public void PauseGame() {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pausePanel != null) {
            pausePanel.SetActive(true);
        }
    }
    
    public void ResumeGame() {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pausePanel != null) {
            pausePanel.SetActive(false);
        }
    }
    
    public void QuitGame() {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
} 