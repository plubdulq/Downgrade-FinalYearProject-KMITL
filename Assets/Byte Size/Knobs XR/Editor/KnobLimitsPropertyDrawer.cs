using UnityEngine;
using UnityEditor;

namespace ByteSize.KnobsXR.Editor
{
    [CustomPropertyDrawer(typeof(KnobLimits))]
    public class KnobLimitsPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty useLimit;
        private SerializedProperty limitRange;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ObtainSerializedPropertiesRelativeTo(property);
            DrawUseLimitProperty();
            DrawLimitRangePropertyIf(this.useLimit.boolValue);
        }

        private void ObtainSerializedPropertiesRelativeTo(SerializedProperty parentProperty)
        {
            this.useLimit = parentProperty.FindPropertyRelative("useLimits");
            this.limitRange = parentProperty.FindPropertyRelative("angleRange");
        }

        private void DrawUseLimitProperty()
        {
            this.useLimit.boolValue = EditorGUILayout.Toggle(this.useLimit.displayName, this.useLimit.boolValue);
        }

        private void DrawLimitRangePropertyIf(bool isUseLimitEnabled)
        {
            if (isUseLimitEnabled)
            {
                DrawLimitRangeProperty();
                ClampLimitRange();
            }
        }

        private void DrawLimitRangeProperty()
        {
            this.limitRange.vector2Value = EditorGUILayout.Vector2Field(this.limitRange.displayName, this.limitRange.vector2Value);
        }

        private void ClampLimitRange()
        {
            float limitRangeMin = ClampLimitRangeMinToNegativeRange();
            float limitRangeMax = ClampLimitRangeMaxToPositiveRange();
            this.limitRange.vector2Value = new Vector2(limitRangeMin, limitRangeMax);
        }

        private float ClampLimitRangeMinToNegativeRange()
        {
            return Mathf.Clamp(this.limitRange.vector2Value.x, this.limitRange.vector2Value.x, 0f);
        }

        private float ClampLimitRangeMaxToPositiveRange()
        {
            return Mathf.Clamp(this.limitRange.vector2Value.y, 0f, this.limitRange.vector2Value.y);
        }
    }
}
