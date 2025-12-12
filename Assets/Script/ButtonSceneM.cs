using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonSceneM : MonoBehaviour
{
    public void LoadMenuScene()
    {
        SceneManager.LoadScene("Menu Screen");
        // Lưu ý: tên Scene phải đúng chính tả với tên trong Build Settings
    }

}
