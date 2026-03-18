using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CCTVDetectionZone : MonoBehaviour
{
    [Header("CCTV Info")]
    public string cameraName = "";

    [Header("Debug")]
    public bool debugLogs = true;

    [Header("Zone Visual")]
    public Transform zoneVisual;
    public Renderer zoneRenderer;
    public Color normalColor = new Color(0f, 1f, 0f, 0.12f);
    public Color detectedColor = new Color(1f, 0f, 0f, 0.18f);

    [Header("State (Read Only)")]
    public bool isPlayerInside = false;
    public string lastEnterTime = "";
    public string lastExitTime = "";

    private BoxCollider boxCol;

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(cameraName))
        cameraName = gameObject.name;
        
        boxCol = GetComponent<BoxCollider>();
        SyncZoneVisual();
        UpdateZoneVisualColor();
    }

    private void OnValidate()
    {
        boxCol = GetComponent<BoxCollider>();
        SyncZoneVisual();
        UpdateZoneVisualColor();
    }

    private void Start()
    {
        Debug.Log($"[CCTV] {cameraName} Detection Zone started on object: {gameObject.name}");
        UpdateZoneVisualColor();
    }

    private void OnTriggerEnter(Collider other)
    {
        CCTVPlayerMarker marker = other.GetComponentInParent<CCTVPlayerMarker>();

        if (debugLogs)
        {
            string rootName = other.transform.root != null ? other.transform.root.name : "NULL";
            string markerStatus = marker != null ? "YES" : "NO";
            Debug.Log($"[CCTV DEBUG] ENTER by: {other.name} | root: {rootName} | marker: {markerStatus}");
        }

        if (marker == null)
            return;

        if (isPlayerInside)
            return;

        isPlayerInside = true;
        lastEnterTime = DateTime.Now.ToString("HH:mm:ss");
        UpdateZoneVisualColor();

        Debug.Log($"[CCTV] {gameObject.name} detected PLAYER ENTER at {lastEnterTime}");
    }

    private void OnTriggerExit(Collider other)
    {
        CCTVPlayerMarker marker = other.GetComponentInParent<CCTVPlayerMarker>();

        if (debugLogs)
        {
            string rootName = other.transform.root != null ? other.transform.root.name : "NULL";
            string markerStatus = marker != null ? "YES" : "NO";
            Debug.Log($"[CCTV DEBUG] EXIT by: {other.name} | root: {rootName} | marker: {markerStatus}");
        }

        if (marker == null)
            return;

        if (!isPlayerInside)
            return;

        isPlayerInside = false;
        lastExitTime = DateTime.Now.ToString("HH:mm:ss");
        UpdateZoneVisualColor();

        Debug.Log($"[CCTV] {cameraName} detected PLAYER EXIT at {lastExitTime}");
    }

    private void SyncZoneVisual()
    {
        if (boxCol == null || zoneVisual == null)
            return;

        zoneVisual.localPosition = boxCol.center;
        zoneVisual.localRotation = Quaternion.identity;
        zoneVisual.localScale = boxCol.size;
    }

    private void UpdateZoneVisualColor()
    {
        if (zoneRenderer == null)
            return;

        if (zoneRenderer.material != null)
        {
            zoneRenderer.material.color = isPlayerInside ? detectedColor : normalColor;
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) return;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = isPlayerInside ? detectedColor : normalColor;
        Gizmos.DrawCube(col.center, col.size);

        Gizmos.color = isPlayerInside ? Color.red : Color.green;
        Gizmos.DrawWireCube(col.center, col.size);

        Gizmos.matrix = oldMatrix;
    }
}