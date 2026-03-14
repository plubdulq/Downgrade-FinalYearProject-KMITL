using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public static RoomGenerator Instance;
    public Transform floor;
    public Transform ceiling; // Optional
    [SerializeField] Transform way;
    public Transform wallN, wallS, wallE, wallW;
    string roomName;

    [Header("Custom Room Size")]
    public float customWidth = 10f;
    public float customLength = 10f;
    public float customHeight = 3f;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        // Check if we should generate a custom room
        if (PlayerPrefs.HasKey("IsNewCustomRoom") && PlayerPrefs.GetInt("IsNewCustomRoom") == 1)
        {
            float w = PlayerPrefs.GetFloat("NewRoomWidth", 10f);
            float l = PlayerPrefs.GetFloat("NewRoomLength", 10f);
            float h = PlayerPrefs.GetFloat("NewRoomHeight", 3f);

            GenerateRoom(w, l, h);

            // Check for custom name
            string roomName = PlayerPrefs.GetString("NewRoomName", "");
            
            // If name is empty, DO NOT generate a default name and DO NOT save.
            if (!string.IsNullOrEmpty(roomName))
            {
                this.roomName = roomName;
                
                // We need to set these values in SaveManager so it saves them next time
                // Or trigger a save immediately.
                if (SaveManager.Instance != null)
                {
                    // CRITICAL: Update SaveManager with the new custom dimensions BEFORE saving
                    SaveManager.Instance.currentRoomWidth = w;
                    SaveManager.Instance.currentRoomLength = l;
                    SaveManager.Instance.currentRoomHeight = h;

                    SaveManager.Instance.SaveGame(roomName);
                    
                    // Store for ScreenshotTrigger
                    PlayerPrefs.SetString("LastLoadedSaveName", roomName);
                }
            }
            else
            {
                this.roomName = ""; // Ensure it's empty so OnApplicationQuit doesn't save either
            }

            // Clear flag so reloading scene doesn't reset specialized changes
            PlayerPrefs.SetInt("IsNewCustomRoom", 0);
            PlayerPrefs.DeleteKey("NewRoomName"); // Clear name prefw
        }
    }
    private void OnApplicationQuit()
    {
        if (SaveManager.Instance != null && !string.IsNullOrEmpty(roomName))
        {
            SaveManager.Instance.SaveGame(roomName);
        }
    }

    public void SetRoomName(string name)
    {
        this.roomName = name;
    }
    public void SaveOnLoadScene()
    {
        if (SaveManager.Instance != null && !string.IsNullOrEmpty(roomName))
        {
            SaveManager.Instance.SaveGame(roomName);

        }
    }
    public void GenerateRoom(float width, float length, float height)
    {
        // Simple resizing logic assume pivot is center for X/Z, bottom for Y
        if(floor) floor.localScale = new Vector3(width, 1, length);
        if(ceiling) 
        {
            ceiling.localScale = new Vector3(width, 1, length);
            ceiling.localPosition = new Vector3(0, height, 0);
        }

        // Adjust Walls... (Implementation depends on pivots)
        // Assume wall pivot is bottom-center
        // WallN (Forward) -> Z = length/2
        if(wallN) {
             wallN.localPosition = new Vector3(0, 0, length / 2f);
             wallN.localScale = new Vector3(width, height, 1);
        }
        // WallS (Back) -> Z = -length/2
        if(wallS) {
             // Custom: Width - 2, Left Aligned (Shift center by -1.0)
             wallS.localPosition = new Vector3(-1.0f, 0, -length / 2f);
             wallS.localScale = new Vector3(width - 2f, height, 1);

            // Instantiate Way Prefab in the gap
                GameObject wayPrefab = Resources.Load<GameObject>("Prefabs/Building/Way");
            if (wayPrefab != null)
            {
                GameObject wayInstance = null;
                if (way.gameObject == null)
                {
                    wayInstance = Instantiate(wayPrefab, wallS.parent); // Parent to same container
                    way = wayInstance.transform;                                                // Position: Right gap center = width/2 - 1
                }
                else
                {
                    wayInstance = way.gameObject;
                }
                // Z matches wallS
                wayInstance.transform.localPosition = new Vector3(width / 2f - 1f, 0, -length / 2f);
                // Match rotation of wallS if possible, otherwise identity or look 'in'
                wayInstance.transform.localRotation = wallS.localRotation;
                // Optional: Scale height to match room? User didn't specify. Keeping prefab scale or matching height might be safer.
                // Assuming Way is a standard door/path, might not want to stretch it. Leaving scale alone for now.

                // Move XR Rig to spawnPoint
                Transform spawnPoint = wayInstance.transform.Find("SpawnPoint");
                if (spawnPoint != null)
                {
                    GameObject playerRig = GameObject.Find("XR Rig Advanced (1)");
                    if (playerRig != null)
                    {
                        playerRig.transform.position = spawnPoint.position;
                        playerRig.transform.rotation = spawnPoint.rotation;
                    }
                    else
                    {
                        Debug.LogWarning("XR Rig Advanced (1) not found in scene.");
                    }
                }
                else
                {
                    Debug.LogWarning("'spawnPoint' not found in Way prefab instance.");
                }
            }
        }
        // WallE (Right) -> X = width/2
        if(wallE) {
             wallE.localPosition = new Vector3(width / 2f, 0, 0);
             wallE.localScale = new Vector3(1, height, length);
        }
        // WallW (Left) -> X = -width/2
        if(wallW) {
             wallW.localPosition = new Vector3(-width / 2f, 0, 0);
             wallW.localScale = new Vector3(1, height, length); 
        }
    }
}
