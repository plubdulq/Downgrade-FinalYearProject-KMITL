using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class LabelCardController : MonoBehaviour
{
    public GameObject labelCardPrefab; // prefab ของคุณ
    public Transform container;        // parent (Frame / Panel)

    [SerializeField] private DeviceNetworkState device;
    private NetworkDevice networkDevice;

    public void Awake()
    {
        networkDevice = GetComponentInParent<NetworkDevice>();
    }

    public void GenerateCards()
    {
        device = NetworkManager.Instance.GetDeviceByGuid(networkDevice.guid);
        
        // 🔥 ลบของเก่าก่อน (กัน spawn ซ้ำ)
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // 🔢 วนตามจำนวน port
        int portCount = device.ports.Count; // หรือ device.ports.Count แล้วแต่ structure

        for (int i = 0; i < portCount; i++)
        {
            // ✅ สร้าง instance
            GameObject cardObj = Instantiate(labelCardPrefab, container);

            // ✅ ดึง script ที่คุณเขียนไว้
            var card = cardObj.GetComponent<ALabelInfo>(); // <- เปลี่ยนชื่อให้ตรง
            

            // ✅ set data
            card.device = device;
            card.portIndex = i; // สำคัญมาก (จะได้รู้ว่าเป็น port ไหน)
           
            card.SetCardConnectionInfo();
            
        }
    }
}