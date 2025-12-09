using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public static Vector2 nextSpawnPosition = Vector2.zero;
    public static bool hasSpawn = false;

    private void Start()
    {
        // If a spawn point has been set, move the player
        if (hasSpawn)
        {
            transform.position = nextSpawnPosition;
            hasSpawn = false;
        }
    }

    private void OnEnable()
    {
        // This ensures Start() runs with updated spawn data
        if (nextSpawnPosition != Vector2.zero)
            hasSpawn = true;
    }
}
