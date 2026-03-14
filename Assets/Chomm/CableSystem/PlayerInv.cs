using Chomm.CableSystem;
using UnityEngine;

public class PlayerInv : MonoBehaviour
{
    public static PlayerInv Instance;
    public CablePlug currentPlug;
    private void Awake()
    {
        
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CheckPlug(CablePlug newPlug)
    {
        if (currentPlug !=null&& currentPlug != newPlug)
        {
            currentPlug.UIClose();
        }
        currentPlug = newPlug;
    }
}
