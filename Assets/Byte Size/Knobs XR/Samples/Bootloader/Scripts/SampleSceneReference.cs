using System;
using UnityEngine;
using UnityEngine.UI;

namespace ByteSize.XRKnobs.Samples
{
    [RequireComponent(typeof(Button))]
    public class SampleSceneReference : MonoBehaviour
    {
        public event Action<string> OnSampleSceneReferenceSelected;

        [SerializeField]
        private string sceneName;

        private Button button;

        private void Awake()
        {
            this.button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            this.button.onClick.AddListener(SendSampleSceneReference);
        }

        private void OnDisable()
        {
            this.button.onClick.RemoveListener(SendSampleSceneReference);
        }

        private void SendSampleSceneReference()
        {
            this.OnSampleSceneReferenceSelected?.Invoke(this.sceneName);
        }
    }

}
