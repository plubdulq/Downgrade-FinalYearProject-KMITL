[System.Serializable]
public class PortState
{
    public int portNumber;
    public string portType;
    public string speed;

    public PortConnection connection;

    public PortState(int number, string connectorType, string Portspeed)
    {
        portNumber = number;
        portType = connectorType;
        speed = Portspeed;
        connection = null;
    }
}