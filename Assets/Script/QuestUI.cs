using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    // [Header] và [Tooltip] đã được giữ nguyên
    [Header("UI References")]
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI questProgressText; // Text hiển thị tiến độ 1/3, 2/3...

    [Header("Cài đặt hiển thị")]
    public bool hideOnComplete = false;
    public float hideDelay = 3f;

    private void Start()
    {
        // Cập nhật UI ngay khi khởi động để hiện thị 0/3 ban đầu
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
    /// Cập nhật UI nhiệm vụ (Được gọi mỗi khi Enemy chết)
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

        // =======================================================
        // CẬP NHẬT MÔ TẢ NHIỆM VỤ
        // =======================================================
        if (questDescriptionText != null)
        {
            string status = isComplete ? "✓ Hoàn thành:" : "Nhiệm vụ:";
            questDescriptionText.text = $"{status} Tiêu diệt Enemy";
            questDescriptionText.color = isComplete ? Color.green : Color.white;
        }

        // =======================================================
        // CẬP NHẬT TIẾN ĐỘ (Phần bạn muốn hiển thị 1/3, 2/3)
        // =======================================================
        if (questProgressText != null)
        {
            if (isComplete)
            {
                // Khi hoàn thành: hiển thị "3/3 Đã Xong!"
                questProgressText.text = $"<color=green>{current}/{required} Đã Xong!</color>";
            }
            else
            {
                // Khi chưa hoàn thành: hiển thị "1/3 Enemy" với màu vàng
                questProgressText.text = $"<color=yellow>{current}/{required} Enemy</color>";
            }
        }
        // =======================================================

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
}

