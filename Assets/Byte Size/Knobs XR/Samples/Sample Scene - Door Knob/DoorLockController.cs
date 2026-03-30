using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ByteSize.KnobsXR.Samples
{
    [RequireComponent(typeof(KnobsXRValueProvider))]
    public class DoorLockController : MonoBehaviour
    {
        public UnityEvent OnDoorUnlocked;

        private const float doorUnlockValue = 85f;

        private bool hasEventDoorUnlockedFired = false;

        private KnobsXRValueProvider knobValueProvider;

        private void Awake()
        {
            this.knobValueProvider = GetComponent<KnobsXRValueProvider>();
        }

        private void Update()
        {
            if (this.knobValueProvider.KnobValue < doorUnlockValue) { return; }

            if (this.hasEventDoorUnlockedFired) { return; }

            this.OnDoorUnlocked?.Invoke();
            this.hasEventDoorUnlockedFired = true;
        }
    }
}
