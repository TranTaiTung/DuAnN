using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNameTrigger : MonoBehaviour
{
    [Header("Tên Scene cần chuyển đến")]
    [Tooltip("Nhập tên chính xác của Scene (ví dụ: 'Level 1', 'Map1', 'Menu Screen')")]
    public string targetSceneName;

    [Header("Điều kiện chuyển Scene")]
    [Tooltip("Bật nếu cần hoàn thành nhiệm vụ giết Enemy trước khi chuyển")]
    public bool requireQuestComplete = true;

    [Header("Cài đặt")]
    [Tooltip("Chỉ kích hoạt 1 lần (sau đó tự tắt)")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu đã trigger rồi và chỉ cho phép trigger 1 lần
        if (hasTriggered && triggerOnce)
            return;

        // Kiểm tra nếu là Player
        if (collision.CompareTag("Player"))
        {
            // Kiểm tra tên scene có hợp lệ không
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogWarning($"[SceneNameTrigger] Tên Scene chưa được đặt trên {gameObject.name}!");
                return;
            }

            // Kiểm tra điều kiện nhiệm vụ nếu được bật
            if (requireQuestComplete)
            {
                if (EnemyKillCounter.Instance == null)
                {
                    Debug.LogWarning($"[SceneNameTrigger] Không tìm thấy EnemyKillCounter! Vui lòng thêm GameObject có EnemyKillCounter vào scene.");
                    return;
                }

                if (!EnemyKillCounter.Instance.IsQuestComplete)
                {
                    int current = EnemyKillCounter.Instance.CurrentKills;
                    int required = EnemyKillCounter.Instance.RequiredKills;
                    Debug.Log($"[SceneNameTrigger] Chưa hoàn thành nhiệm vụ! Đã giết {current}/{required} Enemy. Hãy giết đủ {required} Enemy trước!");
                    return;
                }

                Debug.Log($"[SceneNameTrigger] ✓ Đã hoàn thành nhiệm vụ! Chuyển đến Scene: {targetSceneName}");
            }
            else
            {
                Debug.Log($"[SceneNameTrigger] Chuyển đến Scene: {targetSceneName}");
            }

            hasTriggered = true;

            // Chuyển scene
            SceneManager.LoadScene(targetSceneName);
        }
    }

    // Reset trigger nếu cần (có thể gọi từ code khác)
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}

