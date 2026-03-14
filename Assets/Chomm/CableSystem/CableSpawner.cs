using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // For XRGrabInteractable if needed, though not used in this file directly? 
// Wait, CableSpawner DOES use Instantiate but doesn't reference Interactables directly except for maybe when it was creating loose plugs?
// Ah, the file content of CableSpawner.cs I have in memory (Step 75) uses:
// using UnityEngine;
// using UnityEngine.XR;
// using UnityEngine.XR.Interaction.Toolkit;
// using UnityEngine.XR.Interaction.Toolkit.Interactables;
// using System.Collections.Generic;

using BNG; // Added

namespace Chomm.CableSystem
{
    public class CableSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        public GameObject cablePrefab;
        public float spawnDistance = 0.5f;
        public float spawnHeight = 1.0f;

        [Header("Input Settings")]
        public Transform leftHandTransform;
        public float rotationThreshold = 160f; // Detect if Y is > 160 (close to 180)
        public float cooldownTime = 1.0f;
        
        private float lastSpawnTime;
        private bool wasInPosition = false;

        private void Update()
        {
            // Gesture: Left Hand Y Rotation ~ 180 OR Thumb Button Pressed
            if (leftHandTransform != null)
            {
                // 1. Rotation Check
                float yAngle = leftHandTransform.localEulerAngles.y;
                bool isRotationCorrect = (yAngle > rotationThreshold && yAngle < (360 - rotationThreshold));

                // 2. Input Button Check (Primary Button / Thumb Button) using BNG
                // On Oculus/Quest: X is Left Primary.
                bool isButtonPressed = InputBridge.Instance.XButton;

                if (isRotationCorrect || isButtonPressed)
                {
                    // Debug Log
                    Debug.Log($"[CableSpawner] Trigger Active! Rotation: {yAngle:F1} (Valid: {isRotationCorrect}), Button: {isButtonPressed}");

                    if (!wasInPosition && Time.time > lastSpawnTime + cooldownTime)
                    {
                        SpawnCable();
                        lastSpawnTime = Time.time;
                    }
                    wasInPosition = true;
                }
                else
                {
                    // Reset state only if BOTH are false (which is the case if we are in this else block)
                    wasInPosition = false;
                }
            }
            else
            {
                bool isButtonPressed = InputBridge.Instance.XButton;
                if (isButtonPressed && leftHandTransform ==
                    null)
                {
                    leftHandTransform = GameObject.Find("LeftHandAnchor").transform;
                }
            }
        }



        public void SpawnCable()
        {
            if (cablePrefab == null)
            {
                Debug.LogWarning("[CableSpawner] No Cable Prefab assigned!");
                return;
            }

            Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * spawnDistance;
            spawnPos.y = Mathf.Max(spawnPos.y, spawnHeight);

            GameObject newCable = Instantiate(cablePrefab, spawnPos, Quaternion.identity);
            
            // Optional: If we need to "kick" the plugs apart so they aren't inside each other
            Cable cable = newCable.GetComponent<Cable>();
            if (cable != null)
            {
                if (cable.PlugA != null) cable.PlugA.transform.localPosition = Vector3.left * 0.2f;
                if (cable.PlugB != null) cable.PlugB.transform.localPosition = Vector3.right * 0.2f;
            }

            Debug.Log("[CableSpawner] Spawned Cable");
        }
    }
}
