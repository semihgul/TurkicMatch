using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState { MainMenu, Playing, Win, NoMoves }

    [Header("Referanslar")]
    [SerializeField] private MahjongBoard board;
    [SerializeField] private SlotManager slotManager;
    [SerializeField] private LevelManager levelManager;

    [Header("İstatistikler")]
    public int Score { get; private set; }
    public int RemainingTiles => board.GetActiveTiles().Count;
    public int UsedHints { get; private set; }
    public int UsedShuffles { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    public string CurrentLayoutName { get; private set; } = "";

    private Coroutine hintCoroutine = null;

    private void Awake()
    {
        // Disable VSync to allow custom frame rate target
        QualitySettings.vSyncCount = 0;

        // Query the device's native refresh rate if available, otherwise default to 60 FPS
        double nativeRefreshRate = Screen.currentResolution.refreshRateRatio.value;
        int targetFPS = nativeRefreshRate > 0 ? (int)System.Math.Round(nativeRefreshRate) : 60;

        // Force a minimum target of 60 FPS for smooth mobile animations
        if (targetFPS < 60)
        {
            targetFPS = 60;
        }

        Application.targetFrameRate = targetFPS;
    }

    private void Start()
    {
        if (slotManager != null)
        {
            slotManager.OnMatchCleared += OnSlotMatchCleared;
            slotManager.OnSlotsFullNoMatch += OnSlotsFull;
        }

        ShowMainMenu();
    }

    private void OnDestroy()
    {
        if (slotManager != null)
        {
            slotManager.OnMatchCleared -= OnSlotMatchCleared;
            slotManager.OnSlotsFullNoMatch -= OnSlotsFull;
        }
    }





    public void ShowMainMenu()
    {
        CurrentState = GameState.MainMenu;
        board.ClearBoard();
        slotManager?.ClearAll();
        FindFirstObjectByType<MahjongUI>()?.UpdateUI();
    }

    public void StartGame()
    {
        if (levelManager == null)
        {
            Debug.LogError("LevelManager atanmamış!");
            return;
        }


        Score = 0;
        UsedHints = 0;
        UsedShuffles = 0;

        if (hintCoroutine != null) { StopCoroutine(hintCoroutine); hintCoroutine = null; }

        slotManager?.ClearAll();
        board.ClearBoard();

        LevelConfig cfg = levelManager.CurrentConfig;
        CurrentLayoutName = cfg.ToString();

        LayoutData.Layout layout = levelManager.GetCurrentLayout();
        board.GenerateBoard(layout, cfg.Level);

        slotManager?.RefreshPositions();

        CurrentState = GameState.Playing;
        FindFirstObjectByType<MahjongUI>()?.UpdateUI();
    }

    public void NextLevel()
    {
        levelManager?.AdvanceLevel();
        StartGame();
    }

    public void RestartCurrentLevel() => StartGame();





    public void OnTileSelected(MahjongTile tile)
    {
        if (CurrentState != GameState.Playing) return;
        if (!tile.IsFree || tile.IsInSlot) return;
        if (slotManager == null) return;
        if (slotManager.IsFull) return;

        board.RemoveTile(tile);

        if (!slotManager.TryAddTile(tile))
        {
            board.AddTile(tile);
        }

        FindFirstObjectByType<MahjongUI>()?.UpdateUI();
    }





    private void OnSlotMatchCleared(int points)
    {
        Score += points;

        if (board.GetActiveTiles().Count == 0 && slotManager.FilledCount == 0)
        {
            CurrentState = GameState.Win;
            levelManager?.AdvanceLevel();
        }

        FindFirstObjectByType<MahjongUI>()?.UpdateUI();
    }

    private void OnSlotsFull()
    {
        CurrentState = GameState.NoMoves;
        FindFirstObjectByType<MahjongUI>()?.UpdateUI();
    }



    public void TriggerHint()
    {
        if (CurrentState != GameState.Playing) return;

        var match = board.FindHint();
        if (match.HasValue)
        {
            UsedHints++;
            if (hintCoroutine != null) StopCoroutine(hintCoroutine);
            hintCoroutine = StartCoroutine(FlashTilesCoroutine(match.Value.Key, match.Value.Value));
            FindFirstObjectByType<MahjongUI>()?.UpdateUI();
        }
    }

    private IEnumerator FlashTilesCoroutine(MahjongTile tile1, MahjongTile tile2)
    {
        float duration = 2.5f;
        float elapsed = 0f;
        Color flashColor = new Color(0.3f, 1f, 0.4f, 1f);

        SpriteRenderer sr1 = tile1?.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer sr2 = tile2?.GetComponentInChildren<SpriteRenderer>();

        while (elapsed < duration)
        {
            if (tile1 == null || tile2 == null) yield break;

            float pulse = Mathf.PingPong(Time.time * 6f, 1f);
            Color c = Color.Lerp(Color.white, flashColor, pulse);

            tile1.UpdateVisuals(); tile2.UpdateVisuals();
            if (sr1 != null) sr1.color = c;
            if (sr2 != null) sr2.color = c;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (tile1 != null) tile1.UpdateVisuals();
        if (tile2 != null) tile2.UpdateVisuals();
        hintCoroutine = null;
    }



    public void TriggerShuffle()
    {
        if (CurrentState != GameState.Playing) return;

        UsedShuffles++;
        board.ShuffleBoard();
        FindFirstObjectByType<MahjongUI>()?.UpdateUI();
    }
}
