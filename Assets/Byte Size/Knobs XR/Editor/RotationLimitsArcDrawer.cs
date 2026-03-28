using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ByteSize.KnobsXR.Editor
{
    public class RotationLimitsArcDrawerArg
    {
        public Vector2 AngleLimits => angleLimits;
        private Vector2 angleLimits;

        public Vector3 Position => position;
        private Vector3 position;

        public Vector3 Forward => forward;
        private Vector2 forward;

        public Vector3 Up => up;
        private Vector3 up;

        public RotationLimitsArcDrawerArg(Vector2 angleLimits, Vector3 position, Vector3 forward, Vector3 up)
        {
            this.angleLimits = angleLimits;
            this.position = position;
            this.forward = forward;
            this.up = up;
        }
    }
    public class RotationLimitsArcDrawer
    {
        private static readonly Color arcColor = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.25f);

        public static void Draw(RotationLimitsArcDrawerArg arg)
        {
            Vector3 start = Quaternion.AngleAxis(arg.AngleLimits.x, arg.Forward) * arg.Up;
            float angleOffset = Vector3.Angle(start, arg.Up);

            Handles.color = arcColor;
            Handles.DrawSolidArc(
                arg.Position,
                arg.Forward,
                start,
                angleOffset + arg.AngleLimits.y,
                1f
            );
        }
    }
}
