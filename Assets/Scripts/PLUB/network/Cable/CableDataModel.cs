using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CableDataModel
{
    public string guid;
    public string connectedDeviceGuid1;
    public string connectedDeviceGuid2;

     // 🔥 simulation queue and drop
    public float capacityBps = 10000f;
    public float maxQueueBytes = 5000f;

    private float queueBytes = 0f;
    public float droppedBytes = 0f;

    public float CurrentLoad => queueBytes;

    public void ProcessFlow(float incomingBytes, float deltaTime)
    {
        queueBytes += incomingBytes;

        // DROP
        if (queueBytes > maxQueueBytes)
        {
            float overflow = queueBytes - maxQueueBytes;
            droppedBytes += overflow;
            queueBytes = maxQueueBytes;
            Debug.Log($"[DROP] {overflow} bytes on cable {guid}");
        }
        Debug.Log($"[NODROP] [CABLE] {guid} - Queue: {queueBytes} bytes, Dropped: {droppedBytes} bytes");
        // SEND
        float canSend = capacityBps * deltaTime;
        float sent = Mathf.Min(queueBytes, canSend);

        queueBytes -= sent;
    }

}
