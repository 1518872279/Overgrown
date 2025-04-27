using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverHandler : MonoBehaviour {
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    public Button mainMenuButton;

    void Awake() {
        if (gameOverPanel != null) {
            gameOverPanel.SetActive(false);
        }
    }

    public void TriggerGameOver(string reason) {
        // Stop time or put game in paused state
        Time.timeScale = 0;
        
        // Display game over UI
        if (gameOverPanel != null) {
            gameOverPanel.SetActive(true);
        }
        
        // Set reason text
        if (gameOverText != null) {
            gameOverText.text = "Game Over: " + reason;
        }
        
        // Optional: Play sound, animation, etc.
    }
    
    public void RestartGame() {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
    
    public void ReturnToMainMenu() {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Assuming main menu is scene 0
    }
} 