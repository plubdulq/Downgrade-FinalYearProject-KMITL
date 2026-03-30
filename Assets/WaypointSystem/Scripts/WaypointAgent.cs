using System;
using System.Collections.Generic;
using UnityEngine;


namespace WaypointSystem {
    public class WaypointAgent : MonoBehaviour {
        // for some sort of reason it will make one additional movement before saying it reached its path

        [Tooltip("The last waypoint that was visited or the start of the path.")]
        public Waypoint pathToFollow;
        public GameObject wp;
        public string wpName;

        /// <summary>
        /// The current objective containing waypoint and t-value info.
        /// Null if no objective is currently set.
        /// </summary>
        public NavigationInfo _currentObjective { get; private set; } = null;

        [Tooltip("Movement speed of the agent.")]
        public float speed = 5;
         public float chaneSpped = 1;

        [Header("Path Following Offsets")]
        [Tooltip("Preserve original X position while following path.")]
        public bool OffsetX = false;

        [Tooltip("Preserve original Y position while following path.")]
        public bool OffsetY = false;

        [Tooltip("Preserve original Z position while following path.")]
        public bool OffsetZ = false;

        /// <summary>
        /// Invoked when the current objective is reached.
        /// </summary>
        public event Action OnObjectiveReached;

        [Header("Behavior Settings")]
        [Tooltip("If true, agent will rotate to face movement direction.")]
        public bool AdjustRotation = true;

        [Tooltip("Controls whether the agent is actively processing instructions.")]
        public bool Working = true;

        /// <summary>
        /// The current forward rotation direction vector of the agent.
        /// </summary>
        public Vector3 Rotation { get; private set; }

        public bool ExecuteFirstInstructionOnStart = false;
        /// <summary>
        /// Index of the current instruction being executed.
        /// </summary>
        [HideInInspector] public int CurrentInstructionIndex = 0;

        /// <summary>
        /// The full list of movement and behavior instructions for the agent.
        /// </summary>
        public List<Instruction> InstructionList { get; private set; } = new List<Instruction>();

        [Header("Path Join Settings")]
        [SerializeField, Tooltip("If true, the agent will attempt to join the path from its current position.")]
        private bool joinTrack = false;

        [SerializeField, Tooltip("If true, the agent will follow the waypoint path normally.")]
        private bool FollowWaypoint = false;


        private bool finished = false; // would be changed to attained objective
        public float currentTValue = 0;
        private Vector3 LastTrackPosition;
        private float? _traveledDistance = null;
        void OnDrawGizmos() {
            Color c = Gizmos.color;
            Gizmos.color = Color.yellow;

            // enable this to make a yellow cube over the current waypoint of this agent
            //Gizmos.DrawCube(pathToFollow.transform.position, new Vector3(1, 1, 1));

            Gizmos.color = Color.green;
            if (_currentObjective != null && _currentObjective.waypoint != null) {
                // enable this to make a green cube over the current objective waypoint of this agent
                //Gizmos.DrawCube(_currentObjective.waypoint.transform.position + new Vector3(0, 1, 0), new Vector3(1, 1, 1));
            }

            Gizmos.color = c;


        }

        void OnValidate() {
            if (joinTrack && FollowWaypoint) FollowWaypoint = false;
        }


        private void Start() {
            if(wpName == "W1".ToString()){
            wp = GameObject.Find("W1");
            wp.GetComponent<Waypoint>();
            }else if(wpName == "W2".ToString()){
            wp = GameObject.Find("W2");
            wp.GetComponent<Waypoint>();
            }else if(wpName == "W3".ToString()){
            wp = GameObject.Find("W3");
            wp.GetComponent<Waypoint>();
            }else if(wpName == "W4".ToString()){
            wp = GameObject.Find("W4");
            wp.GetComponent<Waypoint>();
            }else if(wpName == "W5".ToString()){
            wp = GameObject.Find("W5");
            wp.GetComponent<Waypoint>();
            }else if(wpName == "W6".ToString()){
            wp = GameObject.Find("W6");
            wp.GetComponent<Waypoint>();
            }
            pathToFollow = wp.GetComponent<Waypoint>();
            LastTrackPosition = pathToFollow.GetNextPosition(0, ref currentTValue, out pathToFollow);

            if (ExecuteFirstInstructionOnStart) ExecuteNextInstruction();



        }
        private void Update() {
            if (!Working || speed.Equals(0)) return;

            if (finished || (_traveledDistance != null && _traveledDistance <= 0)) {
                ExecuteNextInstruction();
                return;
            }

            if (FollowWaypoint) FollowPath();
            else if (joinTrack) JoinPath(false);
        }
        private void LateUpdate() {
            if (AdjustRotation) transform.rotation = Quaternion.LookRotation(Rotation);
        }


        #region handle instruction
        public void ExecuteNextInstruction() {
            // resetting the past objectives;
            _currentObjective = null;
            _traveledDistance = null;
            if (CurrentInstructionIndex >= InstructionList.Count) {
                joinTrack = false;
                FollowWaypoint = false;
            }
            if (InstructionList.Count == 0 || CurrentInstructionIndex >= InstructionList.Count) {
                Debug.LogWarning("the list or instruction index is out of bounds");
                return;
            }

            finished = false;
            OnObjectiveReached?.Invoke();
            Instruction type = InstructionList[CurrentInstructionIndex];
            CurrentInstructionIndex++;

            switch (type.Type) {
                case InstructionType.Stop:
                    Working = false;
                    break;
                case InstructionType.SetOffsetX:
                    if (type.value.HasValue) {
                        OffsetX = type.value == 0 ? false : true;
                    }
                    ExecuteNextInstruction();
                    break;
                case InstructionType.SetOffsetY:
                    if (type.value.HasValue) {
                        OffsetY = type.value == 0 ? false : true;
                    }
                    ExecuteNextInstruction();
                    break;
                case InstructionType.SetOffsetZ:
                    if (type.value.HasValue) {
                        OffsetZ = type.value != 0;
                    }
                    ExecuteNextInstruction();
                    break;
                case InstructionType.ChangeSpeed:
                   // if (type.value.HasValue) {
                       // speed = 1;//type.value.Value; 
                   // }
                    speed = chaneSpped; 
                    ExecuteNextInstruction();
                    break;

                case InstructionType.GoToInstructionIndex:
                    if (type.value.HasValue && (int)type.value.Value >= 0 && (int)type.value.Value <= InstructionList.Count) {

                        CurrentInstructionIndex = (int)type.value.Value;
                    }
                    ExecuteNextInstruction();
                    break;
                case InstructionType.AllowRotation:
                    if (type.value.HasValue) {
                        AdjustRotation = type.value != 0;
                    }
                    ExecuteNextInstruction();
                    break;
                case InstructionType.TravelSomeSeconds:
                    if (type.value.HasValue) {
                        FollowWaypoint = true;
                        Invoke("ExecuteNextInstruction", type.value.Value);

                    }
                    else ExecuteNextInstruction();
                    break;
                case InstructionType.JoinPath:
                    joinTrack = true;
                    FollowWaypoint = false;
                    break;

                case InstructionType.WaitSeconds:
                    if (type.value.HasValue) {
                        Working = false;
                        Invoke("ExecuteBoth", type.value.Value);

                    }
                    else ExecuteNextInstruction();
                    break;

                case InstructionType.GoNthWaypoints:
                    if (type.value.HasValue) {
                        joinTrack = false;
                        FollowWaypoint = true;
                        int val = (int)type.value.Value;
                        Waypoint w = pathToFollow.getNthNextWaypoint(val);

                        _currentObjective = new NavigationInfo(0, null);
                        _currentObjective.waypoint = w;
                        _currentObjective.TValue = Mathf.Abs(type.value.Value - val);
                        if (w.next == null) _currentObjective.TValue = 0;
                    }
                    else ExecuteNextInstruction();
                    break;
                case InstructionType.GoToEnd:
                    _currentObjective = null;
                    FollowWaypoint = true;
                    break;
                case InstructionType.TravelSomeDistance:
                    _currentObjective = null;
                    if (type.value.HasValue) {
                        _traveledDistance = type.value.Value;
                    }
                    else ExecuteNextInstruction();
                    break;
            }
        }


        #endregion handle instruction

        private void FollowPath() {
            float distanceToTravel = Time.deltaTime * speed;

            float lastTValue = currentTValue;
            Waypoint lastWaypoint = pathToFollow;

            Vector3 newPosition = pathToFollow.GetNextPosition(distanceToTravel, ref currentTValue, out pathToFollow);


            bool prev = pathToFollow.previous == lastWaypoint;
            bool next = pathToFollow.next == lastWaypoint;
            if (lastWaypoint != pathToFollow) {

                if (prev) {
                    currentTValue = 0;
                    newPosition = pathToFollow.transform.position;
                }
                else if (next) {
                    currentTValue = 1;
                    newPosition = lastWaypoint.transform.position;

                }
            }

            bool endCondition = false;
            if (_currentObjective != null && _currentObjective.waypoint != null) {

                // Movement bounds
                float lower = Mathf.Min(lastTValue, currentTValue);
                float upper = Mathf.Max(lastTValue, currentTValue);
                float ObjTValue = _currentObjective.TValue;


                // Case 1: Stills on same waypoint
                if (lastWaypoint == pathToFollow && lastWaypoint == _currentObjective.waypoint) {
                    endCondition = _currentObjective.TValue >= lower && _currentObjective.TValue <= upper;
                }
                // Case 2: Just crossed a waypoint that contains the objective // if one of the two was the objective

                else if (pathToFollow == _currentObjective.waypoint) {
                    if (speed > 0 && (currentTValue >= ObjTValue || Mathf.Approximately(currentTValue, ObjTValue))) endCondition = true;
                    else if (speed < 0 && (currentTValue <= ObjTValue || Mathf.Approximately(currentTValue, ObjTValue))) endCondition = true;
                }
                else if (lastWaypoint == _currentObjective.waypoint) {
                    if (speed > 0 && (lastTValue <= ObjTValue || Mathf.Approximately(lastTValue, ObjTValue))) endCondition = true;
                    else if (speed < 0 && (lastTValue >= ObjTValue || Mathf.Approximately(lastTValue, ObjTValue))) endCondition = true;
                }
            }
            // issue is that when you reach the end, it will put you at the waypoint objective as if you reached it
            if (/*(Mathf.Approximately(currentTValue, lastTValue) && lastWaypoint == pathToFollow) || */endCondition) {
                // will need to adjust the position to be exact
                finished = true;
                if (_currentObjective == null || _currentObjective.waypoint == null) return;

                pathToFollow = _currentObjective.waypoint;
                currentTValue = _currentObjective.TValue;
                Vector3 position = pathToFollow.getVector3FromTValue(currentTValue);
                if (OffsetX) position.x = transform.position.x;
                if (OffsetY) position.y = transform.position.y;
                if (OffsetZ) position.z = transform.position.z;
                transform.position = position;

                return;
            }




            Vector3 deltaPosition = newPosition - LastTrackPosition;

            Vector3 rectify = transform.position;

            if (!OffsetX) rectify.x = LastTrackPosition.x;
            if (!OffsetY) rectify.y = LastTrackPosition.y;
            if (!OffsetZ) rectify.z = LastTrackPosition.z;

            rectify += deltaPosition;
            _traveledDistance -= deltaPosition.magnitude;
            Rotation = deltaPosition;

            transform.position = rectify;

            LastTrackPosition = newPosition;
        }

        public void JoinPath(bool immediately) {
            if (pathToFollow == null) {
                Debug.LogWarning("pathToFollow is null");
                return;
            }

            Vector3 closest = pathToFollow.GetClosestPointOnPath(transform.position, true, true);

            if (OffsetX) closest.x = transform.position.x;
            if (OffsetY) closest.y = transform.position.y;
            if (OffsetZ) closest.z = transform.position.z;

            float step = Mathf.Abs(speed) * Time.deltaTime;
            Vector3 newPosition = Vector3.MoveTowards(transform.position, closest, step);
            Rotation = (closest - transform.position).normalized;

            _traveledDistance -= Vector3.Distance(transform.position, newPosition);
            transform.position = newPosition;

            if (immediately || Vector3.Distance(transform.position, closest) < 0.001f) {
                joinTrack = false;
                finished = true;

                transform.position = closest;
                NavigationInfo info = pathToFollow.GetWaypointAndTValueFromVector3(closest);
                currentTValue = info.TValue;
                pathToFollow = info.waypoint;

                LastTrackPosition = closest;
            }
            else {
                joinTrack = true;
            }
        }



        public bool ReachedEnd() {
            return (pathToFollow.next == null);
        }


        public bool IsTravelComplete() {
            return !finished;
        }

        public bool IsOnPath(float buffer) {
            Vector3 distance = pathToFollow.GetClosestPointOnPath(transform.position, true, true);
            // need to consider the offset
            if (OffsetX) distance.x = transform.position.x;
            if (OffsetY) distance.y = transform.position.y;
            if (OffsetZ) distance.z = transform.position.z;

            return Vector3.Distance(distance, transform.position) <= buffer;

        }

        private void ExecuteBoth() {
            Working = true;
            ExecuteNextInstruction();
        }
    }
}