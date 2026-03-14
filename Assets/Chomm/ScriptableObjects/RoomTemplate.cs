using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomTemplate", menuName = "Room System/Room Template")]
public class RoomTemplate : ScriptableObject
{
    public string templateName;
    [TextArea] public string description;
    public Sprite thumbnail;
    public string sceneName; // The scene to load
    
    // Optional: Pre-configuration data if using a single scene
    // public Vector2 defaultSize; 
}
