using UnityEngine;
using TMPro;

public class PacketUIController : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_Text sourceInput;
    public TMP_InputField destinationInput;
    public TMP_Text payloadInput;   // MB
    public TMP_Text rateInput;      // packets/sec

    [Header("Optional Debug")]
    public bool autoNormalize = true;

    // 🎯 ผูกกับปุ่ม Apply
    public void OnClickApply()
    {
        // 🔍 อ่านค่าจาก UI
        EquipmentData device = GetComponentInParent<EquipmentData>();
        string deviceGuid = device.uniqueID;
        string sourceIP = sourceInput.text.Trim();
        string destIP   = destinationInput.text.Trim();

        if (string.IsNullOrEmpty(sourceIP) || string.IsNullOrEmpty(destIP))
        {
            Debug.LogError("Source or Destination is empty");
            return;
        }

        // 🔧 normalize (สำคัญ)
        if (autoNormalize)
        {
            sourceIP = NetworkUtils.NormalizeToCIDR(sourceIP);
            destIP   = NetworkUtils.NormalizeToCIDR(destIP);
        }

        // 🔢 parse payload
        if (!float.TryParse(payloadInput.text, out float payloadMB))
        {
            Debug.LogError("Invalid payload input");
            return;
        }

        // 🔢 parse rate
        if (!float.TryParse(rateInput.text, out float rate))
        {
            Debug.LogError("Invalid rate input");
            return;
        }

        // 🔥 แปลง MB → Bytes
        float payloadBytes = payloadMB * 1024f * 1024f;

        // 🚀 เรียก FlowManager
        FlowManager.Instance.CreateAndRegisterFlow(
            deviceGuid,
            sourceIP,
            destIP,
            payloadBytes,
            rate
        );

        Debug.Log($"[UI] Flow Request: {sourceIP} -> {destIP}");
    }
}