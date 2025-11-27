using UnityEngine;
using UnityEngine.SceneManagement;
public class RestartGame : MonoBehaviour
{
    public void RestartApplication()
    {
        // Reloads the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
