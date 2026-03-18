using System.Threading.Tasks;
using UnityEngine;

public class NetworkDevice : MonoBehaviour
{
    public string deviceType;
    public string guid;

    private QueryPortSchema repository;

    void Start()
    {
        repository = new QueryPortSchema();
        RegisterDevice();
    }

    async Task RegisterDevice()
    {

        EquipmentData device = GetComponent<EquipmentData>();
        guid = device.uniqueID;

        // query DB
        PortSchemaDB schema = await repository.GetPortSchema(deviceType);
        
        // create state
        DeviceNetworkState state = new DeviceNetworkState(guid);

        int portIndex = 0;
        foreach (var portType in schema.ports_schema)
        {
            //string type = portType.Key;
            PortSchema data = portType.Value;

            for (int i = 0; i < data.count; i++)
            {
                state.ports.Add(
                    new PortState(
                        portIndex++,
                        //type,
                        data.connector,
                        data.speed
                    )
                );
            }
        }

        // register
        NetworkManager.Instance.RegisterDevice(state);
    }
}