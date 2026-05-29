using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    private void Start()
    {
        // Ensure SettingsManager persists across scenes
        if (SettingsManager.Instance == null)
        {
            GameObject sm = new GameObject("SettingsManager");
            sm.AddComponent<SettingsManager>();
        }

        // Find the Canvas in the scene and attach SettingsUI to it
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null && canvas.GetComponent<SettingsUI>() == null)
        {
            canvas.gameObject.AddComponent<SettingsUI>();
        }

        // Connect Exit Game button (find by name in scene)
        GameObject exitBtnObj = GameObject.Find("Exit Game");
        if (exitBtnObj != null)
        {
            var exitBtn = exitBtnObj.GetComponent<UnityEngine.UI.Button>();
            if (exitBtn != null)
            {
                exitBtn.onClick.RemoveAllListeners();
                exitBtn.onClick.AddListener(() => Application.Quit());
                Debug.Log("Exit button connected!");
            }
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}