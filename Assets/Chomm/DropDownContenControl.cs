using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropDownContenControl : MonoBehaviour
{
    [SerializeField] GameObject itemPrefab;
    Dropdown dropdown;
    [SerializeField] Transform container;
    [SerializeField] int min, max;
    [SerializeField] bool generate = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dropdown = GetComponent<Dropdown>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnValidate()
    {
        if (generate)
        {
            GenerateItem();
        }
    }
    void GenerateItem()
    {
        foreach (Transform item in container)
        {
            Destroy(item.gameObject);
        }
        List<string> options = new List<string>()
        {
            "Option 1",
            "Option 2",
            "Option 3"
        };

        // เพิ่ม option เข้า dropdown
        dropdown.AddOptions(options);
        for (int i = min; i <= max; i++) { 
            GameObject go = Instantiate(itemPrefab,container);
            //Dropdown.op
            //dropdown.AddOptions()
            go.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = i.ToString();   
        }
    }
}
