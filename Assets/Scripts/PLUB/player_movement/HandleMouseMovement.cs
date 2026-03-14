using UnityEngine;

public class HandleMouseMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float moveSpeed;
    public float lookSpeed;

    private float rotX = 0f;
    private float rotY = 0f;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;

        rotX += mouseX;
        rotY -= mouseY;
        rotY = Mathf.Clamp(rotY, -90f, 90f);

        transform.localRotation = Quaternion.Euler(rotY, rotX, 0f);
    }
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal"); // A, D
        float v = Input.GetAxis("Vertical");   // W, S

        Vector3 dir = (transform.forward * v) + (transform.right * h);
        transform.position += dir * moveSpeed * Time.deltaTime;
    }
}
