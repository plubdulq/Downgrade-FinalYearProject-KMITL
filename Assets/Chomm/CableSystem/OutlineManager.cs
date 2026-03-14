using Chomm.CableSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

public class OutlineManager : MonoBehaviour
{
    public static OutlineManager Instance;
    public List<Cable> outlines;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ActiveOL(Cable activeThis, bool uiActive)
    {
        DisableAll();
        foreach (Cable cable in outlines)
        {
            if (cable == activeThis)
            {
                cable.ActiveOutline(uiActive);
                break;
            }
        }
    }

    internal void AddCable(Cable cable)
    {
        outlines.Add(cable);
    }

    internal void DisableAll()
    {
        foreach (Cable cable in outlines)
        {
            cable.ActiveOutline(false);
        }

    }
}
