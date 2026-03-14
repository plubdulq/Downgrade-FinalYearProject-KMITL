using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour
{
    [SerializeField] Image roomIcon;
    [SerializeField] TextMeshProUGUI roomSizeText;
    [SerializeField] TextMeshProUGUI roomScaleText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private string saveFileName;
    private string sceneName;
    private MainMenuManager menuManager;

    public void Setup(Sprite sprite, string roomSize, string roomScal, string fileName, string sceneToLoad, MainMenuManager manager = null)
    {
        if(roomIcon != null) roomIcon.sprite = sprite;
        if(roomSizeText != null) roomSizeText.text = roomSize;
        if(roomScaleText != null) roomScaleText.text = roomScal;
        saveFileName = fileName;
        sceneName = sceneToLoad;
        menuManager = manager;
    }

    public void OnClickLoadRoom()
    {
        if (string.IsNullOrEmpty(saveFileName)) return;

        // Store the save name to load in the next scene
        PlayerPrefs.SetString("PendingLoadSave", saveFileName);
        PlayerPrefs.SetString("LastLoadedSaveName", saveFileName); // For ScreenshotTrigger

        
        // Load the specific scene if known, otherwise default to PlayScene
        string targetScene = !string.IsNullOrEmpty(sceneName) ? sceneName : "PlayScene";
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
    }

    public void OnClickDeleteRoom()
    {
        if (string.IsNullOrEmpty(saveFileName)) return;

        if (menuManager != null)
        {
            menuManager.ShowConfirmDelete(saveFileName);
        }
        else
        {
            // Fallback if no manager assigned (shouldn't happen with new Setup)
            Debug.LogError("MainMenuManager reference missing in RoomButton.");
        }
    }
}
