using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

namespace Chomm.DebugTools
{
    public class VRInputDebugger : MonoBehaviour
    {
        public bool logTrigger = true;
        public bool logGrip = true;
        public bool logPrimaryButton = true;
        public bool logSecondaryButton = true;

        private List<InputDevice> devices = new List<InputDevice>();

        private void OnEnable()
        {
            InputDevices.deviceConnected += OnDeviceConnected;
            List<InputDevice> currentDevices = new List<InputDevice>();
            InputDevices.GetDevices(currentDevices);
            foreach (var device in currentDevices)
            {
                OnDeviceConnected(device);
            }
        }

        private void OnDisable()
        {
            InputDevices.deviceConnected -= OnDeviceConnected;
            devices.Clear();
        }

        private void OnDeviceConnected(InputDevice device)
        {
            if (!devices.Contains(device) && (device.characteristics & InputDeviceCharacteristics.Controller) != 0)
            {
                devices.Add(device);
                Debug.Log($"[VRDebug] Device Connected: {device.name} ({device.role})");
            }
        }

        private void Update()
        {
            foreach (var device in devices)
            {
                CheckButton(device, CommonUsages.triggerButton, "Trigger", logTrigger);
                CheckButton(device, CommonUsages.gripButton, "Grip", logGrip);
                CheckButton(device, CommonUsages.primaryButton, "Primary Button (A/X)", logPrimaryButton);
                CheckButton(device, CommonUsages.secondaryButton, "Secondary Button (B/Y)", logSecondaryButton);
            }
        }

        // Simple state tracking to log only on 'Down'
        private Dictionary<string, bool> buttonStates = new Dictionary<string, bool>();

        private void CheckButton(InputDevice device, InputFeatureUsage<bool> usage, string buttonName, bool log)
        {
            if (!log) return;

            string key = $"{device.name}_{buttonName}";
            if (!buttonStates.ContainsKey(key)) buttonStates[key] = false;

            if (device.TryGetFeatureValue(usage, out bool pressed))
            {
                if (pressed && !buttonStates[key])
                {
                    Debug.Log($"[VRDebug] {device.role} {buttonName} PRESSED");
                    buttonStates[key] = true;
                }
                else if (!pressed && buttonStates[key])
                {
                    Debug.Log($"[VRDebug] {device.role} {buttonName} RELEASED");
                    buttonStates[key] = false;
                }
            }
        }
    }
}
