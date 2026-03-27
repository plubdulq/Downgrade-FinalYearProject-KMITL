using UnityEngine;

namespace ByteSize.KnobsXR
{
    [AddComponentMenu("Byte Size/Knobs XR/Knobs XR ValueProvider")]
    [RequireComponent(typeof(KnobsXRInteractable), typeof(RotationDelta))]
    public class KnobsXRValueProvider : MonoBehaviour
    {
        public float KnobValue { get; private set; }
        public float MinValue => this.knobInteractable.MinRotationLimit;
        public float MaxValue => this.knobInteractable.MaxRotationLimit;

        [SerializeField]
        private bool invertValue;

        private float rotationAlongConstraint => Vector3.Dot(this.knobInteractable.ConstraintAxis.normalized, this.rotationDelta.Direction + this.transform.position);
        private float rotationSign => Mathf.Sign(rotationAlongConstraint);
        
        private RotationDelta rotationDelta;
        private KnobsXRInteractable knobInteractable;

        private void Awake()
        {
            this.rotationDelta = GetComponent<RotationDelta>();
            this.knobInteractable = GetComponent<KnobsXRInteractable>();
        }

        private void Start()
        {
            this.rotationDelta.SetInitialRotation(this.transform.rotation);
        }

        private void Update()
        {
            this.rotationDelta.UpdateRotation(this.transform.rotation);
            UpdateKnobValue();
        }

        private void UpdateKnobValue()
        {
            float signedRotationAngle = this.rotationDelta.Angle * this.rotationSign;
            float invertableRotationAngle = this.invertValue ? -1 * signedRotationAngle : signedRotationAngle;

            this.KnobValue = invertableRotationAngle;
        }
    }
}
