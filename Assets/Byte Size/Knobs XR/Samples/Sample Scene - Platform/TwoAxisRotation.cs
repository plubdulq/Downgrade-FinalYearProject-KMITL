using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ByteSize.KnobsXR.Samples
{
    [AddComponentMenu("Byte Size/Knobs XR/Samples/Two Axis Rotation")]
    public class TwoAxisRotation : MonoBehaviour
    {
        [SerializeField]
        private KnobsXRValueProvider frontToBackKnob;

        [SerializeField]
        private KnobsXRValueProvider sideToSideKnob;

        private Vector3 frontToBackRotationAxis;
        private Vector3 sideToSideRotationAxis;
        private Quaternion initialRotation;

        
        private void Start()
        {
            SetInitialRotationValues();
        }

        private void Update()
        {
            this.transform.rotation = Quaternion.AngleAxis(this.frontToBackKnob.KnobValue, this.frontToBackRotationAxis)
            * Quaternion.AngleAxis(this.sideToSideKnob.KnobValue, this.sideToSideRotationAxis) * this.initialRotation;
        }

        private void SetInitialRotationValues()
        {
            this.frontToBackRotationAxis = this.transform.right;
            this.sideToSideRotationAxis = this.transform.forward;
            this.initialRotation = this.transform.rotation;
        }
    }
}
