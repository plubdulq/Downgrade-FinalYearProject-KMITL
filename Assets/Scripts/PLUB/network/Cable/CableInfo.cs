using UnityEngine;

public class CableInfo : MonoBehaviour
{
    public string uniqueID;

    [ContextMenu("GenerateID")]
    public void GenerateID()
    {
        uniqueID = System.Guid.NewGuid().ToString();
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            GenerateID();
        }
    }
    
}