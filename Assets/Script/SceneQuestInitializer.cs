using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneQuestInitializer : MonoBehaviour
{
    [Header("Cài đặt Chuyển Scene")]
    [Tooltip("Tên Scene sẽ chuyển đến")]
    public string nextSceneName = "Map2";

    [Header("Thông báo UI")]
    public TextMeshProUGUI notificationText;
    private bool playerIsNear = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
            CheckAndNotifyPlayer();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
            // Xóa thông báo khi Player rời đi
            if (notificationText != null)
            {
                notificationText.text = "";
            }
        }
    }

    void Update()
    {
        // Chỉ kiểm tra khi Player đang ở gần cổng
        if (playerIsNear)
        {
            // Kiểm tra phím bấm (ví dụ: nhấn E để vào cổng)
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryTransitionScene();
            }
        }
    }

    private void CheckAndNotifyPlayer()
    {
        if (notificationText == null || EnemyKillCounter.Instance == null) return;

        if (EnemyKillCounter.Instance.IsQuestComplete)
        {
            notificationText.text = "Nhấn [E] để chuyển Scene.";
            notificationText.color = Color.green;
        }
        else
        {
            int current = EnemyKillCounter.Instance.CurrentKills;
            int required = EnemyKillCounter.Instance.RequiredKills;
            notificationText.text = $"Cần tiêu diệt thêm {required - current} Enemy để mở cổng. ({current}/{required})";
            notificationText.color = Color.red;
        }
    }

    public void TryTransitionScene()
    {
        // >>> ĐIỀU KIỆN QUAN TRỌNG NHẤT <<<
        if (EnemyKillCounter.Instance != null && EnemyKillCounter.Instance.IsQuestComplete)
        {
            Debug.Log($"Đã hoàn thành nhiệm vụ! Chuyển sang Scene: {nextSceneName}");
            // Reset thời gian để đảm bảo game không bị dừng
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // Nếu nhiệm vụ chưa hoàn thành, hiển thị lại thông báo (nếu có)
            CheckAndNotifyPlayer();
            Debug.Log("Chưa hoàn thành nhiệm vụ giết Enemy. Cổng vẫn khóa.");
        }
    }
}
