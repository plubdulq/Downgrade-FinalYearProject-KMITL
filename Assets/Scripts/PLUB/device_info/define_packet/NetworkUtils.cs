using System;
using System.Net;
using UnityEngine;
using System.Collections.Generic;

public static class NetworkUtils
{
    // ✅ NEW: normalize IP ให้เป็น CIDR
    public static string NormalizeToCIDR(string ip)
    {
        if (string.IsNullOrEmpty(ip))
            return ip;

        if (!ip.Contains("/"))
            return ip + "/24"; // default mask

        return ip;
    }


    // 🔥 เช็ค subnet จาก CIDR เช่น 192.168.1.1/24
    public static bool IsSameSubnet(string cidr1, string cidr2)
    {
        if (!TryParseCIDR(cidr1, out uint ip1, out int prefix1)) return false;
        if (!TryParseCIDR(cidr2, out uint ip2, out int prefix2)) return false;

        // ใช้ prefix ที่เล็กกว่า (strict กว่า)
        int prefix = Math.Min(prefix1, prefix2);

        uint mask = PrefixToMask(prefix);

        return (ip1 & mask) == (ip2 & mask);
    }

    // 🔥 เช็คว่า IP อยู่ใน subnet นี้มั้ย
    public static bool IsIPInSubnet(string ip, string cidr)
    {
        if (!TryParseCIDR(cidr, out uint networkIP, out int prefix))
        {
            Debug.Log($"CIDR parse failed: {cidr}");
            return false;
        }

        if (!IPAddress.TryParse(ip, out var ipAddr)) return false;

        uint ipUint = IPToUint(ipAddr);
        uint mask = PrefixToMask(prefix);

        return (ipUint & mask) == (networkIP & mask);
    }

    // 🔧 parse CIDR
    private static bool TryParseCIDR(string cidr, out uint ip, out int prefix)
    {
        ip = 0;
        prefix = 0;

        var parts = cidr.Split('/');
        if (parts.Length != 2) return false;

        if (!IPAddress.TryParse(parts[0], out var ipAddr)) return false;
        if (!int.TryParse(parts[1], out prefix)) return false;

        ip = IPToUint(ipAddr);
        return true;
    }

    private static uint PrefixToMask(int prefix)
    {
        return prefix == 0 ? 0 : uint.MaxValue << (32 - prefix);
    }

    private static uint IPToUint(IPAddress ip)
    {
        byte[] bytes = ip.GetAddressBytes();
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        return BitConverter.ToUInt32(bytes, 0);
    }

    public static List<string> FindReachableServers(string startDeviceGuid)
    {
        HashSet<string> resultSet = new HashSet<string>(); // 🔥 กัน duplicate
        List<string> result = new List<string>();

        Queue<string> queue = new Queue<string>();
        HashSet<string> visited = new HashSet<string>();

        queue.Enqueue(startDeviceGuid);
        visited.Add(startDeviceGuid);

        while (queue.Count > 0)
        {
            string current = queue.Dequeue();

            var neighbors = CableManager.Instance.GetConnectedDevicesGuid(current);

            foreach (var next in neighbors)
            {
                Debug.Log($"From {current} → {next}");
                if (visited.Contains(next))
                    continue;

                var device = NetworkManager.Instance.GetDeviceByGuid(next);
                if (device == null)
                    continue;

                // 🔥 CASE 1: Server → add + mark visited + STOP
                if (device.device_type == DeviceType.Server)
                {
                    if (resultSet.Add(next)) // ✅ add เฉพาะถ้ายังไม่มี
                    {
                        Debug.Log($"Found reachable server: {next}");
                        result.Add(next);
                    }

                    visited.Add(next); // 🔥 mark visited ด้วย
                    continue;
                }

                // ❌ กัน LB chain
                if (device.device_type == DeviceType.LoadBalancer)
                {
                    visited.Add(next);
                    continue;
                }

                // ✅ network device → traverse ต่อ
                if (device.device_type == DeviceType.Switch ||
                    device.device_type == DeviceType.Firewall ||
                    device.device_type == DeviceType.Router)
                {
                    queue.Enqueue(next);
                    visited.Add(next);
                }
            }
        }
        return result;
    }
}