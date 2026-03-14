using UnityEngine;

public class CableConnector : MonoBehaviour
{
    public CableEnd cableEnd;

    private void OnTriggerEnter(Collider other)
    {
        PortSocket port = other.GetComponent<PortSocket>();

        if (port != null)
        {
            if (port.IsCorrectCable(cableEnd.cableType))
            {
                Debug.Log("Correct Cable Type!");
            }
            else
            {
                Debug.Log("Not Cable type!");
            }
        }
    }
}
