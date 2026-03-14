
using UnityEngine;

public class LAN_Plug : MonoBehaviour
{
    Rigidbody rb;
    CableSocket socket;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SnapToSocket(CableSocket s)
    {
        socket = s;
        rb.isKinematic = true;
        transform.position = s.snapPoint.position;
        transform.rotation = s.snapPoint.rotation;
        transform.SetParent(s.snapPoint);
    }

    public void Unplug()
    {
        if (socket != null)
        {
            socket.Release();
            socket = null;
        }

        transform.SetParent(null);
        rb.isKinematic = false;
    }
}
