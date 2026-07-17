using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MahjongUI : MonoBehaviour
{
    [Header("Game Manager Reference")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private LevelManager levelManager;

    [Header("Canvases & Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayHudPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject stuckPanel;

    [Header("HUD Text Elements")]
    [SerializeField] private TextMeshProUGUI levelTitleText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI tilesRemainingText;

    [Header("Main Menu Text")]
    [SerializeField] private TextMeshProUGUI menuLevelText;
    [SerializeField] private TextMeshProUGUI menuDifficultyText;

    [Header("Action Buttons")]
    [SerializeField] private Button hintButton;
    [SerializeField] private Button shuffleButton;

    [Header("Win Screen")]
    [SerializeField] private TextMeshProUGUI winStatsText;
    [SerializeField] private TextMeshProUGUI winLevelText;

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (gameManager == null) return;

        bool isMainMenu = gameManager.CurrentState == GameManager.GameState.MainMenu;
        bool isPlaying = gameManager.CurrentState != GameManager.GameState.MainMenu;
        bool isWin = gameManager.CurrentState == GameManager.GameState.Win;
        bool isStuck = gameManager.CurrentState == GameManager.GameState.NoMoves;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(isMainMenu);
        if (gameplayHudPanel != null) gameplayHudPanel.SetActive(isPlaying);
        if (winPanel != null) winPanel.SetActive(isWin);
        if (stuckPanel != null) stuckPanel.SetActive(isStuck);


        if (isMainMenu && levelManager != null)
        {
            LevelConfig cfg = levelManager.CurrentConfig;
            if (menuLevelText != null) menuLevelText.text = $"Seviye {cfg.Level}";
            if (menuDifficultyText != null) menuDifficultyText.text = $"{cfg.Shape} | {cfg.Layers} Katman | ~{cfg.ApproxTileCount} Karo";
        }


        if (isPlaying)
        {
            if (levelTitleText != null) levelTitleText.text = gameManager.CurrentLayoutName;
            if (scoreText != null) scoreText.text = $"{gameManager.Score}";
            if (tilesRemainingText != null) tilesRemainingText.text = $"Kalan Karo: {gameManager.RemainingTiles}";

            if (hintButton != null) hintButton.interactable = true;
            if (shuffleButton != null) shuffleButton.interactable = true;
        }


        if (isWin)
        {
            int nextLevel = levelManager != null ? levelManager.CurrentLevel : 0;
            if (winLevelText != null) winLevelText.text = $"Seviye {nextLevel} Tamamlandı!";
            if (winStatsText != null)
            {
                winStatsText.text = $"Skor: {gameManager.Score}\n" +
                                    $"İpucu: {gameManager.UsedHints}\n" +
                                    $"Karıştırma: {gameManager.UsedShuffles}";
            }
        }
    }



    public void OnPlayButtonClicked() => gameManager.StartGame();

    public void OnNextLevelButtonClicked() => gameManager.NextLevel();

    public void OnRestartButtonClicked() => gameManager.RestartCurrentLevel();

    public void OnHintButtonClicked() => gameManager.TriggerHint();

    public void OnShuffleButtonClicked() => gameManager.TriggerShuffle();

    public void OnMainMenuButtonClicked() => gameManager.ShowMainMenu();
}
