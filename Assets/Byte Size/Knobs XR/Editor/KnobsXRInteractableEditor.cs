using System;
using UnityEngine;
using UnityEditor;

namespace ByteSize.KnobsXR.Editor
{
    [CustomEditor(typeof(KnobsXRInteractable))]
    public class KnobsXRInteractableEditor : UnityEditor.Editor
    {
        private readonly static string foldoutNameForBaseInteractableProperties = "Base Interactable Properties";
        private readonly static Color rotationLimitsColor = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.25f);
        private readonly static Color upDirectionColor = Color.yellow;
        private readonly static float upDirectionArrowLength = 1.25f;
        
        private SerializedProperty knobLimits;
        private SerializedProperty useKnobLimits;
        private SerializedProperty initialDirections;
        private Transform transform;
        private Action DrawRotationLimitsArcStrategy;

        private bool showInteractionEvents = false;

        private void OnEnable()
        {
            ObtainSerializedProperties();

            this.transform = (target as KnobsXRInteractable).transform;

            DetermineStrategyToDrawRotationLimitsArc(EditorApplication.isPlayingOrWillChangePlaymode);
            EditorApplication.playModeStateChanged += DetermineStrategyToDrawRotationLimitsArc;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= DetermineStrategyToDrawRotationLimitsArc;
        }

        public override void OnInspectorGUI()
        {
            DrawFoldoutForBaseInteractableProperties();
            
            // Force show knob limits here so it doesn't appear under the base interactable properties foldout
            EditorGUILayout.PropertyField(this.knobLimits);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFoldoutForBaseInteractableProperties()
        {
            this.showInteractionEvents = EditorGUILayout.Foldout(this.showInteractionEvents, foldoutNameForBaseInteractableProperties);
            if (this.showInteractionEvents)
            {
                base.OnInspectorGUI();
            }
        }

        public void OnSceneGUI()
        {
            if (Event.current.type != EventType.Repaint) { return; }

            DrawUpDirectionArrow();
            DrawRotationLimitsArcIf(this.useKnobLimits.boolValue);
        }

        private void DrawRotationLimitsArcIf(bool shouldDraw)
        {
            if (shouldDraw)
            {
                DrawRotationLimitsArcStrategy();
            }
        }

        private void DetermineStrategyToDrawRotationLimitsArc(PlayModeStateChange playModeState)
        {
            DetermineStrategyToDrawRotationLimitsArc(playModeState == PlayModeStateChange.EnteredPlayMode);
        }

        private void DetermineStrategyToDrawRotationLimitsArc(bool isInPlayMode)
        {
            if (isInPlayMode)
            {
                this.DrawRotationLimitsArcStrategy = DrawRotationLimitsArcInPlayMode;
            }
            else
            {
                this.DrawRotationLimitsArcStrategy = DrawRotationLimitsArcInEditMode;
            }
        }

        private void DrawRotationLimitsArcInPlayMode()
        {
            Vector2 knobAngleLimits = this.knobLimits.FindPropertyRelative("angleRange").vector2Value;

            Vector3 initialForward = this.initialDirections.FindPropertyRelative("initialForward").vector3Value;
            Vector3 initialUp = this.initialDirections.FindPropertyRelative("initialUp").vector3Value;

            Vector3 start = Quaternion.AngleAxis(knobAngleLimits.x, initialForward) * initialUp;
            float angleOffset = Vector3.Angle(start, initialUp);

            Handles.color = rotationLimitsColor;
            Handles.DrawSolidArc(
                this.transform.position,
                initialForward,
                start,
                angleOffset + knobAngleLimits.y,
                1f
            );
        }

        private void DrawRotationLimitsArcInEditMode()
        {
            Vector2 knobAngleLimits = this.knobLimits.FindPropertyRelative("angleRange").vector2Value;

            Vector3 start = Quaternion.AngleAxis(knobAngleLimits.x, this.transform.forward) * this.transform.up;
            float angleOffset = Vector3.Angle(start, this.transform.up);

            Handles.color = rotationLimitsColor;
            Handles.DrawSolidArc(
                this.transform.position,
                this.transform.forward,
                start,
                angleOffset + knobAngleLimits.y,
                1f
            );
        }

        private void DrawUpDirectionArrow()
        {
            Vector3 up = this.transform.position + (this.transform.up * upDirectionArrowLength);
            Handles.color = upDirectionColor;
            Handles.DrawLine(this.transform.position, up);
        }

        private void ObtainSerializedProperties()
        {
            this.knobLimits = serializedObject.FindProperty("knobLimits");
            this.useKnobLimits = this.knobLimits.FindPropertyRelative("useLimits");
            this.initialDirections = serializedObject.FindProperty("initialDirections");
        }
    }
}
