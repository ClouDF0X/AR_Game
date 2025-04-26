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
        PlayerPrefs.Save();
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}