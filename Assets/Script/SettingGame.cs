using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingGame : MonoBehaviour
{
    public GameObject settingPanel;   // Panel hiển thị khi pause
    public Button settingButton;  // Nút cài đặt
    public Button continueButton;     // Nút tiếp tục
    public Button exitButton;       // Nút thoát

    private bool isPaused = false;

    void Start()
    {
        settingPanel.SetActive(false);

        // Gắn sự kiện cho các nút
        settingButton.onClick.AddListener(OpenSettingMenu);
        continueButton.onClick.AddListener(ContinueGame);
        exitButton.onClick.AddListener(ExitToMainMenu);
    }

    void OpenSettingMenu()
    {
        settingPanel.SetActive(true);
        Time.timeScale = 0f; // Dừng game
        isPaused = true;
    }

    void ContinueGame()
    {
        settingPanel.SetActive(false);
        Time.timeScale = 1f; // Tiếp tục game
        isPaused = false;
    }

    void ExitToMainMenu()
    {
        Time.timeScale = 1f; // Reset lại tốc độ game
        SceneManager.LoadScene("Menu Screen"); // đổi "MainMenu" thành tên scene chính của bạn
    }

}
