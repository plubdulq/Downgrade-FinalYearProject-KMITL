using UnityEngine;
using System.Collections;

public class SlidingDoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("ตำแหน่งของประตูตอนปิด (ปกติคือตำแหน่งเริ่มต้น)")]
    public Vector3 closedPosition;
    [Tooltip("ตำแหน่งของประตูตอนเปิด (ขยับแกนที่ต้องการแล้วใช้รัน Set Open Position)")]
    public Vector3 openPosition;
    [Tooltip("ความเร็วในการเลื่อน (วินาที)")]
    public float slideDuration = 1.0f;

    private bool isOpen = false;
    private bool isMoving = false;

    private void Start()
    {
        // หากไม่ได้ตั้งค่า closedPosition ไว้ ให้เอาตำแหน่งปัจจุบันเป็นตำแหน่งปิด
        if (closedPosition == Vector3.zero)
        {
            closedPosition = transform.localPosition;
        }
    }

    /// <summary>
    /// สั่งเปิด-ปิด สลับกัน (เอาไปใส่ใน UnityEvent ของปุ่มได้เลย)
    /// </summary>
    public void ToggleDoor()
    {
        if (isMoving) return;
        
        if (isOpen)
            StartCoroutine(SlideDoor(closedPosition));
        else
            StartCoroutine(SlideDoor(openPosition));

        isOpen = !isOpen;
    }

    public void OpenDoor()
    {
        if (isMoving || isOpen) return;
        StartCoroutine(SlideDoor(openPosition));
        isOpen = true;
    }

    public void CloseDoor()
    {
        if (isMoving || !isOpen) return;
        StartCoroutine(SlideDoor(closedPosition));
        isOpen = false;
    }

    private IEnumerator SlideDoor(Vector3 targetPosition)
    {
        isMoving = true;
        Vector3 startPosition = transform.localPosition;
        float timeElapsed = 0;

        while (timeElapsed < slideDuration)
        {
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, timeElapsed / slideDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition;
        isMoving = false;
    }

    [ContextMenu("📌 Set Current as Closed Position")]
    private void SetClosed()
    {
        closedPosition = transform.localPosition;
    }

    [ContextMenu("📌 Set Current as Open Position")]
    private void SetOpen()
    {
        openPosition = transform.localPosition;
    }
}
