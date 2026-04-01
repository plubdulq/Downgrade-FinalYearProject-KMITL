using System.Threading.Tasks;
using UnityEngine;

public class NetworkDevice : MonoBehaviour
{
    public string device_type;
    public string device_id;
    public string guid;

    private QueryPortSchema repository;

    void Start()
    {
        repository = new QueryPortSchema();
        EquipmentData device = GetComponent<EquipmentData>();
        Debug.Log("DDDDDDdddddddddd");
        if (device == null)
        {
            Debug.Log($"testttttttttttt");
        }
        guid = device.uniqueID;
        //DeviceMapManager.Instance.Register(guid, this.gameObject);
        RegisterDevice();
    }

    async Task RegisterDevice()
    {

        // EquipmentData device = GetComponent<EquipmentData>();
        // guid = device.uniqueID;

        // query DB
        PortSchemaDB schema = await repository.GetPortSchema(device_id);
        
        // create state
        DeviceNetworkState state = new DeviceNetworkState(guid, device_type);

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