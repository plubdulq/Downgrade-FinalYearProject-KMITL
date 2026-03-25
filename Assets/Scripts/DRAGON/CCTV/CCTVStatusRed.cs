using UnityEngine;

public class CCTVStatusLED : MonoBehaviour
{
    [Header("Target")]
    public Renderer ledRenderer;

    [Header("Blink Settings")]
    public bool ledEnabled = true;
    public float blinkSpeed = 2f;

    [Header("Colors")]
    public Color offColor = new Color(0.15f, 0f, 0f, 1f);
    public Color onColor = new Color(1f, 0f, 0f, 1f);

    [Header("Emission")]
    public bool useEmission = true;
    public Color offEmission = new Color(0.05f, 0f, 0f, 1f);
    public Color onEmission = new Color(2f, 0f, 0f, 1f);

    private Material runtimeMat;

    private void Awake()
    {
        if (ledRenderer == null)
            ledRenderer = GetComponent<Renderer>();

        if (ledRenderer != null)
            runtimeMat = ledRenderer.material;
    }

    private void OnEnable()
    {
        UpdateLEDImmediate();
    }

    private void Update()
    {
        if (runtimeMat == null)
            return;

        if (!ledEnabled)
        {
            ApplyColor(offColor, offEmission);
            return;
        }

        float t = (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;

        Color baseColor = Color.Lerp(offColor, onColor, t);
        Color emissionColor = Color.Lerp(offEmission, onEmission, t);

        ApplyColor(baseColor, emissionColor);
    }

    public void SetLEDEnabled(bool value)
    {
        ledEnabled = value;
        UpdateLEDImmediate();
    }

    private void UpdateLEDImmediate()
    {
        if (runtimeMat == null)
            return;

        if (ledEnabled)
            ApplyColor(onColor, onEmission);
        else
            ApplyColor(offColor, offEmission);
    }

    private void ApplyColor(Color baseColor, Color emissionColor)
    {
        runtimeMat.color = baseColor;

        if (useEmission)
        {
            runtimeMat.EnableKeyword("_EMISSION");
            runtimeMat.SetColor("_EmissionColor", emissionColor);
        }
    }
}