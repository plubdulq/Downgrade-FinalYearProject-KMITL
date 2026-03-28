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

        [Header("UI")]
        public TMP_Text roomText;
        public TMP_Text frontText;
        public TMP_Text rearText;
        public TMP_Text statusText;
        public TMP_Text TempToUpdate;
        public TMP_Text FanToUpdate;

        [Header("Control")]
        [Range(0, 100)] public int setTempPercent = 50;
        [Range(0, 100)] public int fanSpeedPercent = 40;

        public bool powerOn = false;
        public GameObject waypointGroup;

        [Header("Auto Bind")]
        public bool autoBindUI = true;
        public bool autoBindWaypointGroup = true;
        public bool debugLogs = true;

        private List<ServerState> servers = new List<ServerState>();
        private int totalDevices = 0;
        private int closedPanels = 0;

        float frontTemp = 24f;
        float rearTemp = 32f;
        float roomTemp = 28f;
        float ambientTemp = 32f;

        void Awake()
        {
            Instance = this;
            AutoBind();
        }

        void Start()
        {
            UpdateControlDisplay();
            UpdateDisplay();

            if (waypointGroup != null)
                waypointGroup.SetActive(powerOn);
        }

        void OnValidate()
        {
            if (!Application.isPlaying)
                AutoBind();
        }

        void AutoBind()
        {
            if (autoBindUI)
            {
                if (roomText == null) roomText = FindTMPByName("room");
                if (frontText == null) frontText = FindTMPByName("front");
                if (rearText == null) rearText = FindTMPByName("rear");
                if (statusText == null) statusText = FindTMPByName("status");
                if (TempToUpdate == null) TempToUpdate = FindTMPByName("temp");
                if (FanToUpdate == null) FanToUpdate = FindTMPByName("fan");
            }

            if (autoBindWaypointGroup && waypointGroup == null)
            {
                waypointGroup = FindChildGameObjectContains(transform, "waypoint");
            }
        }

        TMP_Text FindTMPByName(string key)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in texts)
            {
                if (t != null && t.name.ToLower().Contains(key.ToLower()))
                    return t;
            }
            return null;
        }

        GameObject FindChildGameObjectContains(Transform root, string key)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.ToLower().Contains(key.ToLower()))
                    return child.gameObject;
            }
            return null;
        }

        void Update()
        {
            Simulate();
            UpdateDisplay();
        }

        public void SetServers(List<ServerState> list)
        {
            servers = list ?? new List<ServerState>();
            RecalculateFromServers();
        }

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

            if (debugLogs)
                Debug.Log($"Devices: {totalDevices} | Panels Closed: {closedPanels}");
        }

        void Simulate()
        {
            float dt = Time.deltaTime;

            float targetTemp = Mathf.Lerp(18f, 30f, setTempPercent / 100f);
            float airflow = Mathf.Lerp(0.2f, 1.2f, fanSpeedPercent / 100f);

            float heat = totalDevices * 0.5f;

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

        void UpdateDisplay()
        {
            if (frontText) frontText.text = $"Front : {frontTemp:F1} °C";
            if (rearText) rearText.text = $"Rear : {rearTemp:F1} °C";
            if (roomText) roomText.text = $"Room : {roomTemp:F1} °C";

            if (statusText)
                statusText.text = powerOn ? "PAC ON" : "PAC OFF";
        }

        void UpdateControlDisplay()
        {
            if (FanToUpdate)
                FanToUpdate.text = "Fan : " + fanSpeedPercent + " %";

            if (TempToUpdate)
                TempToUpdate.text = "Set Temp : " +
                    Mathf.Lerp(18f, 30f, setTempPercent / 100f).ToString("F0") + " °C";
        }

        public void OnTempSlider(float v)
        {
            setTempPercent = Mathf.RoundToInt(v);
            UpdateControlDisplay();

            float target = Mathf.Lerp(18f, 30f, setTempPercent / 100f);
            frontTemp = Mathf.Lerp(frontTemp, target, 0.5f);
        }

        public void OnFanSlider(float v)
        {
            fanSpeedPercent = Mathf.RoundToInt(v);
            UpdateControlDisplay();

            rearTemp -= fanSpeedPercent * 0.1f;
        }

        public void TogglePower()
        {
            powerOn = !powerOn;

            if (statusText)
                statusText.text = powerOn ? "PAC ON" : "PAC OFF";

            if (waypointGroup != null)
                waypointGroup.SetActive(powerOn);
        }
    }
}