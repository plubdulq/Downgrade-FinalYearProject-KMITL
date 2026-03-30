using UnityEngine;
namespace WaypointSystem {
    public class Instruction {
        public InstructionType Type;
        public float? value;

        private Instruction(InstructionType type, float? number = null) {
            Type = type;
            value = number;
        }
        public static Instruction Create(InstructionType type, float? value = null) {
            bool needsValue = type switch {
                InstructionType.GoNthWaypoints or
                InstructionType.WaitSeconds or
                InstructionType.TravelSomeDistance or
                InstructionType.ChangeSpeed or
                InstructionType.SetOffsetX or
                InstructionType.SetOffsetY or
                InstructionType.SetOffsetZ or
                InstructionType.GoToInstructionIndex or
                InstructionType.AllowRotation or
                InstructionType.TravelSomeSeconds => true,
                _ => false
            };

            if (needsValue && value == null)
                Debug.LogWarning($"{type} instruction created without value. It may be ignored later.");

            if (!needsValue && value != null)
                Debug.LogWarning($"{type} instruction does not need a value. Value will be ignored.");

            return new Instruction(type, value);
        }
    }
    /// <summary>
    /// Defines the different types of movement or behavior instructions for a WaypointAgent.
    /// </summary>
    public enum InstructionType {

        /// <summary>
        /// Move forward along the path for a specified distance.
        /// </summary>
        TravelSomeDistance,

        /// <summary>
        /// Move forward until the end of the path is reached.
        /// </summary>
        GoToEnd,

        /// <summary>
        /// Move forward a specific number of waypoints.
        /// The whole number indicates waypoints to traverse; the decimal represents a t-value offset.
        /// </summary>
        GoNthWaypoints,

        /// <summary>
        /// Join the path from the current position and realign to it.
        /// </summary>
        JoinPath,

        /// <summary>
        /// Pause for a number of seconds before continuing.
        /// </summary>
        WaitSeconds,

        /// <summary>
        /// Stop executing further instructions.
        /// </summary>
        Stop,

        /// <summary>
        /// Change the movement speed to a specified value.
        /// </summary>
        ChangeSpeed,

        /// <summary>
        /// Enable or disable the X-axis position offset while following the path.
        /// </summary>
        SetOffsetX,

        /// <summary>
        /// Enable or disable the Y-axis position offset while following the path.
        /// </summary>
        SetOffsetY,

        /// <summary>
        /// Enable or disable the Z-axis position offset while following the path.
        /// </summary>
        SetOffsetZ,

        /// <summary>
        /// Jump to a specific instruction index in the instruction list.
        /// </summary>
        GoToInstructionIndex,

        /// <summary>
        /// Move forward for a number of seconds based on current speed.
        /// </summary>
        TravelSomeSeconds,

        /// <summary>
        /// Enable or disable rotation adjustment during movement.
        /// </summary>
        AllowRotation
    }
}