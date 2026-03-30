using UnityEngine;
using TMPro;

public class AddStaticRouteUI : MonoBehaviour
{
    [Header("UI Input")]
    public TMP_Text interfaceInput;   // G0/1
    public TMP_Text destinationInput; // 192.168.10.0/24
    public TMP_Text nextHopInput;     // 192.168.1.2

    private NetworkDevice networkDevice;

    [Header("Context")]
    public string currentDeviceGuid;
    

    // =============================
    // 🔥 กดปุ่ม Add
    // =============================
    public void OnClickAddRoute()
    {
        networkDevice = GetComponentInParent<NetworkDevice>(); // อัพเดต port info ใน label card ก่อน
        currentDeviceGuid = networkDevice.guid;
        Debug.Log($"OnClickAddRoute - currentDeviceGuid: {currentDeviceGuid}");

        string interfaceName = interfaceInput.text;
        string dest = destinationInput.text;
        string nextHop = nextHopInput.text;

        if (string.IsNullOrEmpty(interfaceName) || string.IsNullOrEmpty(dest))
        {
            Debug.LogError("Missing input");
            return;
        }

        // 🔧 normalize
        dest = NetworkUtils.NormalizeToCIDR(dest);

        int portIndex = ParseInterface(interfaceName);
        if (portIndex < 0)
        {
            Debug.LogError("Invalid interface format");
            return;
        }

        string portIP = NetworkManager.Instance.GetPortIP(currentDeviceGuid, portIndex);
        Debug.Log($"[UI] {nextHop} IP in {portIndex}: {portIP}");
        if (!NetworkUtils.IsSameSubnet(portIP, nextHop))
        {
            Debug.LogError("Next hop not in same subnet as interface!");
            return;
        }

        // 🔥 Add static route
        RoutingTableManager.Instance.AddStaticRoute(
            currentDeviceGuid,
            dest,
            nextHop,
            portIndex
        );

        Debug.Log($"[UI] Add Route {dest} via {nextHop} on port {portIndex}");
    }

    // =============================
    // 🔧 แปลง G0/1 → 1
    // =============================
    private int ParseInterface(string iface)
    {
        // ✅ case 1: user ใส่ "1", "2", "3"
        if (int.TryParse(iface, out int directIndex))
        {
            return directIndex;
        }
        // รองรับ G0/1, G0/2
        if (iface.Contains("/"))
        {
            var parts = iface.Split('/');
            if (int.TryParse(parts[1], out int index))
                return index;
        }

        return -1;
    }
}