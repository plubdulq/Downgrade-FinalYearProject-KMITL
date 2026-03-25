using System;
using System.Net;
using UnityEngine;

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
}