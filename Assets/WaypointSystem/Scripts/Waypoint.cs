using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;
using WaypointSystem;
namespace WaypointSystem {

    [ExecuteAlways]
    /// <summary>
    /// Represents a waypoint in a path. Can be linked to other waypoints to form a chain,
    /// and can optionally use its own visual settings or inherit from the previous waypoint.
    /// </summary>
    public class Waypoint : MonoBehaviour {
        /// <summary>
        /// The previous waypoint in the path.
        /// </summary>
        [Tooltip("The previous waypoint in the path.")]
        public Waypoint previous;

        /// <summary>
        /// The next waypoint in the path.
        /// </summary>
        [Tooltip("The next waypoint in the path.")]
        public Waypoint next;

        /// <summary>
        /// If true, this waypoint uses its own visual settings (size, color, line thickness, etc.).
        /// If false, it inherits them from the previous waypoint.
        /// </summary>
        [Tooltip("If true, this waypoint uses its own visual settings. If false, it inherits them from the previous waypoint.")]
        public bool useSelfForInfo = false;

        [Header("Gizmos Settings")]

        /// <summary>
        /// The color of the gizmo point when this waypoint is drawn in the Scene view.
        /// </summary>
        [Tooltip("Color of the gizmo point representing the waypoint.")]
        public Color pointColor = Color.yellow;

        /// <summary>
        /// The color of the line connecting this waypoint to the next one in the Scene view.
        /// </summary>
        [Tooltip("Color of the gizmo line connecting waypoints.")]
        public Color lineColor = Color.green;

        /// <summary>
        /// The radius of the sphere drawn for the waypoint in the Scene view.
        /// </summary>
        [Tooltip("Radius of the gizmo sphere representing this waypoint.")]
        public float sphereRadius = 0.5f;

        /// <summary>
        /// The size of the arrow drawn to indicate the path direction in the Scene view.
        /// </summary>
        [Tooltip("Size of the arrow used to indicate the direction of the path.")]
        public float arrowSize = 0.3f;

        /// <summary>
        /// The thickness of the line drawn between waypoints.
        /// </summary>
        [Tooltip("Thickness of the gizmo line connecting this waypoint to the next.")]
        public float lineSize = 5f;

        [Header("Path Settings")]

        /// <summary>
        /// The resolution used to generate the curve between waypoints. Smaller values produce smoother curves.
        /// </summary>
        [Tooltip("Resolution of the path curve. Lower values create smoother paths.")]
        [Range(0.001f, 1)]
        public float CurveResolution = 0.05f;

        /// <summary>
        /// Curve data used internally for path interpolation. Hidden in the Inspector.
        /// </summary>
        [HideInInspector]
        public ICurve curve;

        /*[HideInInspector]
        public List<Vector3> cachedCurvePoints = new();*/




        private void Start() {
            curve = GetComponent<ICurve>();
        }

        private void OnDrawGizmos() {
           
            Gizmos.color = pointColor;
            #if UNITY_EDITOR
            Handles.color = lineColor;
             #endif
            Gizmos.DrawSphere(transform.position, sphereRadius);
           
            if (next == null) return;

            if (IsWaypointCurve()) {

                DrawCurve();
                return;
            }

            DrawNormalPath();

        }

        private void DrawNormalPath() {
            #if UNITY_EDITOR
            Handles.DrawAAPolyLine(lineSize, transform.position, next.transform.position); // 4f is thickness
            #endif
            DrawArrow(transform.position, next.transform.position);
        }

        private bool IsWaypointCurve() {
            return !(curve == null);
        }

        private void DrawArrow(Vector3 from, Vector3 to) {
            Vector3 direction = (to - from).normalized;
            Vector3 arrowTail = Vector3.Lerp(from, to, 0.9f);
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -150, 0) * Vector3.forward;

            #if UNITY_EDITOR
            Handles.DrawAAPolyLine(lineSize, arrowTail, arrowTail + right * arrowSize);
            Handles.DrawAAPolyLine(lineSize, arrowTail + left * arrowSize);
            #endif

        }

        private void DrawCurve() {


            List<Vector3> points = curve.GetSampledPoints(transform.position, next.transform.position, CurveResolution);

            if (points.Count > 1) {
                for (int i = 1; i < points.Count; i++) {
                    #if UNITY_EDITOR
                    Handles.DrawAAPolyLine(lineSize, points[i - 1], points[i]);
                     #endif
                }

                // Optional: draw arrow at end
                DrawArrow(points[^2], points[^1]);
            }

        }


        void OnValidate() {
            UpdateInfo();
        }

        private void UpdateInfo() {
            curve = GetComponent<ICurve>();
            if (useSelfForInfo) return;

            var visited = new HashSet<Waypoint>();
            Waypoint current = this;

            while (current != null && visited.Add(current)) {
                if (current.useSelfForInfo) break;

                if (current.previous != null) {
                    current.lineColor = current.previous.lineColor;
                    current.arrowSize = current.previous.arrowSize;
                    current.pointColor = current.previous.pointColor;
                    current.sphereRadius = current.previous.sphereRadius;
                    current.lineSize = current.previous.lineSize;

                    if (current.previous.next == null) current.previous.next = current;
                }

                if (current.next != null && current.next.previous == null) {
                    current.next.previous = current;
                }

                current = current.next;
            }
        }

        public float GetDistanceToPrevious() {
            if (previous != null) {
                previous.getDistanceToNext();
            }
            return -1;
        }
        public float getDistanceToNext() {
            if (next != null) {
                if (curve != null) return getDistanceCurve();
                return Vector3.Distance(next.transform.position, transform.position);
            }
            return 0;
        }

        private float getDistanceCurve() {
            List<Vector3> points = curve.GetSampledPoints(transform.position, next.transform.position, CurveResolution);
            float distance = 0;
            if (points.Count > 1) {
                for (int i = 1; i < points.Count; i++) {
                    distance += Vector3.Distance(points[i - 1], points[i]);
                }


                return distance;
            }
            return -1;
        }



        public Waypoint GetDistanceWaypoint(Vector3 point, bool backwards, bool nearest) {
            Waypoint current = this;
            Waypoint solution = this;

            float distance = nearest ? float.MaxValue : 0f;
            var visited = new HashSet<Waypoint>();

            // Forward traversal
            while (current != null && visited.Add(current)) {
                float dist = Vector3.Distance(current.transform.position, point);
                if ((nearest && dist < distance) || (!nearest && dist > distance)) {
                    distance = dist;
                    solution = current;
                }
                current = current.next;
            }

            // Backward traversal
            if (backwards) {
                current = this.previous;
                while (current != null && visited.Add(current)) {
                    float dist = Vector3.Distance(current.transform.position, point);
                    if ((nearest && dist < distance) || (!nearest && dist > distance)) {
                        distance = dist;
                        solution = current;
                    }
                    current = current.previous;
                }
            }

            return solution;
        }


        public NavigationInfo GetWaypointAndTValueFromVector3(Vector3 pos) {
            pos = GetClosestPointOnPath(pos, true, true); // pos is great
            Waypoint closest = GetDistanceWaypoint(pos, true, true);
            Debug.DrawLine(pos, new Vector3(0, 0, 0), Color.cyan, 6);


            Debug.DrawLine(closest.transform.position, new Vector3(1, 2, 1));


            if (closest != this) return closest.GetWaypointAndTValueFromVector3(pos);
            Debug.DrawLine(pos, new Vector3(0, 0, 0), Color.black, 6);


            if (/*pos == transform.position ||*/ (previous == null && next == null)) {


                return new NavigationInfo(0, this);
            }


            if (next != null) {
                if (IsBetweenCurve(pos)) {
                    List<Vector3> points = curve.GetSampledPoints(transform.position, next.transform.position, CurveResolution);
                    //Debug.Log("closest to the path from next and curve");
                    float distNext = GetSumUntilReachPoint(points, pos);
                    return new NavigationInfo(getTValue(distNext), this);
                }
                else if (!IsWaypointCurve() && IsBetween(transform.position, next.transform.position, pos)) {
                    float distNext = (pos - transform.position).magnitude;
                    //Debug.Log("closest to the path from next and line");
                    //Debug.Log(getTValue(distNext));
                    return new NavigationInfo(getTValue(distNext), this);

                }
            }

            if (previous != null) {
                if (previous.IsBetweenCurve(pos)) {
                    List<Vector3> points = previous.curve.GetSampledPoints(previous.transform.position, previous.next.transform.position, previous.CurveResolution);
                    float distPrev = GetSumUntilReachPoint(points, pos);
                    //Debug.Log("closest to the path from previous and curve");
                    return new NavigationInfo(previous.getTValue(distPrev), previous);
                }
                else if (!previous.IsWaypointCurve() && IsBetween(previous.transform.position, transform.position, pos)) {
                    float distPrev = (pos - previous.transform.position).magnitude; // it is 1
                                                                                    //Debug.Log("closest to the path from previous and line");
                                                                                    //Debug.Log(distPrev + " was distance and TValue : " + previous.getTValue(distPrev));
                    return new NavigationInfo(previous.getTValue(distPrev), previous);
                }
            }

            Debug.LogWarning("GetWaypointAndTValueFromVector3 failed to find a valid segment.");
            return new NavigationInfo(0, this);
        }

        private float GetSumUntilReachPoint(List<Vector3> points, Vector3 target) {
            float distance = 0;
            if (points.Count > 1) {
                for (int i = 1; i < points.Count; i++) {
                    distance += Vector3.Distance(points[i - 1], points[i]);
                    if (Vector3.Equals(points[i], target)) return distance;
                }
            }
            return distance;
        }

        /// <summary>
        /// finds the closest (or farthest) point on the path
        /// </summary>
        /// <param name="point"> the point for which you want to find the closest point</param>
        /// <param name="goBackwards"> specifies if you can search the previous waypoints to find the closest point</param>
        /// <param name="getClosest"> true = closest , false =  farthest </param>
        /// <returns></returns>
        public Vector3 GetClosestPointOnPath(Vector3 point, bool goBackwards, bool getClosest) {
            Vector3 resultPoint = transform.position;
            float bestDistance = getClosest ? float.MaxValue : 0;

            var visited = new HashSet<Waypoint>();
            Waypoint current = this;

            // Forward
            while (current != null && current.next != null && visited.Add(current)) {
                Vector3 candidate = current.GetClosestPointBetween(point);
                float dist = Vector3.Distance(point, candidate);

                if ((!getClosest && dist > bestDistance) || (getClosest && dist < bestDistance)) {
                    bestDistance = dist;
                    resultPoint = candidate;
                }

                current = current.next;
            }
            visited.Clear();
            //visited = new HashSet<Waypoint>();
            if (goBackwards) {
                current = this.previous;
                while (current != null  /* && current.previous != null*/ && visited.Add(current)) {
                    Vector3 candidate = current.GetClosestPointBetween(point);
                    float dist = Vector3.Distance(point, candidate);

                    if ((!getClosest && dist > bestDistance) || (getClosest && dist < bestDistance)) {
                        bestDistance = dist;
                        resultPoint = candidate;
                    }

                    current = current.previous;
                }
            }
            Debug.DrawLine(point, resultPoint);
            return resultPoint;
        }



        private Vector3 GetClosestPointBetween(Vector3 point) {

            if (curve != null) { // if the current waypoint is a curve
                var sampledPoints = curve.GetSampledPoints(transform.position, next.transform.position, CurveResolution);
                return GetClosestPointFromList(sampledPoints, point);
            }
            else { // its a single line
                return ClosestPointOnPath(point);
            }
        }

        private Vector3 GetClosestPointFromList(List<Vector3> points, Vector3 target) // get the closest point between the target and the list of points
        => points.OrderBy(p => (target - p).sqrMagnitude).First();


        #region Math
        private Vector3 ClosestPointOnPath(Vector3 point) { // get the closest point on the path between this and the next waypoint
            Vector3 a = transform.position;
            Vector3 b = next.transform.position;
            Vector3 ab = b - a;
            float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
            t = Mathf.Clamp01(t);
            return a + t * ab;
        }

        /// <summary>
        /// Check if C is on the line segment AB
        /// </summary>
        bool IsBetween(Vector3 A, Vector3 B, Vector3 C) {
            // Check if C is on the line segment AB
            float cross = Vector3.Cross(B - A, C - A).magnitude;
            if (cross > 0.0001f) return false; // Not colinear

            float dot = Vector3.Dot(C - A, B - A);
            if (dot < 0) return false; // C is before A

            float lenSq = (B - A).sqrMagnitude;
            if (dot > lenSq) return false; // C is after B

            return true; // C is between A and B
        }
        /// <summary>
        /// Checks if the vector A is on the curve between this waypoint and the previous
        /// </summary>
        bool IsBetweenCurve(Vector3 A) {
            if (!IsWaypointCurve() || next == null) return false;
            List<Vector3> points = curve.GetSampledPoints(transform.position, next.transform.position, CurveResolution);
            for (int i = 1; i < points.Count; i++) {
                if (IsBetween(points[i - 1], points[i], A)) return true;
            }
            return false;
        }

        #endregion Math

        public Vector3 GetNextPosition(float travelDistance, ref float tValue, out Waypoint lastVisited) {

            lastVisited = this;

            tValue += getTValue(travelDistance); // tValue = travelDistance / segmentLength
            if (getTValue(travelDistance).Equals(0) && travelDistance < 0 && previous != null && tValue.Equals(0)) {
                tValue = 1;
                return previous.GetNextPosition(travelDistance, ref tValue, out lastVisited);
            }


            if (Mathf.Approximately(tValue, 0f) || (tValue > 0 && next == null) || (tValue < 0 && previous == null)) {
                tValue = 0;
                return transform.position;
            }

            float segmentLength = getDistanceToNext();

            if (tValue < 0f) {


                float traveled = segmentLength * -tValue;
                float remaining = travelDistance - traveled;
                tValue = 1f;
                return previous.GetNextPosition(remaining, ref tValue, out lastVisited);
            }

            else if (tValue > 1f) {


                float traveled = segmentLength * (tValue - 1f);
                float remaining = travelDistance - traveled;
                tValue = 0f;
                return next.GetNextPosition(remaining, ref tValue, out lastVisited);
            }
            // in that case a next one exists and tvalue is between 0 and 1
            return getVector3FromTValue(tValue);
        }






        public float getTValue(float distanceToTravel) {
            if (Mathf.Approximately(getDistanceToNext(), 0)) return 0;
            return distanceToTravel / getDistanceToNext();
        }
        public Vector3 getVector3FromTValue(float t) { // this assumes it is below 1 
            if (IsWaypointCurve()) {
                return getVector3FromTValueCurve(t);
            }
            return transform.position + GetVectorToNext() * getDistanceToNext() * t;
        }

        private Vector3 getVector3FromTValueCurve(float t) {
            List<Vector3> points = curve.GetSampledPoints(transform.position, next.transform.position, CurveResolution);
            float totalDistance = getDistanceToNext();
            float distanceToTravel = totalDistance * t;

            float currentDistance = 0f;
            for (int i = 1; i < points.Count; i++) {
                float segmentDistance = Vector3.Distance(points[i - 1], points[i]);

                if (currentDistance + segmentDistance < distanceToTravel) currentDistance += segmentDistance;

                else {
                    float remaining = distanceToTravel - currentDistance;
                    float segmentT = remaining / segmentDistance;

                    return Vector3.Lerp(points[i - 1], points[i], segmentT);
                }
            }

            // If t = 1 or rounding error at end
            return points[^1];
        }


        public Vector3 GetVectorToNext() {
            if (next == null) return Vector3.zero;

            return (next.transform.position - transform.position).normalized;
        }

        public Waypoint getNthNextWaypoint(int n) {

            Waypoint current = this;

            while (n != 0) {
                if (n > 0) {
                    if (current.next == null) return current;
                    current = current.next;
                    n--;
                }
                else {
                    if (current.previous == null) return current;
                    current = current.previous;
                    n++;
                }
            }
            return current;
        }


        /// <summary>
        /// to get the starting waypoint of the path. If its a loop, it will return self
        /// </summary>
        /// <returns></returns>
        public Waypoint getHeadWaypoint() {
            var visited = new HashSet<Waypoint>();
            Waypoint current = this;

            while (current.previous != null) {
                if (!visited.Add(current)) return this; // loop detected → return self
                current = current.previous;
            }

            return current;
        }
        /// <summary>
        /// to get the ending waypoint of the path. If its a loop, it will return self
        /// </summary>
        /// <returns></returns>
        public Waypoint getTailWaypoint() {
            var visited = new HashSet<Waypoint>();
            Waypoint current = this;

            while (current.next != null) {
                if (!visited.Add(current)) return this; // loop detected → return self
                current = current.next;
            }

            return current;
        }

        public void SetNext(Waypoint p) {
            if (p == null) return;
            next = p;
            p.previous = this;
        }
        public void SetPrevious(Waypoint p) {
            if (p == null) return;
            p.SetNext(this);
        }
    }
}
