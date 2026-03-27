using UnityEngine;
using UnityEngine.UI;

namespace ByteSize.KnobsXR.Samples
{
    [AddComponentMenu("Byte Size/Knobs XR/Samples/Radio Volume Display")]
    public class RadioVolumeDisplay : MonoBehaviour
    {
        [SerializeField]
        private Image radioVolumeImage;

        [SerializeField]
        private AudioSource radioAudioSource;

        [SerializeField]
        private Canvas radioCanvas;

        private float radioCanvasWidth;

        private void Awake()
        {
            this.radioCanvasWidth = (this.radioCanvas.transform as RectTransform).rect.width;
        }

        private void Update()
        {
            float volumeToImageWidth = Mathf.Lerp(0.25f, this.radioCanvasWidth, this.radioAudioSource.volume);
            this.radioVolumeImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, volumeToImageWidth);
        }
    }
}
