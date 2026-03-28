using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace ByteSize.KnobsXR
{
    [AddComponentMenu("Byte Size/Knobs XR/Knobs XR Interactable")]
    public class KnobsXRInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
    {
        public Vector3 ConstraintAxis { get; private set; }
        public float MinRotationLimit => knobLimits.Min;
        public float MaxRotationLimit => knobLimits.Max;

        [SerializeField, HideInInspector]
        private KnobInitialDirections initialDirections;

        [SerializeField, HideInInspector]
        private KnobLimits knobLimits = new KnobLimits();

        private Quaternion rotationBeforeGrab;
        private Quaternion interactorRotationOnGrab;

        protected void Start()
        {
            rotationBeforeGrab = transform.rotation;
            ConstraintAxis = transform.forward;
            initialDirections = new KnobInitialDirections(transform);
            knobLimits.SetInitialRotation(transform.rotation);
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);

            var interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor;
            if (interactor != null)
            {
                var attach = interactor.GetAttachTransform(this);
                interactorRotationOnGrab = attach.rotation;
            }
        }

        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            base.OnSelectExiting(args);
            rotationBeforeGrab = transform.rotation;
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                ProcessUpdate();
        }

        private void ProcessUpdate()
        {
            if (!isSelected) return;
            PerformInstantaneousUpdate();
        }

        private void PerformInstantaneousUpdate()
        {
            var interactor = firstInteractorSelecting as UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor;
            if (interactor == null) return;

            var attach = interactor.GetAttachTransform(this);

            Quaternion interactorDelta =
                attach.rotation *
                Quaternion.Inverse(interactorRotationOnGrab);

            interactorDelta.ToAngleAxis(out float angle, out Vector3 axis);

            float rotationAngle =
                Vector3.Dot(axis, ConstraintAxis) * angle;

            Quaternion rotationAttempt =
                Quaternion.AngleAxis(rotationAngle, ConstraintAxis) *
                rotationBeforeGrab;

            if (knobLimits.UseLimitsAndRotationNotWithinLimits(rotationAttempt, ConstraintAxis))
                return;

            transform.rotation = rotationAttempt;
        }
    }
}