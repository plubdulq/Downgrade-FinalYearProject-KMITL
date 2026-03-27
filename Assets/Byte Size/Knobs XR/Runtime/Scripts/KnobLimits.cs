using UnityEngine;

namespace ByteSize.KnobsXR
{
    [System.Serializable]
    public class KnobLimits
    {
        public float Min => this.angleRange.x;
        public float Max => this.angleRange.y;

        [SerializeField]
        private bool useLimits;

        [SerializeField]
        private Vector2 angleRange;

        private Quaternion initialRotation;

        public void SetInitialRotation(Quaternion rotation)
        {
            this.initialRotation = rotation;
        }

        public bool UseLimitsAndRotationNotWithinLimits(Quaternion rotation, Vector3 constraint)
        {
            return this.useLimits && !IsRotationWithinLimits(rotation, constraint);
        }

        public bool IsRotationWithinLimits(Quaternion rotation, Vector3 constraint)
        {
            Quaternion rotationDelta = rotation * Quaternion.Inverse(this.initialRotation);

            float angleDelta = 0f;
            Vector3 rotationDirection;
            rotationDelta.ToAngleAxis(out angleDelta, out rotationDirection);

            float amountAlongConstraint = Vector3.Dot(constraint.normalized, rotationDirection.normalized);
            float angleSign = Mathf.Sign(amountAlongConstraint);

            return IsAngleWithinLimits(angleSign * angleDelta);
        }

        private bool IsAngleWithinLimits(float angle)
        {
            return angle >= this.angleRange.x && angle <= this.angleRange.y;
        }
    }
}
