using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class RoomCreatorManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject templatePanel;
    public GameObject customRoomPanel;

    [Header("Template UI")]
    public Transform templateContent;
    public GameObject templateButtonPrefab;
    public List<RoomTemplate> availableTemplates;

    [Header("Custom Room UI")]
    public TMP_InputField widthInput;
    public TMP_InputField lengthInput;
    public TMP_InputField heightInput;
    public string builderSceneName = "PlayScene"; // Default Empty Room Scene
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] Transform container;
    MainMenuUI mainMenuUI;
    public List<RoomSizePreset> roomPresets;

    [Header("Dropdown UI")]
    public TMP_Dropdown xDropdown;
    public TMP_Dropdown yDropdown;

    [Header("Room Name UI")]
    public GameObject roomNamePanel;
    public InputField roomNameInput;

    private float pendingWidth;
    private float pendingLength;
    private float pendingHeight;
    private string pendingSceneName;

    private void Start()
    {
        mainMenuUI = GetComponent<MainMenuUI>();
        LoadTemplates();
        if(roomNamePanel) roomNamePanel.SetActive(false);
    }

    public void ShowTemplatePanel()
    {
        mainMenuUI.ActiveUI("readytouse");
    }

    public void ShowCustomRoomPanel()
    {
        mainMenuUI.ActiveUI("custome");
    }

    private void LoadTemplates()
    {
        foreach (Transform child in templateContent) 
        {
            Destroy(child.gameObject);
        }

        foreach (var template in availableTemplates)
        {
            GameObject btnObj = Instantiate(templateButtonPrefab, templateContent);
            RoomButton btn = btnObj.GetComponent<RoomButton>();
            if (btn != null)
            {
                // Template buttons don't need a filename to load from disk, just display info
                btn.Setup(template.thumbnail, template.templateName, template.description, "", template.sceneName);
                
                Button b = btn.GetComponent<Button>();
                if(b)
                {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(() => OnClickTemplate(template));
                }
            }
        }
    }
    public bool isOnValidate = false;
    private void OnValidate()
    {
        if (!isOnValidate)
        {
            return;
        }
        for (int i = 0; i < roomPresets.Count; i++)
        {
            GameObject go = Instantiate(buttonPrefab, container);
            RoomSizePreset rs = roomPresets[i];
            string g = $"{rs.width}x{rs.length}x{rs.height}";
            TextMeshProUGUI text = go.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            if (text != null)
                text.text = g;
            LoadPresetScript sc = go.GetComponent<LoadPresetScript>();
            if (sc != null)
            {
                sc.creatorManager = this;
                sc.presetIndex = i;
            }
        }
    }

    private void OnClickTemplate(RoomTemplate template)
    {
        // Load the specific scene for this template
        SceneManager.LoadScene(template.sceneName);
    }

    public void OnClickCreateCustomRoom()
    {
        float w = 10f;
        float l = 10f;
        float h = 3f; // Default height

        float.TryParse(widthInput.text, out w);
        float.TryParse(lengthInput.text, out l);
        if(heightInput != null) float.TryParse(heightInput.text, out h);

        // Store and show name panel
        pendingWidth = w;
        pendingLength = l;
        pendingHeight = h;
        pendingSceneName = builderSceneName;

        if (roomNamePanel)
        {
            roomNamePanel.SetActive(true);
            if(roomNameInput) roomNameInput.text = ""; // Clear previous
        }
        else
        {
            // Fallback if no panel assigned
            CreateRoom(w, l, h);
        }
    }

    [System.Serializable]
    public class RoomSizePreset
    {
        public string presetName;
        public float width;
        public float length;
        public float height;
    }


    public void OnClickCreateCustomRoomByPreset(int presetIndex)
    {
        if (roomPresets != null && presetIndex >= 0 && presetIndex < roomPresets.Count)
        {
            RoomSizePreset p = roomPresets[presetIndex];
            
            // Store and show name panel
            pendingWidth = p.width;
            pendingLength = p.length;
            pendingHeight = p.height;
            pendingSceneName = builderSceneName;

            if (roomNamePanel)
            {
                roomNamePanel.SetActive(true);
                if (roomNameInput) roomNameInput.text = ""; // Clear previous
            }
            else
            {
                CreateRoom(p.width, p.length, p.height);
            }
        }
        else
        {
            Debug.LogError("Invalid Preset Index: " + presetIndex);
        }
    }

    public void OnClickCreateFromDropdown()
    {
        if (xDropdown != null && yDropdown != null)
        {
            string xStr = xDropdown.options[xDropdown.value].text;
            string yStr = yDropdown.options[yDropdown.value].text;

            float w = 10f, l = 10f, h = 3f;
            float.TryParse(xStr, out w);
            float.TryParse(yStr, out l);

            pendingWidth = w;
            pendingLength = l;
            pendingHeight = h;
            pendingSceneName = xStr + yStr;

            if (roomNamePanel)
            {
                roomNamePanel.SetActive(true);
                if (roomNameInput) roomNameInput.text = ""; // Clear previous
            }
            else
            {
                CreateRoom(w, l, h);
            }
        }
    }

    public void OnConfirmCreateRoom()
    {
        string name = ""; // Default to empty
        if(roomNameInput != null && !string.IsNullOrEmpty(roomNameInput.text))
        {
            name = roomNameInput.text;
        }
        // Else stays empty. No auto-generated name.

        PlayerPrefs.SetString("NewRoomName", name);
        CreateRoom(pendingWidth, pendingLength, pendingHeight);
    }

    public void OnCancelCreateRoom()
    {
        if (roomNamePanel) roomNamePanel.SetActive(false);
    }

    private void CreateRoom(float w, float l, float h)
    {
        // Clear any pending load request so SaveManager doesn't try to load an old file
        PlayerPrefs.DeleteKey("PendingLoadSave");

        // Store parameters to PlayerPrefs to be read by the builder scene
        PlayerPrefs.SetFloat("NewRoomWidth", w);
        PlayerPrefs.SetFloat("NewRoomLength", l);
        PlayerPrefs.SetFloat("NewRoomHeight", h);
        PlayerPrefs.SetInt("IsNewCustomRoom", 1);
        
        // Generate a temporary name for the new room so ScreenshotTrigger works immediately
        // The actual save name is generated in RoomGenerator.Start(), but let's pre-generate or handle it there.
        // Actually, RoomGenerator generates "Room_MMdd_HHmm". We can't know it here easily without logic duplicaton.
        // Best bet: RoomGenerator sets the Pref "LastLoadedSaveName" when it generates/saves.
        
        string targetScene = string.IsNullOrEmpty(pendingSceneName) ? builderSceneName : pendingSceneName;
        SceneManager.LoadScene(targetScene);
    }
}
