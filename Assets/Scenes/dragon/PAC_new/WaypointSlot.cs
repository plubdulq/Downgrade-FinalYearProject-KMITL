using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaypointSystem;
using BNG;
namespace WaypointSystem
{
public class WaypointSlot : MonoBehaviour
{
    public bool hasDevice = false;
    public Waypoint waypoint; 
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Device"))
        {
            hasDevice = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Device"))
        {
            hasDevice = false;
        }
    }
}
}
