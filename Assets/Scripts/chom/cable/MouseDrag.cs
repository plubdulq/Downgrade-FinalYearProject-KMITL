using UnityEngine;

public class MouseDrag : MonoBehaviour
{
    public float distanceFromCamera = 2f;

    void OnMouseDrag()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = distanceFromCamera; // ระยะห่างจากกล้อง

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        transform.position = worldPos;
    }
}
