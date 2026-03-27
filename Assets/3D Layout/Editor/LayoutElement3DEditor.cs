#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LayoutElement3D))]
public class LayoutElement3DEditor : Editor
{
    private LayoutElement3D Element;

    private Vector3 ElementDimensions;
    private bool IgnoreLayout;
    private bool DrawDimensionGizmos;

    public override void OnInspectorGUI()
    {

        Element = target as LayoutElement3D;
        DrawDefaultInspector();
        bool shouldRebuild = false;

        // Element Dimensions
        EditorGUI.BeginChangeCheck();

        ElementDimensions = EditorGUILayout.Vector3Field(new GUIContent("Element Dimensions", ToolTip_ElemDimensions), Element.ElementDimensions);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(Element, "Change Element Dimensions");
            Element.ElementDimensions = ElementDimensions;
            shouldRebuild = true;
        }

        // Ignore layout
        EditorGUI.BeginChangeCheck();
        IgnoreLayout = EditorGUILayout.Toggle(new GUIContent("Ignore Layout"), Element.IgnoreLayout);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(Element, "Change Element Ignore Layout");
            Element.IgnoreLayout = IgnoreLayout;
            shouldRebuild = true;
        }

        if(shouldRebuild)
        {
            // Try to get parent layout group and tell it to rebuild
            LayoutGroup3D parentLayout = Element.GetComponentInParent<LayoutGroup3D>();
            if (parentLayout != null)
            {
                parentLayout.RebuildLayout();
            }
        }

        EditorGUI.BeginChangeCheck();
        DrawDimensionGizmos = EditorGUILayout.Toggle(new GUIContent("Draw Dimension Gizmos"), Element.DrawDimensionGizmos);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(Element, "Change Element Draw Gizmos");
            Element.DrawDimensionGizmos = DrawDimensionGizmos;
        }
    }

    private const string ToolTip_ElemDimensions = "The size of this element in local space used to determine the distance between their pivot points in the layout.  Only used in Linear Layouts.";
}

#endif
