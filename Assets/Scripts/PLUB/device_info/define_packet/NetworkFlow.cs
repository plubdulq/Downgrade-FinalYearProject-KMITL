using System;
using System.Collections.Generic;

[System.Serializable]
public class NetworkFlow
{
    public string flowGuid;

    // 🔥 เปลี่ยนเป็น IP
    public string sourceIP;
    public string destinationIP;

    public float payloadSize;
    public float packetRate;

    public float BytesPerSecond => payloadSize * packetRate;

    public List<string> cablePath = new List<string>();

    public NetworkFlow()
    {
        flowGuid = Guid.NewGuid().ToString();
    }
}