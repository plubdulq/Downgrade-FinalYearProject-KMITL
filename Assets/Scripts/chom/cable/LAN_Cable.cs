
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LAN_Cable : MonoBehaviour
{
    public Transform[] points;
    LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = points.Length;
    }

    void Update()
    {
        for (int i = 0; i < points.Length; i++)
            lr.SetPosition(i, points[i].position);
    }
}
