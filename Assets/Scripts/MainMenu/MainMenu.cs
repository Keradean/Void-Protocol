using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Load the main game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
        Debug.Log("Tschüss gell ...");
    }
}
