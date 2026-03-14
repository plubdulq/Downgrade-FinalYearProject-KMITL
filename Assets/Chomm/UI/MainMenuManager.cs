using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject roomButtonPrefab;
    public Transform contentTransform; // The Content object of the ScrollView
    public Sprite defaultIcon;

    [Header("Delete Confirmation")]
    public GameObject confirmDeletePanel;
    private string roomToDelete;

    void Start()
    {
        if(confirmDeletePanel) confirmDeletePanel.SetActive(false);
        RefreshRoomList();
    }

    public void RefreshRoomList()
    {
        // Clear existing
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        string path = Application.persistentDataPath;
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        string[] files = Directory.GetFiles(path, "*.json");

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);

            // Optional: Read file to get more info (date, room size, etc.)
            // For now, just use file name
            
            GameObject btnObj = Instantiate(roomButtonPrefab, contentTransform);
            RoomButton btn = btnObj.GetComponent<RoomButton>();
            if (btn != null)
            {
                // Parse file info if needed, e.g. "RoomName_Size_Scale.json" or read content
                // Here we just pass raw filename
                string displaySize = "Unknown";
                string displayScale = "1:1";
                string savedSceneName = "PlayScene";
                
                try 
                {
                    string json = File.ReadAllText(file);
                    GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                    displaySize = data.timestamp; 
                    if (!string.IsNullOrEmpty(data.sceneName)) savedSceneName = data.sceneName;
                }
                catch {}

                // Check for screenshot
                Sprite thumb = defaultIcon;
                string pngPath = Path.Combine(path, fileName + ".png");
                if (File.Exists(pngPath))
                {
                    byte[] bytes = File.ReadAllBytes(pngPath);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(bytes))
                    {
                        thumb = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    }
                }

                btn.Setup(thumb, fileName, displaySize, fileName, savedSceneName, this);
                
                // Add listener
                 Button uibtn = btn.GetComponent<Button>();
                 if(uibtn) 
                 {
                     uibtn.onClick.AddListener(btn.OnClickLoadRoom);
                 }
            }
        }
    }

    public void ShowConfirmDelete(string fileName)
    {
        roomToDelete = fileName;
        if(confirmDeletePanel) confirmDeletePanel.SetActive(true);
    }

    public void ConfirmDelete()
    {
        if(string.IsNullOrEmpty(roomToDelete)) return;

        string path = Path.Combine(Application.persistentDataPath, roomToDelete + ".json");
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
                Debug.Log($"Deleted room save: {path}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to delete room save: {e.Message}");
            }
        }

        // Close panel and refresh list
        if(confirmDeletePanel) confirmDeletePanel.SetActive(false);
        roomToDelete = "";
        RefreshRoomList();
    }

    public void CancelDelete()
    {
        if(confirmDeletePanel) confirmDeletePanel.SetActive(false);
        roomToDelete = "";
    }
}
