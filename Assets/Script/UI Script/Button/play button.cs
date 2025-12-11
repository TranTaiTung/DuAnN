using UnityEngine;
using UnityEngine.SceneManagement;

public class playbutton : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Map1");
        Time.timeScale = 1f;
    }
}
