using System;

[System.Serializable]
public class FirewallRule
{
    public string sourceIP;
    public string destinationIP;

    public string port;      
    public string protocol;  

    public string action;    

    public FirewallRule(string source, string destination, string port, string protocol, string action)
    {
        this.sourceIP = source;
        this.destinationIP = destination;
        this.port = port;
        this.protocol = protocol;
        this.action = action;
    }
}