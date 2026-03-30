using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
namespace WaypointSystem {


    // PLEASE PUT IN THE EDITOR FOLDER
    public class WaypointEditorTool : EditorWindow {
        private enum SortMode { Axis, Proximity }
        private enum Axis { X, Y, Z }

        private SortMode sortMode = SortMode.Proximity;
        private List<Axis> axisOrder = new() { Axis.X, Axis.Y, Axis.Z };
        private bool[] axisEnabled = new bool[3] { true, false, false };
        private bool loop = false;

        [MenuItem("Tools/Waypoint/Smart Link Tool")]
        public static void ShowWindow() {
            GetWindow<WaypointEditorTool>("Waypoint Linker");
        }

        void OnGUI() {
            GUILayout.Label("Waypoint Linking Tool", EditorStyles.boldLabel);

            sortMode = (SortMode)EditorGUILayout.EnumPopup("Sort Mode", sortMode);

            if (sortMode == SortMode.Axis) {
                GUILayout.Label("Sort Axis Priority (Top = Highest):");
                axisEnabled[0] = EditorGUILayout.ToggleLeft("Sort by X", axisEnabled[0]);
                axisEnabled[1] = EditorGUILayout.ToggleLeft("Sort by Y", axisEnabled[1]);
                axisEnabled[2] = EditorGUILayout.ToggleLeft("Sort by Z", axisEnabled[2]);
            }

            loop = EditorGUILayout.Toggle("Loop Path", loop);

            if (GUILayout.Button("Link Selected Objects")) {
                if (sortMode == SortMode.Proximity)
                    LinkByProximity();
                else
                    LinkByAxis();
            }
        }

        void LinkByAxis() {
            GameObject[] selected = Selection.gameObjects;
            if (selected.Length < 2) {
                Debug.LogWarning("Select at least two objects.");
                return;
            }

            List<Func<Transform, float>> order = new();

            if (axisEnabled[0]) order.Add(t => t.position.x);
            if (axisEnabled[1]) order.Add(t => t.position.y);
            if (axisEnabled[2]) order.Add(t => t.position.z);

            Array.Sort(selected, (a, b) => {
                foreach (var key in order) {
                    int cmp = key(a.transform).CompareTo(key(b.transform));
                    if (cmp != 0) return cmp;
                }
                return 0;
            });

            LinkWaypoints(selected);
        }

        void LinkByProximity() {
            GameObject[] selected = Selection.gameObjects;
            if (selected.Length < 2) {
                Debug.LogWarning("Select at least two objects.");
                return;
            }

            List<GameObject> remaining = new(selected);
            List<GameObject> sorted = new();

            GameObject current = GetClosestTo(Vector3.zero, remaining);
            sorted.Add(current);
            remaining.Remove(current);

            while (remaining.Count > 0) {
                GameObject next = GetClosestTo(current.transform.position, remaining);
                sorted.Add(next);
                remaining.Remove(next);
                current = next;
            }

            LinkWaypoints(sorted.ToArray());
        }

        void LinkWaypoints(GameObject[] sorted) {
            for (int i = 0; i < sorted.Length; i++) {
                Waypoint wp = sorted[i].GetComponent<Waypoint>();
                if (wp == null) wp = sorted[i].AddComponent<Waypoint>();
                wp.SetPrevious((i > 0) ? sorted[i - 1].GetComponent<Waypoint>() : null);
                wp.SetNext((i < sorted.Length - 1) ? sorted[i + 1].GetComponent<Waypoint>() : (loop ? sorted[0].GetComponent<Waypoint>() : null));


                EditorUtility.SetDirty(wp);
            }

            if (loop && sorted.Length > 1) {
                Waypoint first = sorted[0].GetComponent<Waypoint>();
                Waypoint last = sorted[^1].GetComponent<Waypoint>();
                first.previous = last;
                EditorUtility.SetDirty(first);
            }

            Debug.Log("Waypoints linked using " + sortMode + " mode.");
        }

        GameObject GetClosestTo(Vector3 point, List<GameObject> objs) {
            float minDist = float.MaxValue;
            GameObject closest = null;
            foreach (var obj in objs) {
                float dist = Vector3.Distance(point, obj.transform.position);
                if (dist < minDist) {
                    minDist = dist;
                    closest = obj;
                }
            }
            return closest;
        }
    }
}