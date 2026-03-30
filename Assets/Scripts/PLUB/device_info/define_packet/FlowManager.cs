using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class FlowManager : MonoBehaviour
{
    public static FlowManager Instance;

    public Dictionary<string, NetworkFlow> flowDict = new();

    void Awake()
    {
        Instance = this;
    }

    public void CreateAndRegisterFlow(string src_guid, string sourceIP, string destIP, float payloadBytes, float packetRate)
    {
        // 🔧 normalize ให้เป็น CIDR
        sourceIP = NetworkUtils.NormalizeToCIDR(sourceIP);
        destIP   = NetworkUtils.NormalizeToCIDR(destIP);

        var sourceDevice = NetworkManager.Instance.devices[src_guid];

        if (sourceDevice == null)
        {
            Debug.LogError("Source device not found");
            return;
        }

        // 🌊 สร้าง flow (ใช้ IP แทน GUID)
        NetworkFlow flow = new NetworkFlow
        {
            sourceIP = sourceIP,
            destinationIP = destIP,
            payloadSize = payloadBytes,
            packetRate = packetRate
        };

        // 🔥 build path + apply load + register per device
        bool success = BuildPathBFS(flow, sourceDevice, destIP);

        if (!success)
        {
            Debug.LogError("Path not found");
            return;
        }

        // ✅ register ลง dict
        if (!flowDict.ContainsKey(flow.flowGuid))
        {
            flowDict.Add(flow.flowGuid, flow);
        }

        NetworkSimulationManager.Instance.flows.Add(flow);

        Debug.Log($"[FLOW REGISTERED] {flow.flowGuid} | {flow.sourceIP} -> {flow.destinationIP}");
    }

    private bool BuildPathBFS(NetworkFlow flow, DeviceNetworkState startDevice, string destIP)
    {
        Queue<PathNode> queue = new Queue<PathNode>();
        HashSet<string> visited = new HashSet<string>();

        queue.Enqueue(new PathNode
        {
            device = startDevice,
            path = new List<string>()
        });

        while (queue.Count > 0)
        {
            Debug.Log($"[FlowManager] Queue count: {queue.Count}");
            var current = queue.Dequeue();
            var currentDevice = current.device;

            if (visited.Contains(currentDevice.guid))
                continue;

            visited.Add(currentDevice.guid);

            // 🎯 ถึงปลายทาง
            if (DeviceHasExactIP(currentDevice, destIP))
            {
                flow.cablePath = current.path;

                // 🔥 apply load ทีเดียวตอนเจอ path
                foreach (var cable in flow.cablePath)
                    ApplyLoad(cable, flow);

                Debug.Log("Reached destination via BFS");
                return true;
            }

            // =========================
            // 🔹 CASE 1: SWITCH (flood)
            // =========================
            if (currentDevice.device_type == "switch")
            {
                foreach (var port in currentDevice.ports)
                {
                    if (string.IsNullOrEmpty(port.cable_guid)) continue;

                    string nextGuid = CableManager.Instance.GetOtherDeviceGuid(
                        port.cable_guid,
                        currentDevice.guid
                    );

                    if (visited.Contains(nextGuid)) continue;

                    var nextDevice = NetworkManager.Instance.devices[nextGuid];
                    var newPath = new List<string>(current.path);
                    newPath.Add(port.cable_guid);
                    Debug.Log($"[FlowManager: switch] New path length: {newPath.Count}");
                    queue.Enqueue(new PathNode
                    {
                        device = nextDevice,
                        path = newPath
                    });
                }

                continue;
            }

            // =========================
            // 🔹 CASE 2: L3 device (Firewall/Router)
            // =========================
            bool foundSubnet = false;

            foreach (var port in currentDevice.ports)
            {
                if (string.IsNullOrEmpty(port.cable_guid) || string.IsNullOrEmpty(port.my_ip))
                    continue;

                if (NetworkUtils.IsSameSubnet(port.my_ip, destIP))
                {

                    string nextGuid = CableManager.Instance.GetOtherDeviceGuid(
                        port.cable_guid,
                        currentDevice.guid
                    );

                    if (visited.Contains(nextGuid)) continue;

                    var nextDevice = NetworkManager.Instance.devices[nextGuid];
                    // ✅ 🔥 FIREWALL CHECK ตรงนี้
                    if (currentDevice.device_type == "firewall")
                    {
                        bool allowed = IsPacketAllowedByFirewall(
                            currentDevice.guid,
                            port.my_ip,   // source
                            destIP        // destination
                        );

                        if (!allowed)
                        {
                            Debug.Log($"[Firewall] BLOCKED from {port.my_ip} to {destIP}");
                            continue; // ❌ ห้ามไปต่อ
                        }
                        else
                        {
                            Debug.Log($"[Firewall] ALLOWED from {port.my_ip} to {destIP}");
                        }
                    }

                    var newPath = new List<string>(current.path);
                    newPath.Add(port.cable_guid);
                    Debug.Log($"[FlowManager: network device] New path length: {newPath.Count}");
                    queue.Enqueue(new PathNode
                    {
                        device = nextDevice,
                        path = newPath
                    });

                    foundSubnet = true;
                }
            }

            // =========================
            // 🔴 CASE 3: ไม่เจอ subnet → ใช้ router
            // =========================
            if (!foundSubnet)
            {
                var defaultPort = currentDevice.ports[0];

                if (string.IsNullOrEmpty(defaultPort.cable_guid))
                    continue;

                string gatewayGuid = CableManager.Instance.GetOtherDeviceGuid(
                    defaultPort.cable_guid,
                    currentDevice.guid
                );

                var route = RoutingTableManager.Instance.FindBestRoute(
                    gatewayGuid,
                    destIP.Split("/")[0]
                );

                if (route == null) continue;

                var gatewayDevice = NetworkManager.Instance.devices[gatewayGuid];

                if (route.portIndex < 0 || route.portIndex >= gatewayDevice.ports.Count)
                    continue;

                var routePort = gatewayDevice.ports[route.portIndex];

                if (string.IsNullOrEmpty(routePort.cable_guid))
                    continue;

                string nextGuid = CableManager.Instance.GetOtherDeviceGuid(
                    routePort.cable_guid,
                    gatewayGuid
                );

                if (visited.Contains(nextGuid)) continue;

                var nextDevice = NetworkManager.Instance.devices[nextGuid];

                var newPath = new List<string>(current.path);
                newPath.Add(routePort.cable_guid);
                Debug.Log($"[FlowManager: router] New path length: {newPath.Count}");
                queue.Enqueue(new PathNode
                {
                    device = nextDevice,
                    path = newPath
                });
            }
        }

        return false;
    }

    bool IsPacketAllowedByFirewall(string firewallGuid, string srcIP, string destIP)
    {
        var firewall = FirewallManager.Instance.firewallDevices[firewallGuid];

        if (firewall == null || firewall.policy == null)
            return true; // ไม่มี policy = allow default

        foreach (var rule in firewall.policy.rules)
        {
            bool matchSrc = string.IsNullOrEmpty(rule.sourceIP) || rule.sourceIP == srcIP;
            bool matchDst = string.IsNullOrEmpty(rule.destinationIP) || rule.destinationIP == destIP;

            // (optional) คุณสามารถเพิ่ม port / protocol ได้ตรงนี้
            if (matchSrc && matchDst)
            {
                return rule.action.ToLower() == "allow";
            }
        }

        // default policy (แนะนำให้ explicit)
        return true;
    }

    // 🔥 Core routing แบบ subnet
    private bool BuildPathSubnet(NetworkFlow flow, DeviceNetworkState startDevice, string destIP)
    {
        DeviceNetworkState currentDevice = startDevice;

        HashSet<string> visited = new HashSet<string>();

        int safety = 0;

        while (currentDevice != null && safety < 50)
        {
            safety++;
            
            if (DeviceHasExactIP(currentDevice, destIP))
            {
                Debug.Log("Reached destination device");
                return true;
            }

            visited.Add(currentDevice.guid);

            bool foundSubnetPath = false;

            foreach (var port in currentDevice.ports)
            {
                //Debug.Log($"[FlowManager] Checking device {currentDevice.guid} port {port.portNumber} with IP {port.my_ip} destIP {destIP}");
                if (string.IsNullOrEmpty(port.cable_guid) || string.IsNullOrEmpty(port.my_ip))
                {
                    //Debug.Log($"[FlowManager] Device {currentDevice.guid} has port with missing cable or IP");
                    continue;
                }

                // 🔍 เช็ค subnet
                if (NetworkUtils.IsSameSubnet(port.my_ip, destIP))
                {
                    // 🔥 เพิ่ม load
                    Debug.Log($"[FlowManager] Device {currentDevice.guid} port {port.portNumber} matches subnet with destIP {destIP}. Applying load.");
                    
                    ApplyLoad(port.cable_guid, flow);
                    flow.cablePath.Add(port.cable_guid);

                    string nextGuid = CableManager.Instance.GetOtherDeviceGuid(port.cable_guid, currentDevice.guid);
                    Debug.Log($"[FlowManager] Next device on cable {port.cable_guid} is {nextGuid}");

                    if (nextGuid == null)
                    {
                        Debug.LogError($"[FlowManager] No other device found for cable {port.cable_guid}");
                        return false;
                    }

                    currentDevice = NetworkManager.Instance.devices[nextGuid];

                    // 🔥 check ปลายทาง
                    if (DeviceHasIP(currentDevice, destIP))
                    {
                        //flow.destinationIP = currentDevice.guid;
                        return true;
                    }

                    break;
                }
            }

            //Check Routing Table
            if (foundSubnetPath)
            continue;

            // =============================
            // 🔴 2. ถ้าไม่เจอ → ใช้ routing table
            // =============================
            var defaultGateWayPort = currentDevice.ports[0];

            if (string.IsNullOrEmpty(defaultGateWayPort.cable_guid))
            {
                Debug.LogError("Default gateway port has no cable");
                return false;
            }

            // 🔥 หา device ถัดไป (router)
            string gatewayGuid = CableManager.Instance.GetOtherDeviceGuid(
                defaultGateWayPort.cable_guid,
                currentDevice.guid
            );

            if (string.IsNullOrEmpty(gatewayGuid))
            {
                Debug.LogError("Cannot find next device from default gateway");
                return false;
            }

            // 🔥 ใช้ routing table ของ "router"
            destIP = destIP.Split("/")[0];
            var route = RoutingTableManager.Instance.FindBestRoute(gatewayGuid, destIP);

            if (route == null)
            {
                Debug.LogError($"[Flow] No route found on router {gatewayGuid}");
                return false;
            }

            Debug.Log($"[Flow] Using route {route.networkCIDR} via port {route.portIndex}");

            // 🔥 หา port ตาม route
            if (route.portIndex < 0 || route.portIndex >= currentDevice.ports.Count)
            {
                Debug.LogError("Invalid port index from route");
                return false;
            }

            //var selectedPort = currentDevice.ports[route.portIndex];
            var gatewayCableGuid = NetworkManager.Instance.devices[gatewayGuid].ports[route.portIndex].cable_guid;
            if (string.IsNullOrEmpty(gatewayCableGuid))
            {
                Debug.LogError("Route port has no cable");
                return false;
            }

            // 🔥 เพิ่ม load
            ApplyLoad(gatewayCableGuid, flow);
            flow.cablePath.Add(gatewayCableGuid);

            // 🔥 ไป device ถัดไป
            string nextDeviceGuid = CableManager.Instance.GetOtherDeviceGuid(
                gatewayCableGuid,
                gatewayGuid
            );

            if (nextDeviceGuid == null)
            {
                Debug.LogError("Next device not found from route");
                return false;
            }

            currentDevice = NetworkManager.Instance.devices[nextDeviceGuid];
            //End of using routing table
        }

        return false;
    }

    // 🔥 เช็คว่า device มี IP ปลายทางมั้ย
    private bool DeviceHasIP(DeviceNetworkState device, string destCIDR)
    {
        foreach (var port in device.ports)
        {
            if (string.IsNullOrEmpty(port.my_ip))
                continue;

            // 🔥 เช็ค subnet แทนเทียบตรง ๆ
            if (NetworkUtils.IsSameSubnet(port.my_ip, destCIDR))
                return true;
        }
        return false;
    }

    // 🔌 เพิ่ม load
    private void ApplyLoad(string cableGuid, NetworkFlow flow)
    {
        if (CableManager.Instance.cableDict.TryGetValue(cableGuid, out var cable))
        {
            float load = flow.BytesPerSecond * 0.1f;
            Debug.Log($"Bytes this tick: {load} and flow.BytesPerSecond: {flow.BytesPerSecond} bytes on cable {cableGuid}");
            cable.ProcessFlow(load, 0.1f);
        }
    }

    private bool DeviceHasExactIP(DeviceNetworkState device, string destCIDR)
    {
        string destIPOnly = destCIDR.Split('/')[0];

        foreach (var port in device.ports)
        {
            if (string.IsNullOrEmpty(port.my_ip))
                continue;

            string portIPOnly = port.my_ip.Contains("/") 
                ? port.my_ip.Split('/')[0] 
                : port.my_ip;

            if (portIPOnly == destIPOnly)
                return true;
        }

        return false;
    }
}