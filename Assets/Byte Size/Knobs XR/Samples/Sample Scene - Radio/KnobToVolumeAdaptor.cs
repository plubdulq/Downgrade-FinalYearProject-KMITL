using UnityEngine;

namespace ByteSize.KnobsXR.Samples
{
    [AddComponentMenu("Byte Size/Knobs XR/Samples/KnobToVolumeAdaptor")]
    public class KnobToVolumeAdaptor : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private KnobsXRValueProvider knobValueProvider;

        private void Update()
        {
            this.audioSource.volume = Mathf.InverseLerp(0f, this.knobValueProvider.MaxValue, this.knobValueProvider.KnobValue);
        }
    }
}
