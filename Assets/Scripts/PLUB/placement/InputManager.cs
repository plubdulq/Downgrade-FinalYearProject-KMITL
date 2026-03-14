using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Transform rightController;
    [SerializeField] private LayerMask placementLayerMask;

    private Vector3 lastPosition;
    private Vector3 endpoint;
    private GameObject selectedObject;

    public Vector3 GetSelectedMapPosition()
    {
        Ray ray = new Ray(rightController.position, rightController.forward);
        Debug.DrawRay(ray.origin, ray.direction * 50f, Color.blue);
        if (Physics.Raycast(ray, out RaycastHit hit, 50f, placementLayerMask))
        {
            lastPosition = hit.point;
        }
        return lastPosition;
    }



    // [SerializeField]
    // private Camera sceneCamera;

    // private Vector3 lastPosition;
    // private GameObject selectedObject;

    // [SerializeField]
    // private LayerMask placementlayerMask;

    // void Update()
    // {
    //     GetSelectedObject();
    // }

    // public GameObject GetSelectedObject()
    // {
    //     Vector3 mousePos = Mouse.current.position.ReadValue();
    //     mousePos.z = sceneCamera.farClipPlane;
    //     Vector3 viewportPos = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, mousePos.z);
    //     //Debug.Log("Using main camera: " + sceneCamera.name);
    //     //Debug.Log("Mouse Position: " + mousePos);
    //     Ray ray = sceneCamera.ViewportPointToRay(viewportPos);
    //     //Debug.Log("Ray: " + ray.origin + " dir: " + ray.direction);
    //     Debug.DrawRay(ray.origin, ray.direction * 4000f, Color.red, 2f);
    //     RaycastHit hit;
    //     if (Physics.Raycast(ray, out hit, 4000f, placementlayerMask))
    //     {
    //         //Debug.Log("Hit Position: " + hit.point + hit.collider.name);
    //         lastPosition = hit.point;
    //         if (Input.GetMouseButtonDown(0))
    //         {
    //             selectedObject = hit.collider.gameObject;
    //             //Debug.Log("Selected Object: " + selectedObject.name);
    //         }
    //     }
    //     return selectedObject;
    // }

    // public Vector3 GetSelectedMapPosition()
    // {
    //     Vector3 mousePos = Mouse.current.position.ReadValue();
    //     mousePos.z = sceneCamera.farClipPlane;
    //     Vector3 viewportPos = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, mousePos.z);
    //     //Debug.Log("Using main camera: " + sceneCamera.name);
    //     //Debug.Log("Mouse Position: " + mousePos);
    //     Ray ray = sceneCamera.ViewportPointToRay(viewportPos);
    //     //Debug.Log("Ray: " + ray.origin + " dir: " + ray.direction);
    //     Debug.DrawRay(ray.origin, ray.direction * 4000f, Color.red, 2f);
    //     RaycastHit hit;
    //     if (Physics.Raycast(ray, out hit, 4000f, placementlayerMask))
    //     {
    //         //Debug.Log("Hit Position: " + hit.point + hit.collider.name);
    //         lastPosition = hit.point;
    //     }
    //     return lastPosition;
    // }
}
