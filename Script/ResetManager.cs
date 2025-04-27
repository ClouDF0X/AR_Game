using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetManager : MonoBehaviour
{
    public void ResetAllProgress()
    {
        Debug.Log("ResetAllProgress invoked");
        PlayerPrefs.DeleteKey("found_primary");
        PlayerPrefs.DeleteKey("found_secondary");
        PlayerPrefs.DeleteKey("summary_text");

        // Новий рядок — скидаємо прапорець першого запуску
        PlayerPrefs.DeleteKey("has_seen_intro");

        PlayerPrefs.Save();
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}