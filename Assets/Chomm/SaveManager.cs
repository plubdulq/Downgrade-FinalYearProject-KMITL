using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chomm.CableSystem;
using BNG;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    
    [Header("Current Environment Config")]
    public float currentRoomWidth = 10f;
    public float currentRoomLength = 10f;
    public float currentRoomHeight = 3f;

    [Header("Resources Configuration")]
    public string equipmentResourcePath = "Prefabs/Equipment/"; 
    [System.Serializable]
    public struct CableEntry
    {
        public string typeName; // e.g. "Rj45", "PowerIn"
        public GameObject prefab;
    }
    public List<CableEntry> cablePrefabs; 

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Optional: Check on start too, just in case (though OnSceneLoaded usually covers it)
        CheckForPendingLoad();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        CheckForPendingLoad();
    }

    private void CheckForPendingLoad()
    {
        if (PlayerPrefs.HasKey("PendingLoadSave"))
        {
            string saveToLoad = PlayerPrefs.GetString("PendingLoadSave");
            PlayerPrefs.DeleteKey("PendingLoadSave");
            
            // Wait a frame or two? BNG might need to init.
            // Or just call LoadGame
            Debug.Log($"[SaveManager] Detected Pending Load for: {saveToLoad}. Loading now...");
            LoadGame(saveToLoad);
        }
    }

    // --- SAVE ---
    public void SaveGame(string saveName)
    {
        GameSaveData data = new GameSaveData();
        data.saveName = saveName;
        data.timestamp = System.DateTime.Now.ToString();
        data.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Save Room Config
        data.roomWidth = currentRoomWidth;
        data.roomLength = currentRoomLength;
        data.roomHeight = currentRoomHeight;

        // 1. Save Equipments
        var allEquipments = FindObjectsOfType<EquipmentData>();
        foreach (var eq in allEquipments)
        {
            EquipmentSaveData eqData = new EquipmentSaveData();
            eqData.uniqueID = eq.uniqueID;
            eqData.prefabName = eq.equipmentDataName;
            eqData.position = eq.transform.localPosition; // Use localPosition for children
            eqData.rotation = eq.transform.localRotation; // Use localRotation for children
            
            // Save Parent Info
            if (eq.transform.parent != null)
            {
                EquipmentData parentEq = eq.transform.parent.GetComponent<EquipmentData>();
                if (parentEq != null)
                {
                    eqData.parentID = parentEq.uniqueID;
                }
                else
                {
                    // Fallback to name for static objects
                    eqData.parentID = eq.transform.parent.name;
                }
            }
            
            data.equipments.Add(eqData);
        }

        // 2. Save Cables
        var allCables = FindObjectsOfType<Cable>();
        foreach (var cable in allCables)
        {
            if (cable.PlugA != null && cable.PlugB != null)
            {
                CableSaveData cData = new CableSaveData();
                
                // Save Cable Type
                if (cable.PlugA != null && cable.PlugB != null)
                {
                    PlugType a = cable.PlugA.plugType;
                    PlugType b = cable.PlugB.plugType;
                    
                    if (a == PlugType.FiberLCSinglemode || b == PlugType.FiberLCSinglemode) cData.cableType = "FiberLCSinglemode";
                    else if (a == PlugType.FiberLCMultimode || b == PlugType.FiberLCMultimode) cData.cableType = "FiberLCMultimode";
                    else if (a == PlugType.Rj45 || b == PlugType.Rj45) cData.cableType = "Rj45";
                    else if (a == PlugType.Dsl || b == PlugType.Dsl) cData.cableType = "DslToDsl";
                    else if (a == PlugType.PowerIn || b == PlugType.PowerIn || a == PlugType.PowerOut || b == PlugType.PowerOut) cData.cableType = "Power";
                    else
                    {
                        bool has13 = a == PlugType.IECC13 || b == PlugType.IECC13;
                        bool has14 = a == PlugType.IECC14 || b == PlugType.IECC14;
                        bool has19 = a == PlugType.IECC19 || b == PlugType.IECC19;
                        bool has20 = a == PlugType.IECC20 || b == PlugType.IECC20;
                        bool hasPlug = a == PlugType.StandardPlug || b == PlugType.StandardPlug;
                        
                        if (has13 && hasPlug) cData.cableType = "IECC13ToPlug";
                        else if (has13 && has14) cData.cableType = "IECC13ToC14";
                        else if (has19 && has20) cData.cableType = "IECC19ToC20";
                        else if (has13 && has20) cData.cableType = "IECC13ToC20";
                        else if (has14 && has19) cData.cableType = "IECC14ToC19";
                        else cData.cableType = cable.PlugA.plugType.ToString();
                    }
                }
                else if (cable.PlugA != null)
                {
                    cData.cableType = cable.PlugA.plugType.ToString();
                    if (cData.cableType == "PowerIn")
                    {
                        cData.cableType = "Power";
                    }
                }
                else
                {
                    cData.cableType = "Unknown";
                }
                GetConnectionInfo(cable.PlugA, out cData.startEquipmentID, out cData.startPortIndex);
                GetConnectionInfo(cable.PlugB, out cData.endEquipmentID, out cData.endPortIndex);
                
                // Save Intermediate Points
                if (cable.intermediatePoints != null && cable.intermediatePoints.Count > 0)
                {
                    cData.intermediatePoints = new List<CablePointSaveData>();
                    foreach(var point in cable.intermediatePoints)
                    {
                        if(point == null) continue;
                        
                        CablePointSaveData pData = new CablePointSaveData();
                        if(point.parent != null)
                        {
                            // Check specific Anchor logic requested by User
                            SnapZone zone = point.parent.GetComponent<SnapZone>();
                            if (zone != null && zone.isAnchor)
                            {
                                pData.isAnchorPoint = true;
                                // User requested to save Parent of the Anchor (Grandparent of point)
                                Transform grandParent = point.parent.parent;
                                if (grandParent != null)
                                {
                                    EquipmentData gpEq = grandParent.GetComponent<EquipmentData>();
                                    if (gpEq != null)
                                    {
                                        pData.parentID = gpEq.uniqueID;
                                    }
                                    else
                                    {
                                        pData.parentName = grandParent.name;
                                    }
                                }
                                pData.position = point.localPosition;
                            }
                            else
                            {
                                EquipmentData parentEq = point.parent.GetComponent<EquipmentData>();
                                if(parentEq != null)
                                {
                                    pData.parentID = parentEq.uniqueID;
                                    pData.position = point.localPosition; // Relative to parent
                                }
                                else
                                {
                                    pData.parentName = point.parent.name;
                                    pData.position = point.localPosition; // Relative to parent
                                }
                            }
                        }
                        else
                        {
                            pData.position = point.position; // World space
                        }
                        cData.intermediatePoints.Add(pData);
                    }
                }
                
                cData.sag = cable.sag;

                data.cables.Add(cData);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, saveName + ".json");
        File.WriteAllText(path, json);
        Debug.Log($"Game Saved to {path}");
    }

    private void GetConnectionInfo(CablePlug plug, out string eqID, out int portIndex)
    {
        eqID = "";
        portIndex = -1;

        if (plug == null) return;

        // Check if SnapZone is parent
        SnapZone zone = plug.GetComponentInParent<SnapZone>();
        if (zone != null)
        {
            EquipmentData eq = zone.GetComponentInParent<EquipmentData>();
            if (eq != null)
            {
                eqID = eq.uniqueID;
                var ports = eq.GetPorts();
                if (ports != null)
                {
                    portIndex = ports.IndexOf(zone);
                }
            }
        }
    }

    // --- LOAD ---
    public void LoadGame(string saveName)
    {
        string path = Path.Combine(Application.persistentDataPath, saveName + ".json");
        if (!File.Exists(path)) 
        { 
            Debug.LogError($"Save file not found at {path}"); 
            return; 
        }

        string json = File.ReadAllText(path);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        StartCoroutine(LoadRoutine(data));
    }

    System.Collections.IEnumerator LoadRoutine(GameSaveData data)
    {
        Debug.Log("Starting Load Routine...");

        // 1. Clear Existing Scene
        var oldEquipments = FindObjectsOfType<EquipmentData>();
        foreach (var eq in oldEquipments) Destroy(eq.gameObject);
        
        var oldCables = FindObjectsOfType<Cable>();
        foreach (var c in oldCables) Destroy(c.gameObject);
        
        // Restore Room Config
        if (data.roomWidth > 0) currentRoomWidth = data.roomWidth;
        if (data.roomLength > 0) currentRoomLength = data.roomLength;
        if (data.roomHeight > 0) currentRoomHeight = data.roomHeight;

        // Try to find RoomGenerator to apply size
        RoomGenerator roomGen = FindObjectOfType<RoomGenerator>();
        if (roomGen != null)
        {
            roomGen.GenerateRoom(currentRoomWidth, currentRoomLength, currentRoomHeight);
        }

        yield return null; 
        
        Dictionary<string, EquipmentData> idToEquipment = new Dictionary<string, EquipmentData>();

        // 2. Instantiate Equipments
        // 2. Instantiate Equipments
        // First pass: Instantiate objects that are not already in the scene
        // Better approach: Iterate and check if ID exists. If not, instantiate.
        // But order matters. If we instantiate Child before Parent, we create a loose child. 
        // Then Parent creates a duplicate child.
        // We can't easily solve order without dependency graph. 
        // BUT logic: "If ID exists in dictionary, SKIP instantiate".
        
        foreach (var eqData in data.equipments)
        {
            // If already loaded (e.g. as a child of a previous prefab), skip
            if (idToEquipment.ContainsKey(eqData.uniqueID))
            {
                // Just update data
                EquipmentData existing = idToEquipment[eqData.uniqueID];
                existing.uniqueID = eqData.uniqueID; // Ensure match
                // We will position/parent it in the next loop (2.5) or update pos here?
                // Parent/Pos update happens in 2.5.
                continue;
            }

            // Load from Resources
            GameObject prefab = Resources.Load<GameObject>(equipmentResourcePath + eqData.prefabName);
            if (prefab)
            {
               // Instantiate
               GameObject obj = Instantiate(prefab); 
               
               // Register ALL EquipmentData found in this new object (Self + Children)
               EquipmentData[] allEqs = obj.GetComponentsInChildren<EquipmentData>(true);
               foreach (var eq in allEqs)
               {
                   // If this equipment has a saved ID in the prefab, use it to register
                   // BUT, the save data might expect a specific ID. 
                   // If the prefab has ID "A", and we register "A". 
                   // When the loop reaches "A", it skips. Correct.
                   
                   // PROBLEM: If the prefab has empty ID (generated at runtime), 
                   // we register "RandomID". 
                   // The save data has "SavedID".
                   // Loop reaches "SavedID", doesn't find it. Instantiates NEW "SavedID".
                   // Now we have "RandomID" (child) and "SavedID" (root). Duplicate.
                   
                   // Fix for 'Root' object: We manually overwrite its ID with eqData.uniqueID
                   if (eq.gameObject == obj)
                   {
                       eq.uniqueID = eqData.uniqueID;
                       if (!idToEquipment.ContainsKey(eqData.uniqueID))
                       {
                           idToEquipment.Add(eqData.uniqueID, eq);
                       }
                   }
                   else
                   {
                       // This is a child equipment (e.g. Port inside Rack)
                       // detailed matching to find its Saved Data
                       // Fix: Check ParentID against strict ID OR Parent Name (fallback)
                       var match = data.equipments.FirstOrDefault(x => 
                           (x.parentID == eqData.uniqueID || x.parentID == obj.name || (eq.transform.parent != null && x.parentID == eq.transform.parent.name)) && 
                           (x.prefabName == eq.equipmentDataName || eq.name.Contains(x.prefabName) || string.IsNullOrEmpty(x.prefabName)) &&
                           Vector3.Distance(x.position, eq.transform.localPosition) < 0.1f // Fuzzy match position
                       );
                       
                       // Debug matching
                       if (match == null)
                       {
                           Debug.Log($"[SaveSystem] Child {eq.name} (Parent: {eqData.uniqueID}) - No match found in save data.");
                           
                           // Debug: Scan ALL equipments to see if it exists with different parent?
                           var potentialMatches = data.equipments.Where(x => 
                               (x.prefabName == eq.equipmentDataName || eq.name.Contains(x.prefabName)) &&
                               Vector3.Distance(x.position, eq.transform.localPosition) < 0.5f // Relaxed distance
                           ).ToList();

                           if (potentialMatches.Count > 0)
                           {
                               foreach(var p in potentialMatches)
                               {
                                   Debug.Log($"   [Found Candidate in Global List] ID: {p.uniqueID}, Name: {p.prefabName}, ParentID: {p.parentID} (Expected: {eqData.uniqueID}), Dist: {Vector3.Distance(p.position, eq.transform.localPosition)}");
                               }
                           }
                           else
                           {
                                Debug.Log($"   [No Global Candidate] No equipment matches name '{eq.name}' and pos {eq.transform.localPosition} in entire save file.");
                           }
                       }

                       if (match != null)
                       {
                           // Found the saved data for this native child!
                           // Restore its ID
                           eq.uniqueID = match.uniqueID;
                           
                           // Check if we already loaded this ID (as an orphan earlier)
                           if (idToEquipment.ContainsKey(match.uniqueID))
                           {
                               // We found a better version (this native child). Destroy the orphan.
                               GameObject orphan = idToEquipment[match.uniqueID].gameObject;
                               if(orphan != eq.gameObject) // Safety check
                               {
                                    Debug.Log($"[SaveSystem] Found native child {eq.name} (ID: {eq.uniqueID}). Destroying premature orphan.");
                                    Destroy(orphan);
                                    idToEquipment[match.uniqueID] = eq;
                               }
                           }
                           else
                           {
                               idToEquipment.Add(match.uniqueID, eq);
                           }
                       }
                       else
                       {
                           // Child exists in prefab but NOT in save data? (New feature in prefab?)
                           // Generate new ID if needed or keep prefab ID
                           if (string.IsNullOrEmpty(eq.uniqueID)) eq.GenerateID();
                           
                           // Register if not conflict
                           if (!idToEquipment.ContainsKey(eq.uniqueID))
                           {
                               idToEquipment.Add(eq.uniqueID, eq);
                           }
                       }
                   }
               }
            }
            else
            {
                Debug.LogWarning($"Could not find equipment prefab: {eqData.prefabName} at {equipmentResourcePath}");
            }
        }

        // 2.5 Re-parent and Position Equipments
        foreach (var eqData in data.equipments)
        {
            if (idToEquipment.ContainsKey(eqData.uniqueID))
            {
                EquipmentData childEq = idToEquipment[eqData.uniqueID];
                
                // Try to Parent
                if (!string.IsNullOrEmpty(eqData.parentID))
                { 
                    // Try find parent in loaded equipments
                    if (idToEquipment.ContainsKey(eqData.parentID))
                    {
                        EquipmentData parentEq = idToEquipment[eqData.parentID];
                        childEq.transform.SetParent(parentEq.transform);
                    }
                    else
                    {
                        // Try find static object by name
                        GameObject staticParent = GameObject.Find(eqData.parentID);
                        if (staticParent != null)
                        {
                            childEq.transform.SetParent(staticParent.transform);
                        }
                        else
                        {
                            Debug.LogWarning($"   -> Could not find parent with ID or Name: {eqData.parentID} for equipment {childEq.name}");
                        }
                    }
                }
                
                // Restore local transform (valid for both root and children)
                childEq.transform.localPosition = eqData.position;
                childEq.transform.localRotation = eqData.rotation;
            }
        }

        // 3. Reconnect Cables
        foreach (var cData in data.cables)
        {
            GameObject prefabToUse = null;
            if (cablePrefabs != null)
            {
                var entry = cablePrefabs.FirstOrDefault(x => x.typeName == cData.cableType);
                if (entry.prefab != null) prefabToUse = entry.prefab;
            }
            
            // Fallback (or if list empty)
            if (prefabToUse == null && cablePrefabs != null && cablePrefabs.Count > 0)
            {
                prefabToUse = cablePrefabs[0].prefab;
            }

            if (prefabToUse == null) continue;

            GameObject cableObj = Instantiate(prefabToUse);
            Cable newCable = cableObj.GetComponent<Cable>();

            if (newCable == null) continue;

            newCable.sag = cData.sag;

            // Reconnect A
            SnapZone startZone = null;
            if (!string.IsNullOrEmpty(cData.startEquipmentID) && idToEquipment.ContainsKey(cData.startEquipmentID))
            {
                startZone = idToEquipment[cData.startEquipmentID].GetPortByIndex(cData.startPortIndex);
                if (startZone != null && newCable.PlugA != null)
                {
                    SnapPlugToZone(newCable.PlugA, startZone);
                }
            }

            // Reconnect B
            SnapZone endZone = null;
            if (!string.IsNullOrEmpty(cData.endEquipmentID) && idToEquipment.ContainsKey(cData.endEquipmentID))
            {
                endZone = idToEquipment[cData.endEquipmentID].GetPortByIndex(cData.endPortIndex);
                if (endZone != null && newCable.PlugB != null)
                {
                    SnapPlugToZone(newCable.PlugB, endZone);
                }
            }

            // Restore Intermediate Points
            if (cData.intermediatePoints != null && cData.intermediatePoints.Count > 0)
            {
                foreach (CablePointSaveData pData in cData.intermediatePoints)
                {
                    // Create point helper
                    GameObject pointObj = new GameObject("SavedCablePoint");
                    
                    // Try to Parent
                    bool parentFound = false;
                    
                    if (pData.isAnchorPoint)
                    {
                        // Special Anchor Logic: Find Grandparent -> Attach to Child(0)
                        GameObject grandParent = null; 
                        
                        // Find GrandParent
                        if (!string.IsNullOrEmpty(pData.parentID) && idToEquipment.ContainsKey(pData.parentID))
                        {
                            grandParent = idToEquipment[pData.parentID].gameObject;
                        }
                        else if (!string.IsNullOrEmpty(pData.parentName))
                        {
                            grandParent = GameObject.Find(pData.parentName);
                        }
                        
                        // Attach to Child(0)
                        if (grandParent != null)
                        {
                            if(grandParent.transform.childCount > 0)
                            {
                                Transform anchor = grandParent.transform.GetChild(0);
                                pointObj.transform.SetParent(anchor);
                                pointObj.transform.localPosition = pData.position;
                                parentFound = true;
                            }
                            else
                            {
                                Debug.LogWarning($"[SaveSystem] Anchor grandparent {grandParent.name} has no children. Cannot attach to Child(0). Falling back to grandparent.");
                                pointObj.transform.SetParent(grandParent.transform);
                                pointObj.transform.localPosition = pData.position;
                                parentFound = true;
                            }
                        }
                    }
                    else
                    {
                        // Normal Parent Logic
                        if (!string.IsNullOrEmpty(pData.parentID) && idToEquipment.ContainsKey(pData.parentID))
                        {
                            // Parent is Equipment
                            pointObj.transform.SetParent(idToEquipment[pData.parentID].transform);
                            pointObj.transform.localPosition = pData.position;
                            parentFound = true;
                        }
                        else if (!string.IsNullOrEmpty(pData.parentName))
                        {
                             // Parent is generic object (fallback search)
                             GameObject parentGO = GameObject.Find(pData.parentName);
                             if(parentGO != null)
                             {
                                 pointObj.transform.SetParent(parentGO.transform);
                                 pointObj.transform.localPosition = pData.position;
                                 parentFound = true;
                             }
                        }
                    }
                    
                    if (!parentFound)
                    {
                        // No parent or parent not found -> Use World Position (assuming pData.position was World if no parent)
                        // But wait, our Save logic saves Local if parent exists, World if not.
                        // If we failed to find parent, treating Local as World will be wrong position!
                        // However, we have no choice. 
                        pointObj.transform.position = pData.position;
                    }
                    
                    newCable.intermediatePoints.Add(pointObj.transform);
                }
            }
        }
        
        
        Debug.Log("Load Complete.");
        
        // Notify RoomGenerator of the current save name
        if (RoomGenerator.Instance != null)
        {
            RoomGenerator.Instance.SetRoomName(data.saveName);
        }
    }

    private void SnapPlugToZone(CablePlug plug, SnapZone zone)
    {
        Grabbable grab = plug.GetComponent<Grabbable>();
        if (grab && zone)
        {
             // Pre-position (optional, but helps avoid visual jumping if GrabGrabbable lerps)
             plug.transform.position = zone.transform.position;
             plug.transform.rotation = zone.transform.rotation;

             zone.GrabGrabbable(grab);
        }
    }
}
