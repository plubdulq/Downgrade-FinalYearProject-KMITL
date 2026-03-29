using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Chomm.CableSystem;

namespace BNG {
    public class SnapZoneRingHelper : MonoBehaviour {

        [Header("Reference")]
        public SnapZone Snap;

        [Header("Colors")]
        public Color RestingColor = Color.gray;
        public Color ValidColor = Color.green;
        public Color InvalidColor = Color.red;

        [Header("Scale")]
        public float RestingScale = 1000f;
        public float ActiveScale = 800f;

        [Header("Pulse")]
        public bool usePulse = true;
        public float pulseSpeed = 5f;
        public float pulseIntensity = 0.3f;

        [Header("Highlight")]
        public float highlightDuration = 1.5f;

        CanvasScaler ringCanvas;
        Text ringText;
        GrabbablesInTrigger nearby;

        bool hasObject;
        bool isValid;

        float highlightTimer = 0f;
        Color highlightColor;

        void Start() {
            ringCanvas = GetComponent<CanvasScaler>();
            ringText = GetComponent<Text>();
            nearby = Snap.GetComponent<GrabbablesInTrigger>();
        }

        void Update() {

            CheckState();

            // 🔥 Scale
            float targetScale = hasObject ? ActiveScale : RestingScale;
            ringCanvas.dynamicPixelsPerUnit = Mathf.Lerp(
                ringCanvas.dynamicPixelsPerUnit,
                targetScale,
                Time.deltaTime * 10f
            );

            // 🔥 Highlight จาก PlayerScanner
            if (highlightTimer > 0)
            {
                highlightTimer -= Time.deltaTime;
                ApplyPulse(highlightColor);
                return;
            }

            // 🔥 Auto Mode
            if (!hasObject)
            {
                ringText.color = RestingColor;
            }
            else
            {
                if (isValid)
                    ApplyPulse(ValidColor);   // 🟢
                else
                    ApplyPulse(InvalidColor); // 🔴
            }
        }
void CheckState()
{
    hasObject = false;
    isValid = false;

    if (nearby == null) return;

    if (Snap.HeldItem != null)
        return;

    if (Snap.ClosestGrabbable != null)
    {
        hasObject = true;

        var plug = Snap.ClosestGrabbable.GetComponent<CablePlug>();

        if (plug != null)
        {
            // ✅ รองรับ None เป็น wildcard
            if (Snap.plugType == PlugType.None || plug.plugType == PlugType.None)
            {
                isValid = true;
            }
            else
            {
                isValid = plug.plugType == Snap.plugType;
            }
        }
    }
}
/*        void CheckState()
        {
            hasObject = false;
            isValid = false;

            if (nearby == null) return;

            if (Snap.HeldItem != null)
                return;

            if (Snap.ClosestGrabbable != null)
            {
                hasObject = true;

                var plug = Snap.ClosestGrabbable.GetComponent<CablePlug>();

                if (plug != null)
                {
                    isValid = plug.plugType == Snap.plugType;
                }
            }
        }
*/
        void ApplyPulse(Color baseColor)
        {
            if (!usePulse)
            {
                ringText.color = baseColor;
                return;
            }

            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity + 1f;
            Color final = baseColor * pulse;
            final.a = 1f;

            ringText.color = final;
        }

        // 🔥 เรียกจาก PlayerScanner (Highlight ตอนเข้า)
        public void Highlight(Color color)
        {
            highlightColor = color;
            highlightTimer = highlightDuration;
        }

        // 🔥 Reset ตอนออกจาก Zone
        public void ClearHighlight()
        {
            highlightTimer = 0f;
        }
    }
}

// using System.Collections;
// using UnityEngine;
// using UnityEngine.UI;
// using Chomm.CableSystem;

// namespace BNG {
//     public class SnapZoneRingHelper : MonoBehaviour {

//         [Header("Reference")]
//         public SnapZone Snap;

//         [Header("Colors")]
//         public Color RestingColor = Color.gray;
//         public Color ValidColor = Color.green;
//         public Color InvalidColor = Color.red;

//         [Header("Scale")]
//         public float RestingScale = 1000f;
//         public float ActiveScale = 800f;

//         [Header("Pulse")]
//         public bool usePulse = true;
//         public float pulseSpeed = 5f;
//         public float pulseIntensity = 0.3f;

//         [Header("Highlight")]
//         public float highlightDuration = 1.5f;

//         CanvasScaler ringCanvas;
//         Text ringText;
//         GrabbablesInTrigger nearby;

//         bool hasObject;
//         bool isValid;

//         float highlightTimer = 0f;
//         Color highlightColor;

//         void Start() {
//             ringCanvas = GetComponent<CanvasScaler>();
//             ringText = GetComponent<Text>();
//             nearby = Snap.GetComponent<GrabbablesInTrigger>();
//         }

//         void Update() {

//             CheckState();

//             // 🔥 Scale
//             float targetScale = hasObject ? ActiveScale : RestingScale;
//             ringCanvas.dynamicPixelsPerUnit = Mathf.Lerp(
//                 ringCanvas.dynamicPixelsPerUnit,
//                 targetScale,
//                 Time.deltaTime * 10f
//             );

//             // 🔥 Highlight จาก PlayerScanner
//             if (highlightTimer > 0)
//             {
//                 highlightTimer -= Time.deltaTime;
//                 ApplyPulse(highlightColor);
//                 return;
//             }

//             // 🔥 Auto Mode
//             if (!hasObject)
//             {
//                 ringText.color = RestingColor;
//             }
//             else
//             {
//                 if (isValid)
//                     ApplyPulse(ValidColor);   // 🟢
//                 else
//                     ApplyPulse(InvalidColor); // 🔴
//             }
//         }

//       void CheckState()
//         {
//             hasObject = false;
//             isValid = false;

//             if (nearby == null) return;

//             if (Snap.HeldItem != null)
//                 return;

//             if (Snap.ClosestGrabbable != null)
//             {
//                 hasObject = true;

//                 var plug = Snap.ClosestGrabbable.GetComponent<CablePlug>();

//                 if (plug != null)
//                 {
//                     isValid = plug.plugType == Snap.plugType;
//                 }
//             }
//         }

//         void ApplyPulse(Color baseColor)
//         {
//             if (!usePulse)
//             {
//                 ringText.color = baseColor;
//                 return;
//             }

//             float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity + 1f;
//             Color final = baseColor * pulse;
//             final.a = 1f;

//             ringText.color = final;
//         }

//         // 🔥 เรียกจาก PlayerScanner (Highlight ตอนเข้า)
//         public void Highlight(Color color)
//         {
//             highlightColor = color;
//             highlightTimer = highlightDuration;
//         }

//         // 🔥 Reset ตอนออกจาก Zone
//         public void ClearHighlight()
//         {
//             highlightTimer = 0f;
//         }
//     }
// }