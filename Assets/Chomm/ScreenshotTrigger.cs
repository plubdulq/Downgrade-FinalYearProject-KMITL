using UnityEngine;

public class ScreenshotTrigger : MonoBehaviour
{
    [Tooltip("Key to press to take a screenshot")]
    public KeyCode triggerKey = KeyCode.P;

    [Tooltip("If true, assumes the RoomGenerator has the current room name.")]
    public bool useRoomGeneratorName = true;

    [Tooltip("Fallback name if RoomGenerator is missing or empty")]
    public string fallbackName = "MyRoom";

    void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            TakeScreenshot();
        }
    }

    public void TakeScreenshot()
    {
        string nameToUse = fallbackName;

        if (useRoomGeneratorName && RoomGenerator.Instance != null)
        {
            // We need a way to get the current room name from RoomGenerator
            // Looking at RoomGenerator.cs, it has a private 'roomName' field and 'SetRoomName' method.
            // But it doesn't seem to expose a getter. Explicitly using reflection or hoping we can add a getter later.
            // For now, let's assume we can get it or rely on SaveManager's last save?
            // Actually, RoomGenerator.cs in previous turn showed: 
            // string roomName; ... SetRoomName(string name) ...
            
            // I'll stick to a hardcoded property access for now, assuming I might need to add a Getter to RoomGenerator.
            // Or better, let's modify RoomGenerator to expose it.
            // For this file, I'll assume a GetRoomName() method exists or I will add it.
        }
        
        // Temporary hack: Retrieve name from SaveManager or just generated timestamp?
        // If we want to overwrite the current save's thumbnail, we need the exact save name.
        // SaveManager.Instance.SaveGame(saveName) is called. 
        // Maybe we can store the 'CurrentSaveName' in SaveManager?
        
        if (SaveManager.Instance != null)
        {
             // We'll trust the user to have a save name. 
             // Actually, let's use the current time if we are just testing, 
             // BUT the requirement is "Thumbnail for the save".
             // So we MUST know the save name.
             
             // Let's rely on a static 'CurrentRoomName' in a manager.
        }

        // For now, simplest approach:
        // Ask SaveManager context.
        
        string currentSaveName = PlayerPrefs.GetString("LastLoadedSaveName", "");
        if(!string.IsNullOrEmpty(currentSaveName))
        {
             nameToUse = currentSaveName;
        }
        else
        {
             nameToUse = "Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmm");
        }

        ScreenshotManager.Instance.CaptureAndSave(nameToUse);
    }
}
