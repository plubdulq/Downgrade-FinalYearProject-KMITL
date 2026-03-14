
using UnityEngine;

public class CableSocket : MonoBehaviour
{
    public Transform snapPoint;
    public LAN_Plug currentPlug;

    private void OnTriggerEnter(Collider other)
    {
        LAN_Plug plug = other.GetComponent<LAN_Plug>();
        if (plug && currentPlug == null)
        {
            plug.SnapToSocket(this);
            currentPlug = plug;
        }
    }

    public void Release()
    {
        currentPlug = null;
    }
}
