using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BNG
{
    public enum RackLayerType
    {
        Empty,
        RackDeviceLayer,
        PanelBlock
    }
    public class ServerRoomTempSimulation : MonoBehaviour
    {
        public static ServerRoomTempSimulation Instance;

        void Awake()
        {
            Instance = this;
        }

        [Header("UI")]
        public TMP_Text roomText;
        public TMP_Text frontText;
        public TMP_Text rearText;
        public TMP_Text statusText;

        [Header("Control")]
        [Range(0, 100)] public int setTempPercent = 50;
        [Range(0, 100)] public int fanSpeedPercent = 40;

        public bool powerOn = false;
        public GameObject waypointGroup;

        // 🔥 Server Data
        private List<ServerState> servers = new List<ServerState>();
        private int totalDevices = 0;
        private int closedPanels = 0;

        // Temp
        // Temp
        float frontTemp = 24f;
        float rearTemp = 32f;
        float roomTemp = 28f;
        float ambientTemp = 32f;

        public float RoomTemp => roomTemp;

        void Update()
        {
            Simulate();
            UpdateDisplay();
        }

        // =========================
        // 🔥 รับ Server จาก Detector
        // =========================
        public void SetServers(List<ServerState> list)
        {
            servers = list;
            RecalculateFromServers();
        }

        // =========================
        // 🔥 คำนวณจำนวน Device / Panel
        // =========================
        public void RecalculateFromServers()
        {
            int deviceCount = 0;
            int panelCount = 0;

            foreach (var s in servers)
            {
                if (s == null) continue;

                if (s.hasDevice) deviceCount++;
                if (s.isPanelClosed) panelCount++;
            }

            totalDevices = deviceCount;
            closedPanels = panelCount;

            Debug.Log($"Devices: {totalDevices} | Panels Closed: {closedPanels}");
        }

        // =========================
        // 🔥 Simulation
        // =========================
        void Simulate()
        {
            float dt = Time.deltaTime;

            float targetTemp = Mathf.Lerp(18f, 30f, setTempPercent / 100f);
            float airflow = Mathf.Lerp(0.2f, 1.2f, fanSpeedPercent / 100f);

            float heat = totalDevices * 0.5f;

            // 🔥 ปิดฝา → เย็นขึ้น
            float panelFactor = 1f - (closedPanels * 0.02f);
            panelFactor = Mathf.Clamp(panelFactor, 0.5f, 1f);

            heat *= panelFactor;

            if (!powerOn)
            {
                frontTemp = Mathf.Lerp(frontTemp, ambientTemp, dt * 0.5f);
                rearTemp = Mathf.Lerp(rearTemp, ambientTemp, dt * 0.5f);
                roomTemp = Mathf.Lerp(roomTemp, ambientTemp, dt * 0.5f);
                return;
            }

            rearTemp += heat * dt * 2f;
            frontTemp += (targetTemp - frontTemp) * airflow * dt;

            roomTemp += (rearTemp - roomTemp) * airflow * dt;
            frontTemp += (roomTemp - frontTemp) * airflow * dt;

            rearTemp += (frontTemp - rearTemp) * 0.1f * dt;

            frontTemp = Mathf.Clamp(frontTemp, 15f, 60f);
            rearTemp = Mathf.Clamp(rearTemp, 15f, 80f);
            roomTemp = Mathf.Clamp(roomTemp, 15f, 60f);
        }

        // =========================
        // 📊 UI
        // =========================
        void UpdateDisplay()
        {
            if (frontText) frontText.text = $"Front : {frontTemp:F1} °C";
            if (rearText) rearText.text = $"Rear : {rearTemp:F1} °C";
            if (roomText) roomText.text = $"Room : {roomTemp:F1} °C";

            if (statusText)
                statusText.text = powerOn ? "PAC ON" : "PAC OFF";
        }

        // =========================
        // 🎛 UI Events
        // =========================
        public void OnTempSlider(float v)
        {
            setTempPercent = Mathf.RoundToInt(v);

            float target = Mathf.Lerp(18f, 30f, setTempPercent / 100f);
            frontTemp = Mathf.Lerp(frontTemp, target, 0.5f); // 🔥 เห็นผลทันที
        }

        public void OnFanSlider(float v)
        {
            fanSpeedPercent = Mathf.RoundToInt(v);

            rearTemp -= fanSpeedPercent * 0.1f; // 🔥 ลดร้อนทันที
        }

        public void TogglePower()
        {
            powerOn = !powerOn;

            waypointGroup.SetActive(true);
        }
    }
}