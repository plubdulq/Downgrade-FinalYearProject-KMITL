using UnityEngine;

namespace  ByteSize.KnobsXR
{
    /// <summary>
    /// Gives you rotation data from an initial rotation value
    /// </summary>
    [AddComponentMenu("Byte Size/Knobs XR/Rotation Delta")]
    public class RotationDelta : MonoBehaviour
    {
        public Vector3 Direction
        {
            get { return this.rotationDeltaDirection; }
            private set { this.rotationDeltaDirection = value; }
        }

        public float Angle
        {
            get { return this.rotationDeltaAngle; }
            private set { this.rotationDeltaAngle = value; }
        }

        private Quaternion initialRotation;
        private Quaternion rotationDelta;
        private float rotationDeltaAngle;
        private Vector3 rotationDeltaDirection;

        public void SetInitialRotation(Quaternion initialRotation)
        {
            this.initialRotation = initialRotation;
        }

        public void UpdateRotation(Quaternion rotation)
        {
            this.rotationDelta = rotation * Quaternion.Inverse(this.initialRotation);
            this.rotationDelta.ToAngleAxis(out this.rotationDeltaAngle, out this.rotationDeltaDirection);
        }

        public Quaternion GetTotalRotation()
        {
            return this.rotationDelta * this.initialRotation;
        }
    }
}
