using UnityEngine;

public class RackHeatReceiver : MonoBehaviour
{
    [Header("Heat Model (simple)")]
    [Tooltip("Current heat in watts-equivalent (game value).")]
    public float heat = 3000f;

    [Tooltip("How fast the rack generates heat per second (game value).")]
    public float heatGenPerSecond = 30f;

    [Tooltip("Clamp heat range")]
    public Vector2 heatClamp = new Vector2(0f, 8000f);

    [Header("Debug")]
    public bool debugDraw = true;

    void Update()
    {
        // simple heat generation
        heat += heatGenPerSecond * Time.deltaTime;
        heat = Mathf.Clamp(heat, heatClamp.x, heatClamp.y);
    }

    /// <summary>
    /// Cooling effect applied by InRow unit.
    /// coolingPower is "how much heat to remove per second" in game value.
    /// </summary>
    public void ApplyCooling(float coolingPower)
    {
        heat -= coolingPower * Time.deltaTime;
        heat = Mathf.Clamp(heat, heatClamp.x, heatClamp.y);
    }

    public float Heat01 => Mathf.InverseLerp(heatClamp.x, heatClamp.y, heat);

    void OnDrawGizmosSelected()
    {
        if (!debugDraw) return;
        Gizmos.color = Color.Lerp(Color.cyan, Color.red, Heat01);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.0f, 0.25f);
    }
}
