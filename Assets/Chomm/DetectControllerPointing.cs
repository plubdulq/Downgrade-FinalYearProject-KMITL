using UnityEngine;


public class DetectControllerPointing : MonoBehaviour
{
    // ลาก Controller (ที่มี XRRayInteractor) มาใส่ในช่องนี้ที่ Inspector
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;

    void Update()
    {
        // ตรวจสอบว่า Ray กำลังชนอะไรอยู่หรือไม่
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // ดูว่าชน GameObject ชื่ออะไร
            Debug.Log("Controller ชี้ไปที่: " + hit.collider.gameObject.name);

            // ตัวอย่าง: ถ้าชนวัตถุที่มี Tag "Button"
            if (hit.collider.CompareTag("Button"))
            {
                // ทำอะไรบางอย่าง...
            }
        }
    }
}