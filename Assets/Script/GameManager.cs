using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI (assign in Inspector)")]
    [SerializeField] private GameObject winCanvas;
    [SerializeField] private TextMeshProUGUI hudScoreText;
    [SerializeField] private TextMeshProUGUI hudFiresText;

    [Header("Live HUD Timer (World Space VR)")]
    [SerializeField] private TextMeshProUGUI hudTimerText;

    [Header("Win UI (assign in Inspector)")]
    [SerializeField] private TextMeshProUGUI winFiresText;
    [SerializeField] private TextMeshProUGUI winTimeText;
    [SerializeField] private TextMeshProUGUI winFinalScoreText;

    [Header("Gameplay Rules (fixed)")]
    [Tooltip("Exit within this time (seconds) to earn the 10% exit bonus).")]
    [SerializeField] private float exitBonusTimeLimit = 600f;

    [Header("Optional / Legacy scoring")]
    [SerializeField] private int pointsPerFire = 100;

    [Header("User Manual (assign in Inspector)")]
    [SerializeField] private GameObject userManualPanel;

    // Internal state
    private int extinguishedFires = 0;
    private int currentScore = 0;
    private float startRealtime = 0f;
    private bool playerExited = false;
    private bool gameEnded = false;
    private bool manualOpen = false;
    private float pausedAt = 0f;
    private float finalTimeTaken = 0f;

    private readonly HashSet<Fire> expectedFires = new HashSet<Fire>();
    private readonly HashSet<Fire> activatedFires = new HashSet<Fire>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Reset time only for the valid instance
            Time.timeScale = 1f;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        EnsureHUDTimerExists();
    }

    void Start()
    {
        startRealtime = Time.realtimeSinceStartup;

        if (winCanvas != null) winCanvas.SetActive(false);
        if (userManualPanel != null) userManualPanel.SetActive(false);

        EnsureHUDTimerExists();
        UpdateHUD();
    }

    void Update()
    {
        // If the game is ended, STOP doing anything in this function.
        if (gameEnded || manualOpen) return;

        UpdateHUDTimer();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (manualOpen) CloseUserManual();
            else OpenUserManual();
        }
    }

    // ========== User Manual ==========
    public void OpenUserManual()
    {
        if (manualOpen) return;
        manualOpen = true;
        pausedAt = Time.realtimeSinceStartup;
        if (userManualPanel != null) userManualPanel.SetActive(true);
    }

    public void CloseUserManual()
    {
        if (!manualOpen) return;
        manualOpen = false;
        float pausedDuration = Time.realtimeSinceStartup - pausedAt;
        startRealtime += pausedDuration;
        if (userManualPanel != null) userManualPanel.SetActive(false);
    }

    // ========== Public API ==========
    public void RegisterExpectedFire(Fire f)
    {
        if (f == null || expectedFires.Contains(f)) return;
        expectedFires.Add(f);
        UpdateHUD();
    }

    public void RegisterActivatedFire(Fire f)
    {
        if (f == null) return;
        if (!expectedFires.Contains(f))
        {
            Debug.Log($"GameManager: Ignoring activation of unregistered fire '{f.name}'.");
            return;
        }

        if (activatedFires.Add(f))
        {
            Debug.Log($"GameManager: Fire '{f.name}' activated. Active fires = {activatedFires.Count}.");
            UpdateHUD();
        }
    }

    public void NotifyFireExtinguished(Fire f)
    {
        if (f == null || !expectedFires.Contains(f)) return;

        if (extinguishedFires < activatedFires.Count)
        {
            extinguishedFires++;
            currentScore += pointsPerFire;
            UpdateHUD();
            Debug.Log($"GameManager: Fire '{f.name}' extinguished. extinguishedFires={extinguishedFires}");
        }
    }

    // ========== HUD & timer ==========
    private void EnsureHUDTimerExists()
    {
        if (hudTimerText != null) return;

        GameObject existing = GameObject.Find("HUD_WorldCanvas");
        if (existing != null)
        {
            TextMeshProUGUI existingText = existing.GetComponentInChildren<TextMeshProUGUI>();
            if (existingText != null)
            {
                hudTimerText = existingText;
                return;
            }
        }

        Camera cam = Camera.main ?? GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
        if (cam == null)
        {
            foreach (Camera c in Camera.allCameras)
            {
                if (c != null && c.isActiveAndEnabled)
                {
                    cam = c;
                    break;
                }
            }
        }

        if (cam == null)
        {
            Debug.LogWarning("GameManager: No camera found for HUD.");
            return;
        }

        GameObject canvasGO = new GameObject("HUD_WorldCanvas");
        canvasGO.transform.SetParent(cam.transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;
        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(800f, 150f);
        canvasRT.localPosition = new Vector3(0.2f, 0.25f, 1.5f);
        canvasRT.localScale = Vector3.one * 0.0025f;

        GameObject textGO = new GameObject("HUD_TimerText");
        textGO.transform.SetParent(canvas.transform, false);
        hudTimerText = textGO.AddComponent<TextMeshProUGUI>();
        hudTimerText.fontSize = 30;
        hudTimerText.alignment = TextAlignmentOptions.MidlineRight;
        hudTimerText.color = Color.green;

        RectTransform txtRT = hudTimerText.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        hudTimerText.text = $"{FormatTime(0f)}  |  Exit in {FormatTime(exitBonusTimeLimit)} |  Fires: 0/0";
    }

    private void UpdateHUDTimer()
    {
        if (hudTimerText == null) return;

        // Use finalTimeTaken if the game is over, otherwise calculate live time
        float elapsed = gameEnded ? finalTimeTaken : (Time.realtimeSinceStartup - startRealtime);
        float remaining = Mathf.Max(0f, exitBonusTimeLimit - elapsed);

        hudTimerText.color = remaining > 0 ? Color.green : Color.red;

        hudTimerText.text =
            $"{FormatTime(elapsed)}  |  Exit in {FormatTime(remaining)} |  Fires: {extinguishedFires}/{activatedFires.Count}";
    }

    private string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{mins:00}:{secs:00}";
    }

    // ========== Win ==========

    public void PlayerExited()
    {
        if (gameEnded) return;

        // 1. Capture the frozen time
        finalTimeTaken = Time.realtimeSinceStartup - startRealtime;

        // 2. Set the player flag but DON'T set gameEnded yet
        playerExited = true;

        // 3. Trigger the Win sequence
        WinGame();
    }

    private void CheckWinCondition()
    {
        // This function is now redundant if you call WinGame() directly, 
        // but if you keep it, remove the "if (gameEnded) return" line.
        if (playerExited) WinGame();
    }

    private void WinGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        float timeTaken = finalTimeTaken;
        int denom = Mathf.Max(activatedFires.Count, 1);
        float perFirePercent = 90f / denom;
        float percentFromFires = extinguishedFires * perFirePercent;
        float exitBonusPercent = (timeTaken <= exitBonusTimeLimit) ? 10f : 0f;
        float totalPercent = Mathf.Clamp(percentFromFires + exitBonusPercent, 0f, 100f);

        if (winFiresText != null)
            winFiresText.text = $"Fires Extinguished: {extinguishedFires}/{activatedFires.Count}";
        if (winTimeText != null)
            winTimeText.text = $"Exit Bonus: {exitBonusPercent:F0}% | Time: {FormatTime(timeTaken)}";
        if (winFinalScoreText != null)
            winFinalScoreText.text = $"Completion: {totalPercent:F0}%";


        if (winCanvas != null)
        {
            winCanvas.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    private void UpdateHUD()
    {
        int denom = Mathf.Max(activatedFires.Count, 1);
        float perFirePercent = 90f / denom;
        float percentFromFires = extinguishedFires * perFirePercent;
        float displayPercent = Mathf.Clamp(percentFromFires, 0f, 100f);

        if (hudScoreText != null) hudScoreText.text = $"Completion: {displayPercent:F0}%";
        if (hudFiresText != null) hudFiresText.text = $"{extinguishedFires}/{activatedFires.Count} Fires";
    }

    public bool IsMissionComplete()
    {
        return extinguishedFires >= activatedFires.Count && activatedFires.Count > 0;
    }

    // ========== Buttons ==========
    //public void RestartGame()
    //{
    //    Time.timeScale = 1f;
    //    gameEnded = false;
    //    playerExited = false;
    //    Instance = null;
    //    Debug.Log("GameManager: Restarting game...");
    //    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    //}

    public void ExitGame()
    {
        Debug.Log("GameManager: Exiting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
