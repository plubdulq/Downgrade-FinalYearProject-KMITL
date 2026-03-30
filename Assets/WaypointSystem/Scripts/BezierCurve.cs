using UnityEngine;
using System.Collections.Generic;
namespace WaypointSystem {
    [RequireComponent(typeof(Waypoint))]
    public class BezierCurve : MonoBehaviour, ICurve {
        [Header("Control Point Settings")]
        [Tooltip("Offset direction for the control point near the start.")]
        public Vector3 controlOffsetStart = Vector3.up;

        [Tooltip("Offset direction for the control point near the end.")]
        public Vector3 controlOffsetEnd = Vector3.up;

        [Tooltip("Control offset magnitude (relative to path length).")]
        public float controlDistanceFactor = 0.3f;

        [Header("Curve Rotatison")]
        [Tooltip("Rotate the control point plane around the line between waypoints.")]
        [Range(0f, 360f)]
        public float rotationDegrees = 0f;



        public List<Vector3> GetSampledPoints(Vector3 from, Vector3 to, float resolution) {
            List<Vector3> points = new();

            Vector3 dir = (to - from).normalized;
            float distance = Vector3.Distance(from, to);

            // Get stable perpendicular axes
            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(dir, up)) > 0.99f)
                up = Vector3.right;

            Vector3 right = Vector3.Cross(dir, up).normalized;
            Vector3 upRotated = Vector3.Cross(right, dir).normalized;

            Quaternion twist = Quaternion.AngleAxis(rotationDegrees, dir);

            // Calculate control points with rotation
            Vector3 rotatedStartOffset = twist * (controlOffsetStart.normalized.x * right + controlOffsetStart.normalized.y * upRotated);
            Vector3 rotatedEndOffset = twist * (controlOffsetEnd.normalized.x * right + controlOffsetEnd.normalized.y * upRotated);

            Vector3 p0 = from;
            Vector3 p1 = from + rotatedStartOffset * distance * controlDistanceFactor;
            Vector3 p2 = to + rotatedEndOffset * distance * controlDistanceFactor;
            Vector3 p3 = to;

            for (float t = 0f; t <= 1f; t += resolution) {
                Vector3 point = Mathf.Pow(1 - t, 3) * p0 +
                                3 * Mathf.Pow(1 - t, 2) * t * p1 +
                                3 * (1 - t) * Mathf.Pow(t, 2) * p2 +
                                Mathf.Pow(t, 3) * p3;
                points.Add(point);
            }

            return points;
        }
        private void OnValidate() {
            Waypoint current = GetComponent<Waypoint>();
            if (current == null) return;
            current.curve = this;
        }
    }
}