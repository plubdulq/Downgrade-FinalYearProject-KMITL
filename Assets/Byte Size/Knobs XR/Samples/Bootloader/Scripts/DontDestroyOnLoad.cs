using UnityEngine;

namespace ByteSize.XRKnobs.Samples
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
