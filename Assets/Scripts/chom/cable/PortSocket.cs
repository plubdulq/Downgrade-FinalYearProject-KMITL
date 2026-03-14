using UnityEngine;

public class PortSocket : MonoBehaviour
{
    public CableType expectedType;

    public bool IsCorrectCable(CableType cable)
    {
        return cable == expectedType;
    }
}