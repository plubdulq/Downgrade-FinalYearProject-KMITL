using UnityEngine;
using TMPro;

namespace BNG
{
    public class TempUpdate : MonoBehaviour
    {
        public TMP_Text RoomTempUpdate;
        public ServerRoomTempSimulation sim;

        void Update()
        {
            if (RoomTempUpdate == null || sim == null) return;

            RoomTempUpdate.text = sim.RoomTemp.ToString("F1") + " °C";
        }
    }
}