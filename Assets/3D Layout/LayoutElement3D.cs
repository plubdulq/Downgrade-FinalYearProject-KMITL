using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutElement3D : MonoBehaviour
{
    [HideInInspector]
    public bool IgnoreLayout = false;
    [HideInInspector]
    public Vector3 ElementDimensions;
    [HideInInspector]
    public bool DrawDimensionGizmos = false;

    public LayoutGroup3D ParentLayoutGroup
    {
        get
        {
            if(ParentLayoutInternal == null)
            {
                ParentLayoutInternal = GetComponentInParent<LayoutGroup3D>();
            }

            return ParentLayoutInternal;
        }

        set
        {
            ParentLayoutInternal = value;
        }
    }

    private LayoutGroup3D ParentLayoutInternal;

    public Vector3 GetDimensions()
    {
        return ElementDimensions;
    }

    public void SetDimensions(Vector3 NewDimensions)
    {
        ElementDimensions = NewDimensions;

        if(ParentLayoutGroup != null)
        {
            ParentLayoutGroup.RebuildLayout();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!DrawDimensionGizmos)
        {
            return;
        }

        Gizmos.color = IgnoreLayout ? Color.red : Color.blue;
        // Draw box with element dimensions
        Vector3 dimensions = GetDimensions();
        Vector3 xOffset = transform.right * dimensions.x;
        Vector3 yOffset = transform.up * dimensions.y;
        Vector3 zOffset = transform.forward * dimensions.z;

        Vector3 corner0 = transform.position + xOffset / 2f + yOffset / 2f + zOffset / 2f;
        Gizmos.DrawLine(corner0, corner0 - xOffset);
        Gizmos.DrawLine(corner0, corner0 - yOffset);
        Gizmos.DrawLine(corner0, corner0 - zOffset);
        Vector3 corner1 = corner0 - (xOffset + zOffset);
        Gizmos.DrawLine(corner1, corner1 + zOffset);
        Gizmos.DrawLine(corner1, corner1 + xOffset);
        Gizmos.DrawLine(corner1, corner1 - yOffset);
        Vector3 corner2 = corner0 - (yOffset + xOffset);
        Gizmos.DrawLine(corner2, corner2 + yOffset);
        Gizmos.DrawLine(corner2, corner2 - zOffset);
        Gizmos.DrawLine(corner2, corner2 + xOffset);
        Vector3 corner3 = corner0 - (yOffset + zOffset);
        Gizmos.DrawLine(corner3, corner3 + zOffset);
        Gizmos.DrawLine(corner3, corner3 + yOffset);
        Gizmos.DrawLine(corner3, corner3 - xOffset);
    }
}
