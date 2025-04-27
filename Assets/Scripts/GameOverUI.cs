using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour {
    [Header("UI Elements")]
    public TextMeshProUGUI gameOverReasonText;
    public TextMeshProUGUI scoreText;
    public Button restartButton;
    public Button quitButton;
    
    void Start() {
        // Set up button listeners
        if (restartButton != null) {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        
        if (quitButton != null) {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
        
        // Make sure the panel starts disabled
        gameObject.SetActive(false);
    }
    
    void OnEnable() {
        // Update UI when panel becomes active
        UpdateGameOverUI();
    }
    
    void UpdateGameOverUI() {
        // Update reason text if available
        if (gameOverReasonText != null && GameManager.Instance != null) {
            string reason = "Game Over!";
            gameOverReasonText.text = reason;
        }
        
        // Update score text if available
        if (scoreText != null && XPManager.Instance != null) {
            scoreText.text = "Level: " + XPManager.Instance.level + 
                             "\nXP: " + XPManager.Instance.xp.ToString("F0");
        }
    }
    
    void OnRestartClicked() {
        if (GameManager.Instance != null) {
            GameManager.Instance.RestartGame();
        }
    }
    
    void OnQuitClicked() {
        if (GameManager.Instance != null) {
            GameManager.Instance.QuitGame();
        }
    }
} 