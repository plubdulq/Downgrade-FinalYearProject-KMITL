using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] List<GameObject> holsters;
    [SerializeField] List<GameObject> externalPorts;
    [SerializeField] bool hideExternalport, hideHolsterPort;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HideHolsterUI(hideHolsterPort);
        HideExternalUI(hideExternalport);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void HideHolsterUI(bool hideHolsterPort)
    {
        if (!hideHolsterPort) return;
        int index = 0;
        foreach (GameObject holster in holsters)
        {
            if(holster == null) continue;
            if (index == 0)
            {
                index++;
                continue;
            }
            index++;
            holster.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
        }
    }
    void HideExternalUI(bool hideExternalport)
    {
        foreach (GameObject holster in externalPorts)
        {
            holster.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
        }
    }
}
