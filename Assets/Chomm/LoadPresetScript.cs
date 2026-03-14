using UnityEngine;
using UnityEngine.UI;

public class LoadPresetScript : MonoBehaviour
{
    public RoomCreatorManager creatorManager;
    public int presetIndex = 0;
    Button loadSceneB;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadSceneB = GetComponent<Button>();
        loadSceneB.onClick.AddListener(LoadPreset);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void LoadPreset()
    {
        creatorManager.OnClickCreateCustomRoomByPreset(presetIndex);
    }
}
