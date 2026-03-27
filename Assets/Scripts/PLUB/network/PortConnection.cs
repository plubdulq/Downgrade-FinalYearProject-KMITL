using System.Collections.Generic;

[System.Serializable]
public class PortConnection
{
    public string my_ip;
    public string targetGuid;
    public int targetPort;

    public PortConnection(string guid, int port)
    {
        targetGuid = guid;
        targetPort = port;
    }
}