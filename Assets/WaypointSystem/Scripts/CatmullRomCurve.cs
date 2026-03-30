using UnityEngine;
using System.Collections.Generic;

namespace WaypointSystem {
    [RequireComponent(typeof(Waypoint))]
    public class CatmullRomCurve : MonoBehaviour, ICurve {





        [Header("Rotation")]
        [Tooltip("Rotate the curve around the line between current and next waypoint.")]
        [Range(0f, 360f)]
        public float rotationDegrees = 0f;



        public List<Vector3> GetSampledPoints(Vector3 from, Vector3 to, float resolution) {
            List<Vector3> points = new();

            Waypoint current = GetComponent<Waypoint>();
            if (current == null || current.previous == null || current.next == null || current.next.next == null)
                return new List<Vector3> { from, to };

            Vector3 p0 = current.previous.transform.position;
            Vector3 p1 = from;
            Vector3 p2 = to;
            Vector3 p3 = current.next.next.transform.position;

            Vector3 dir = (to - from).normalized;

            // Stable perpendicular axes
            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(up, dir)) > 0.99f)
                up = Vector3.right;

            Vector3 right = Vector3.Cross(dir, up).normalized;
            Vector3 upRotated = Vector3.Cross(right, dir).normalized;

            Quaternion twist = Quaternion.AngleAxis(rotationDegrees, dir);

            float step = Mathf.Max(0.001f, resolution);

            for (float t = 0; t <= 1; t += step) {
                // Sample Catmull-Rom point
                Vector3 point = 0.5f * (
                    2f * p1 +
                    (-p0 + p2) * t +
                    (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
                    (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
                );

                // Base position along straight line from → to
                Vector3 basePoint = Vector3.Lerp(from, to, t);

                // Local offset from center line
                Vector3 offset = point - basePoint;

                // Rotate offset around the from-to axis
                Vector3 rotatedOffset = twist * offset;

                // Final point = base + rotated offset
                points.Add(basePoint + rotatedOffset);
            }

            return points;
        }
        private void OnValidate() {
            Waypoint attachedWaypoint = GetComponent<Waypoint>();
            if (attachedWaypoint == null) return;
            attachedWaypoint.curve = this;

        }



    }
}