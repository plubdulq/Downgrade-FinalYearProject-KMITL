
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Chomm.DebugTools
{
    public class VRControllerInputDebugger : MonoBehaviour
    {
        private List<InputDevice> devices = new List<InputDevice>();
        private Dictionary<string, bool> buttonStates = new Dictionary<string, bool>();

        private void OnEnable()
        {
            InputDevices.deviceConnected += OnDeviceConnected;
            InputDevices.deviceDisconnected += OnDeviceDisconnected;
            
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
            InputDevices.deviceDisconnected -= OnDeviceDisconnected;
            devices.Clear();
        }

        private void OnDeviceConnected(InputDevice device)
        {
            if ((device.characteristics & InputDeviceCharacteristics.Controller) != 0)
            {
                if (!devices.Contains(device))
                {
                    devices.Add(device);
                    Debug.Log($"[InputDebugger] Device Connected: {device.name} - {device.role}");
                }
            }
        }

        private void OnDeviceDisconnected(InputDevice device)
        {
            if (devices.Contains(device))
            {
                devices.Remove(device);
                Debug.Log($"[InputDebugger] Device Disconnected: {device.name}");
            }
        }

        private void Update()
        {
            foreach (var device in devices)
            {
                CheckButton(device, CommonUsages.triggerButton, "Trigger");
                CheckButton(device, CommonUsages.gripButton, "Grip");
                CheckButton(device, CommonUsages.primaryButton, "Primary (A/X)");
                CheckButton(device, CommonUsages.secondaryButton, "Secondary (B/Y)");
                CheckButton(device, CommonUsages.primary2DAxisClick, "Joystick Click");
                CheckButton(device, CommonUsages.menuButton, "Menu Button");
            }
        }

        private void CheckButton(InputDevice device, InputFeatureUsage<bool> usage, string buttonName)
        {
            if (device.TryGetFeatureValue(usage, out bool isPressed))
            {
                string key = $"{device.name}_{device.role}_{buttonName}";
                
                // Initialize state if not present
                if (!buttonStates.ContainsKey(key))
                {
                    buttonStates[key] = false;
                }

                // Check for state change
                if (isPressed && !buttonStates[key])
                {
                    Debug.Log($"<color=green>[PRESS]</color> {device.role} : {buttonName}");
                    buttonStates[key] = true;
                }
                else if (!isPressed && buttonStates[key])
                {
                    Debug.Log($"<color=red>[RELEASE]</color> {device.role} : {buttonName}");
                    buttonStates[key] = false;
                }
            }
        }
    }
}
