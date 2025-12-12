using System;
using UnityEngine;

public class EnemyKillCounter : MonoBehaviour
{
    public static EnemyKillCounter Instance { get; private set; }

    [Header("Nhiệm vụ")]
    [Tooltip("Số Enemy cần giết để hoàn thành nhiệm vụ")]
    public int requiredKills = 3;

    private int currentKills = 0;

    public int CurrentKills => currentKills;
    public int RequiredKills => requiredKills;
    public bool IsQuestComplete => currentKills >= requiredKills;

    // Event để thông báo khi số kill thay đổi
    public event Action OnKillCountChanged;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Reset counter khi bắt đầu scene mới (nếu cần)
        // currentKills = 0;
    }

    // Tự động cập nhật UI khi thay đổi giá trị trong Inspector
    private void OnValidate()
    {
        if (Application.isPlaying && Instance == this)
        {
            OnKillCountChanged?.Invoke();
        }
    }

    /// <summary>
    /// Tăng số Enemy đã giết (gọi từ Enemy khi chết)
    /// </summary>
    public void AddKill()
    {
        currentKills++;
        Debug.Log($"[EnemyKillCounter] Đã giết {currentKills}/{requiredKills} Enemy");

        // Thông báo cho UI cập nhật
        OnKillCountChanged?.Invoke();

        if (IsQuestComplete)
        {
            Debug.Log($"[EnemyKillCounter] ✓ Hoàn thành nhiệm vụ! Đã giết đủ {requiredKills} Enemy!");
        }
    }

    /// <summary>
    /// Reset counter về 0
    /// </summary>
    public void ResetKills()
    {
        currentKills = 0;
        OnKillCountChanged?.Invoke(); // Thông báo cho UI cập nhật
        Debug.Log("[EnemyKillCounter] Đã reset counter về 0");
    }

    /// <summary>
    /// Đặt số Enemy cần giết (tự động cập nhật UI)
    /// </summary>
    public void SetRequiredKills(int required)
    {
        if (required < 0) required = 0; // Đảm bảo không âm
        requiredKills = required;
        OnKillCountChanged?.Invoke(); // Cập nhật UI
        Debug.Log($"[EnemyKillCounter] Đã đặt số Enemy cần giết: {requiredKills}");
    }

    /// <summary>
    /// Tăng số Enemy cần giết
    /// </summary>
    public void AddRequiredKills(int amount)
    {
        SetRequiredKills(requiredKills + amount);
    }

    /// <summary>
    /// Giảm số Enemy cần giết
    /// </summary>
    public void SubtractRequiredKills(int amount)
    {
        SetRequiredKills(requiredKills - amount);
    }

    /// <summary>
    /// Reset cả counter và required kills về giá trị ban đầu
    /// </summary>
    public void ResetQuest(int newRequiredKills = 3)
    {
        currentKills = 0;
        requiredKills = newRequiredKills;
        OnKillCountChanged?.Invoke();
        Debug.Log($"[EnemyKillCounter] Đã reset nhiệm vụ: 0/{requiredKills}");
    }
    public void SetQuestForNewScene(int requiredKillsForThisScene)
    {
        // 1. Reset số kill hiện tại về 0
        currentKills = 0;

        // 2. Thiết lập số kill cần thiết mới
        requiredKills = requiredKillsForThisScene;

        Debug.Log($"[EnemyKillCounter] Nhiệm vụ Scene mới: Yêu cầu {requiredKillsForThisScene} kills.");

        // 3. Thông báo cập nhật UI (sẽ hiện 0/RequiredKills)
        OnKillCountChanged?.Invoke();
    }
}

