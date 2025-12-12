using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text hiển thị mô tả nhiệm vụ")]
    public TextMeshProUGUI questDescriptionText;
    
    [Tooltip("Text hiển thị tiến độ (ví dụ: '2/3 Enemy')")]
    public TextMeshProUGUI questProgressText;

    [Header("Cài đặt hiển thị")]
    [Tooltip("Tự động ẩn UI khi hoàn thành nhiệm vụ")]
    public bool hideOnComplete = false;
    
    [Tooltip("Thời gian delay trước khi ẩn UI (giây)")]
    public float hideDelay = 3f;

    private void Start()
    {
        UpdateQuestUI();
    }

    private void OnEnable()
    {
        // Đăng ký để nhận thông báo khi Enemy chết
        if (EnemyKillCounter.Instance != null)
        {
            EnemyKillCounter.Instance.OnKillCountChanged += UpdateQuestUI;
        }
        UpdateQuestUI();
    }

    private void OnDisable()
    {
        // Hủy đăng ký
        if (EnemyKillCounter.Instance != null)
        {
            EnemyKillCounter.Instance.OnKillCountChanged -= UpdateQuestUI;
        }
    }

    /// <summary>
    /// Cập nhật UI nhiệm vụ
    /// </summary>
    public void UpdateQuestUI()
    {
        if (EnemyKillCounter.Instance == null)
        {
            if (questDescriptionText != null)
                questDescriptionText.text = "Không tìm thấy nhiệm vụ";
            if (questProgressText != null)
                questProgressText.text = "";
            return;
        }

        int current = EnemyKillCounter.Instance.CurrentKills;
        int required = EnemyKillCounter.Instance.RequiredKills;
        bool isComplete = EnemyKillCounter.Instance.IsQuestComplete;

        // Cập nhật mô tả nhiệm vụ
        if (questDescriptionText != null)
        {
            if (isComplete)
            {
                questDescriptionText.text = "✓ Nhiệm vụ: Giết Enemy";
                questDescriptionText.color = Color.green;
            }
            else
            {
                questDescriptionText.text = "Nhiệm vụ: Giết Enemy";
                questDescriptionText.color = Color.white;
            }
        }

        // Cập nhật tiến độ
        if (questProgressText != null)
        {
            if (isComplete)
            {
                questProgressText.text = $"✓ Hoàn thành! ({current}/{required})";
                questProgressText.color = Color.green;
            }
            else
            {
                questProgressText.text = $"Tiến độ: {current}/{required} Enemy";
                questProgressText.color = Color.yellow;
            }
        }

        // Tự động ẩn UI khi hoàn thành
        if (isComplete && hideOnComplete)
        {
            Invoke(nameof(HideUI), hideDelay);
        }
    }

    private void HideUI()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Hiển thị lại UI (có thể gọi từ code khác)
    /// </summary>
    public void ShowUI()
    {
        gameObject.SetActive(true);
        UpdateQuestUI();
    }
}

