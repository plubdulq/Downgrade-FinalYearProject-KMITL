using UnityEngine;
using System.Collections.Generic;

namespace WaypointSystem {
    [RequireComponent(typeof(Waypoint))]
    public class SineCurve : MonoBehaviour, ICurve {
        [Tooltip("Height of the wave or radius of the helix.")]
        public float amplitude = 1f;

        [Tooltip("Number of sine wave cycles or helix turns between waypoints.")]
        public float frequency = 1f;

        [Tooltip("Phase offset in radians (shifts the curve forward or backward).")]
        public float phase = 0f;

        [Tooltip("Rotate the sine or helix curve around the axis formed between the waypoints.")]
        [Range(0f, 360f)]
        public float rotationDegrees = 0f;

        [Tooltip("Enable to create a circular helicoidal pattern instead of a flat sine wave.")]
        public bool useHelix = false;

        [Tooltip("0 = flat sine wave. 1 = full circle helix. Used only when helix is enabled.")]
        [Range(0f, 1f)]
        public float flatten = 1f;

        private void OnValidate() {
            Waypoint current = GetComponent<Waypoint>();
            if (current == null) return;
            current.curve = this;
        }

        public List<Vector3> GetSampledPoints(Vector3 from, Vector3 to, float resolution) {
            List<Vector3> points = new();

            Vector3 dir = (to - from).normalized;

            // Stable perpendicular axes
            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(dir, up)) > 0.99f)
                up = Vector3.right;

            Vector3 right = Vector3.Cross(dir, up).normalized;
            Vector3 upRotated = Vector3.Cross(right, dir).normalized;
            Quaternion twist = Quaternion.AngleAxis(rotationDegrees, dir);

            for (float t = 0f; t <= 1f; t += resolution) {
                Vector3 basePoint = Vector3.Lerp(from, to, t);
                float angle = t * Mathf.PI * 2 * frequency + phase;

                Vector3 offset;

                if (useHelix) {
                    offset = (Mathf.Cos(angle) * flatten * right + Mathf.Sin(angle) * upRotated) * amplitude;
                }
                else {
                    // Fade the sine to zero at the ends
                    float shapedSine = Mathf.Sin(t * Mathf.PI) * Mathf.Sin(angle);
                    offset = upRotated * shapedSine * amplitude;
                }

                points.Add(basePoint + twist * offset);
            }

            return points;
        }

    }


}
