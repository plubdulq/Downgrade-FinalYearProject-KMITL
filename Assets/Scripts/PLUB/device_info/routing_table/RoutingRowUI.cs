using UnityEngine;
using TMPro;

public class RoutingRowUI : MonoBehaviour
{
    public TextMeshProUGUI interfaceText;
    public TextMeshProUGUI networkText;
    public TextMeshProUGUI nextHopText;
    public TextMeshProUGUI typeText;

    public void SetData(RouteEntry route)
    {
        // Interface
        interfaceText.text = $"G0/{route.portIndex}";

        // Network (เปลี่ยนจาก Destination → Network)
        networkText.text = route.networkCIDR;

        // Next Hop
        if (route.type == RouteType.Connected)
        {
            nextHopText.text = "-";
        }
        else
        {
            nextHopText.text = route.nextHopIP;
        }

        // Type
        typeText.text = route.type == RouteType.Connected ? "C" : "S";
    }
}