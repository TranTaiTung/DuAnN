using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTrigger : MonoBehaviour
{
    [Header("Scene to load")]
    public string sceneName;

    [Header("Player spawn point in the new map")]
    public Vector2 spawnPosition;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("MAP TRIGGER HIT!");
            PlayerSpawnManager.nextSpawnPosition = spawnPosition;
            SceneManager.LoadScene(sceneName);
        }
    
    }
}