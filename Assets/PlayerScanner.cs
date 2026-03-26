using UnityEngine;
using BNG;
using Chomm.CableSystem;
using System.Collections.Generic;

public class PlayerScanner : MonoBehaviour
{
    public AudioClip TriggerSfx;
    public AudioClip PlugSfx;
    public AudioClip SnapSfx;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        ServerPlugScanner server = other.GetComponent<ServerPlugScanner>();

        if (server != null)
        {
            playSfx(TriggerSfx);

            CablePlug plug = GetHeldPlug();

            if (plug != null)
            {
                Debug.Log("Holding Plug: " + plug.plugType);

                foreach (var port in server.allPorts)
                {
                    bool canPlug = port.plugType == plug.plugType;

                    HighlightPort(port, canPlug);
                }
            }
        }
    }
private void OnTriggerStay(Collider other)
{
    ServerPlugScanner server = other.GetComponent<ServerPlugScanner>();

    if (server != null)
    {
        CablePlug plug = GetHeldPlug();

        if (plug != null)
        {
            foreach (var port in server.allPorts)
            {
                bool canPlug = port.plugType == plug.plugType;

                HighlightPort(port, canPlug);
            }
        }
    }
}
    private void OnTriggerExit(Collider other)
    {
        ServerPlugScanner server = other.GetComponent<ServerPlugScanner>();

        if (server != null)
        {
            Debug.Log("Exit Server Zone");

            foreach (var port in server.allPorts)
            {
                ResetPort(port);
            }
        }
    }

    CablePlug GetHeldPlug()
    {
        Grabber[] grabbers = FindObjectsOfType<Grabber>();

        foreach (var g in grabbers)
        {
            if (g.HeldGrabbable != null)
            {
                CablePlug plug = g.HeldGrabbable.GetComponent<CablePlug>();
                if (plug != null)
                    return plug;
            }
        }

        return null;
    }

    void HighlightPort(SnapZone port, bool canPlug)
    {
        var ring = port.GetComponentInChildren<BNG.SnapZoneRingHelper>();

        if (ring != null)
        {
           // playSfx(SnapSfx);

            if (canPlug)
                ring.Highlight(Color.green); // 🟢
            else
                ring.Highlight(Color.red);   // 🔴
        }
    }

    void ResetPort(SnapZone port)
    {
        var ring = port.GetComponentInChildren<BNG.SnapZoneRingHelper>();

        if (ring != null)
        {
            ring.ClearHighlight(); // 🔥 คืนค่า
        }
    }

    void playSfx(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}