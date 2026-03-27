using UnityEngine;

namespace ByteSize.KnobsXR
{
    [System.Serializable]
    public class KnobInitialDirections
    {
        public Vector3 InitialForward => this.initialForward;
        public Vector3 InitialUp => this.initialUp;

        [SerializeField]
        private Vector3 initialForward;
        
        [SerializeField]
        private Vector3 initialUp;

        public KnobInitialDirections(Transform transform)
        {
            this.initialForward = transform.forward;
            this.initialUp = transform.up;
        }
    }
}
