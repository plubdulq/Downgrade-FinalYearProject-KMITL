using UnityEngine;
using System.Collections.Generic;

namespace Chomm.CableSystem
{
    public enum PlugType {None, Rj45, Dsl, PowerIn, PowerOut, IECC13, IECC14, IECC19, IECC20, StandardPlug, FiberLCSinglemode, FiberLCMultimode}
    [ExecuteInEditMode]
    [RequireComponent(typeof(TubeRenderer))]
    public class Cable : MonoBehaviour
    {
        public CablePlug PlugA;
        public CablePlug PlugB;
        private Outline ol;
        [Header("Path Settings")]
        public List<Transform> intermediatePoints = new List<Transform>();

        [Header("Cable Settings")]
        public int segments = 20;
        public float stiffness = 0.5f;
        public float sag = 0.5f;

        private TubeRenderer tubeRenderer;
        private List<Vector3> points = new List<Vector3>();

        private void Awake()
        {
            tubeRenderer = GetComponent<TubeRenderer>();
        }

        private void Update()
        {
            GenerateCable();
        }

        private void OnValidate()
        {
            GenerateCable();
        }
        private void Start()
        {
            ol =  gameObject.GetComponent<Outline>();
            ol.enabled = false;
            OutlineManager.Instance.AddCable(this);
        }

        private List<Transform> autoAnchorsA = new List<Transform>();
        private List<Transform> autoAnchorsB = new List<Transform>();
        private bool createdAutoAnchorA = false;
        private bool createdAutoAnchorB = false;
        public float Sag
        {
            get { return sag; }
            set
            {
                if (sag != value)
                {
                    sag = value;
                    OnSagChanged();
                }
            }
        }

        void OnSagChanged()
        {
            Cable[] comps = transform.GetComponentsInChildren<Cable>();

            foreach (Cable comp in comps)
            {
                if (comp != this)
                {
                    comp.sag = sag;
                }
            }
        }
        public void ActiveOutline(bool active)
        {
            ol.enabled=active;
        }
        private void GenerateCable()
        {
            if (tubeRenderer == null) tubeRenderer = GetComponent<TubeRenderer>();
            if (PlugA == null || PlugB == null) return;
            if (PlugA.Start1 == null || PlugA.Start2 == null) return;
            if (PlugB.Start1 == null || PlugB.Start2 == null) return;
            Cable c = transform.GetComponentInChildren<Cable>();
            points.Clear();

            // 1. Add Plug A Internal Segment
            // Path: A.Start1 -> A.Start2
            points.Add(PlugA.Start1.position);
            points.Add(PlugA.Start2.position);

            // 2. Generate Segments
            Vector3 currentPos = PlugA.Start2.position;
            // DirA: Outward from PlugA
            Vector3 currentDir = (PlugA.Start2.position - PlugA.Start1.position).normalized;
            if (currentDir == Vector3.zero) currentDir = PlugA.transform.up;

            // Combine Auto Anchors and Intermediate Points
            List<Transform> effectivePoints = new List<Transform>();
            
            // Add A Anchors (In Order: Plug -> A1 -> A2)
            if (autoAnchorsA != null && autoAnchorsA.Count > 0) effectivePoints.AddRange(autoAnchorsA);
            
            // Add Intermediate
            if (intermediatePoints != null) effectivePoints.AddRange(intermediatePoints);
            
            // Add B Anchors (In Reverse Order: B2 -> B1 -> Plug)
            if (autoAnchorsB != null && autoAnchorsB.Count > 0)
            {
                for (int i = autoAnchorsB.Count - 1; i >= 0; i--)
                {
                    effectivePoints.Add(autoAnchorsB[i]);
                }
            }

            // Iterate through effective points
            for (int i = 0; i < effectivePoints.Count; i++)
            {
                Transform p = effectivePoints[i];
                if (p == null) continue;

                Vector3 nextPos = p.position;
                Vector3 tangent = Vector3.zero;

                // Determine previous and next positions relative to this point for tangent calc
                Vector3 prevPosForTangent = (i == 0) ? PlugA.Start2.position : effectivePoints[i - 1].position;
                Vector3 nextPosForTangent;
                if (i < effectivePoints.Count - 1)
                {
                    nextPosForTangent = effectivePoints[i + 1].position;
                }
                else
                {
                    nextPosForTangent = PlugB.Start2.position;
                }

                // Calculate smooth tangent (Catmull-Rom style approximation)
                tangent = (nextPosForTangent - prevPosForTangent).normalized;
                if (tangent == Vector3.zero) tangent = p.transform.forward;

                // Incoming Dir (dirB for the incoming segment):
                // GenerateSegment does p3 + dirB. We want p3 - Tangent. So dirB = -Tangent.
                Vector3 nextDir = -tangent;
                
                GenerateSegment(currentPos, nextPos, currentDir, nextDir);

                // Add the intermediate point itself
                points.Add(nextPos);

                // Setup for Next Segment (Outgoing from this point)
                currentPos = nextPos;
                // Outgoing Dir (dirA for next segment): Tangent
                currentDir = tangent;
            }

            // Final Segment (To Plug B)
            Vector3 finalPos = PlugB.Start2.position;
            Vector3 finalDir = (PlugB.Start2.position - PlugB.Start1.position).normalized;
            if (finalDir == Vector3.zero) finalDir = PlugB.transform.up;

            GenerateSegment(currentPos, finalPos, currentDir, finalDir);


            // 3. Add Plug B Internal Segment
            // Path: B.Start2 -> B.Start1
            points.Add(PlugB.Start2.position); // Ensure we land exactly on B.Start2
            points.Add(PlugB.Start1.position);

            // 4. Update Renderer
            tubeRenderer.SetPositions(points.ToArray());
        }

        private void GenerateSegment(Vector3 p0, Vector3 p3, Vector3 dirA, Vector3 dirB)
        {
            float distance = Vector3.Distance(p0, p3);
            float handleLen = distance * stiffness;

            Vector3 p1 = p0 + dirA * handleLen;
            Vector3 p2 = p3 + dirB * handleLen;

            // Apply Sag (Vertical drop on control points)
            p1.y -= distance * sag;
            p2.y -= distance * sag;
            
            // Generate points (Skipping first and last to avoid duplication with the main loop logic)
            // segments count is per-segment logic here.
            for (int i = 1; i < segments; i++)
            {
                float t = (float)i / segments;
                float u = 1 - t;
                float tt = t * t;
                float uu = u * u;
                float uuu = uu * u;
                float ttt = tt * t;

                Vector3 p = uuu * p0 + 
                            3 * uu * t * p1 + 
                            3 * u * tt * p2 + 
                            ttt * p3;

                points.Add(p);
            }
        }

        public void AddIntermediatePoint(CablePlug sourcePlug, Transform targetTransform)
        {
            if (sourcePlug == null || targetTransform == null) return;

            // Prevent duplicate points at the same location
            float minDistance = 0.05f;
            if (sourcePlug == PlugA && intermediatePoints.Count > 0)
            {
                if (Vector3.Distance(intermediatePoints[0].position, targetTransform.position) < minDistance)
                {
                    return;
                }
            }
            else if (sourcePlug == PlugB && intermediatePoints.Count > 0)
            {
                 if (Vector3.Distance(intermediatePoints[intermediatePoints.Count - 1].position, targetTransform.position) < minDistance)
                 {
                     return;
                 }
            }

            // Create a new anchor point at the target transform"s position
            GameObject anchor = new GameObject($"CablePoint_{intermediatePoints.Count}");
            anchor.transform.SetParent(targetTransform);
            anchor.transform.position = targetTransform.position;
            anchor.transform.rotation = targetTransform.rotation;

            if (sourcePlug == PlugA)
            {
                // If dragging start, insert at the beginning
                intermediatePoints.Insert(0, anchor.transform);
            }
            else if (sourcePlug == PlugB)
            {
                // If dragging end, add to the end
                intermediatePoints.Add(anchor.transform);
            }
            
            // Force redraw
            GenerateCable();
        }

        public void RemoveIntermediatePoint(CablePlug sourcePlug)
        {
            if (sourcePlug == null || intermediatePoints.Count == 0) return;

            Transform targetPoint = null;

            if (sourcePlug == PlugA)
            {
                // If holding A, remove the first point (closest to A)
                targetPoint = intermediatePoints[0];
                intermediatePoints.RemoveAt(0);
            }
            else if (sourcePlug == PlugB)
            {
                // If holding B, remove the last point (closest to B)
                targetPoint = intermediatePoints[intermediatePoints.Count - 1];
                intermediatePoints.RemoveAt(intermediatePoints.Count - 1);
            }

            if (targetPoint != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(targetPoint.gameObject);
                }
                else
                {
                    DestroyImmediate(targetPoint.gameObject);
                }
            }

            GenerateCable();
        }

        public void AddAutoAnchor(CablePlug plug, Vector3 position, Quaternion rotation)
        {
            if (plug == null) return;

            // Create helper gameobject
            GameObject anchor = new GameObject(plug.name + "_AutoAnchor");
            anchor.transform.position = position;
            anchor.transform.rotation = rotation;
            anchor.transform.SetParent(plug.transform); 

            if (plug == PlugA)
            {
                RemoveAutoAnchor(PlugA); // cleanup
                autoAnchorsA.Add(anchor.transform);
                createdAutoAnchorA = true;
            }
            else if (plug == PlugB)
            {
                RemoveAutoAnchor(PlugB); // cleanup
                autoAnchorsB.Add(anchor.transform);
                createdAutoAnchorB = true;
            }

            GenerateCable();
        }

        public void SetAutoAnchor(CablePlug plug, List<Transform> anchors)
        {
            if (plug == null) return;

            // Auto-sort: Ensure the list starts with the point closest to the plug
            List<Transform> sortedAnchors = null;
            if (anchors != null && anchors.Count > 0)
            {
                sortedAnchors = new List<Transform>(anchors);
                if (plug.Start2 != null && sortedAnchors.Count > 1)
                {
                    float distFirst = Vector3.Distance(plug.Start2.position, sortedAnchors[0].position);
                    float distLast = Vector3.Distance(plug.Start2.position, sortedAnchors[sortedAnchors.Count - 1].position);

                    if (distLast < distFirst)
                    {
                        sortedAnchors.Reverse();
                    }
                }
            }

            if (plug == PlugA)
            {
                RemoveAutoAnchor(PlugA);
                if (sortedAnchors != null) autoAnchorsA.AddRange(sortedAnchors);
                createdAutoAnchorA = false; 
            }
            else if (plug == PlugB)
            {
                RemoveAutoAnchor(PlugB);
                if (sortedAnchors != null) autoAnchorsB.AddRange(sortedAnchors);
                createdAutoAnchorB = false; 
            }

            GenerateCable();
        }

        public void RemoveAutoAnchor(CablePlug plug)
        {
            if (plug == null) return;

            List<Transform> targets = null;
            bool wasCreated = false;

            if (plug == PlugA)
            {
                targets = new List<Transform>(autoAnchorsA);
                wasCreated = createdAutoAnchorA;
                autoAnchorsA.Clear();
                createdAutoAnchorA = false;
            }
            else if (plug == PlugB)
            {
                targets = new List<Transform>(autoAnchorsB);
                wasCreated = createdAutoAnchorB;
                autoAnchorsB.Clear();
                createdAutoAnchorB = false;
            }

            if (targets != null && wasCreated)
            {
                foreach (var t in targets)
                {
                    if (t != null)
                    {
                        if (Application.isPlaying) Destroy(t.gameObject);
                        else DestroyImmediate(t.gameObject);
                    }
                }
            }

            GenerateCable();
        }
        public float GetCableLength()
        {
            if (PlugA == null || PlugB == null) return 0f;
            if (PlugA.Start2 == null || PlugB.Start2 == null) return 0f;

            float totalLength = 0f;
            Vector3 currentPos = PlugA.Start2.position;

            List<Transform> effectivePoints = new List<Transform>();

            // Add A Anchors
            if (autoAnchorsA != null && autoAnchorsA.Count > 0) effectivePoints.AddRange(autoAnchorsA);

            // Add Intermediate
            if (intermediatePoints != null) effectivePoints.AddRange(intermediatePoints);

            // Add B Anchors (Reverse)
            if (autoAnchorsB != null && autoAnchorsB.Count > 0)
            {
                for (int i = autoAnchorsB.Count - 1; i >= 0; i--)
                {
                    effectivePoints.Add(autoAnchorsB[i]);
                }
            }

            // Calculate Sum
            foreach (Transform p in effectivePoints)
            {
                if (p != null)
                {
                    totalLength += Vector3.Distance(currentPos, p.position);
                    currentPos = p.position;
                }
            }

            // Final segment to Plug B
            totalLength += Vector3.Distance(currentPos, PlugB.Start2.position);

            return totalLength;
        }
    }
}
