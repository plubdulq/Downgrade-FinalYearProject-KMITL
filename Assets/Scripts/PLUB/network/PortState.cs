[System.Serializable]
public class PortState
{
    public int portNumber;
    public string portType;
    public string speed;

    //ip and guid of the connected device
    public string my_ip;
    public string cable_guid;

    public PortState(int number, string connectorType, string Portspeed)
    {
        portNumber = number;
        portType = connectorType;
        speed = Portspeed;
    }
}