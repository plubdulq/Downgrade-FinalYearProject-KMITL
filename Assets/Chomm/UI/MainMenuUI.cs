using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public List<UIU> uiList;
    private string lastPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActiveUI("myserver");
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ActiveUI(string name)
    {
        bool isNewPanel = false;
        foreach (var item in uiList)
        {
            if (item.Object != null)
                item.Object.SetActive(false);
        }
        foreach (UIU ui in uiList)
        {
            if (ui.name.ToLower() == name.ToLower())
            {
                ui.Object.SetActive(true);
                isNewPanel = true;
                lastPanel = ui.name;
                break;
            }
        }
        if ((!isNewPanel))
        {
            ActiveUI(lastPanel);
        }

    }
}

    [System.Serializable]
public class UIU
{
    public string name;
    public GameObject Object;
}
