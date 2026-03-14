using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomSizeUIController : MonoBehaviour
{
    [Header("Refs")]
    public RoomGeneratorDragon generator;

    public TMP_InputField inputX;
    public TMP_InputField inputY;
    public TMP_InputField inputZ;

    public Button btnGenerate;
    public Button btnClear;

    [Header("Defaults")]
    public float defaultX = 5f;
    public float defaultY = 3f; // height
    public float defaultZ = 8f; // length

    private void Awake()
    {
        if (btnGenerate != null) btnGenerate.onClick.AddListener(OnClickGenerate);
        if (btnClear != null) btnClear.onClick.AddListener(OnClickClear);

        if (inputX != null && string.IsNullOrWhiteSpace(inputX.text)) inputX.text = defaultX.ToString();
        if (inputY != null && string.IsNullOrWhiteSpace(inputY.text)) inputY.text = defaultY.ToString();
        if (inputZ != null && string.IsNullOrWhiteSpace(inputZ.text)) inputZ.text = defaultZ.ToString();
    }

    public void OnClickGenerate()
    {
        if (generator == null)
        {
            Debug.LogWarning("[RoomSizeUIController] generator is null");
            return;
        }

        float w = Read(inputX, defaultX);
        float h = Read(inputY, defaultY);
        float l = Read(inputZ, defaultZ);

        // ใช้ preset เดิมเป็นแม่แบบ shape แต่ override ขนาดด้วย input
        generator.GenerateFromPreset(
            generator.debugPreset,
            generator.debugInterior,
            overrideWidth: w,
            overrideLength: l,
            overrideHeight: h
        );
    }

    public void OnClickClear()
    {
        if (generator != null) generator.ClearRoom();
    }

    private float Read(TMP_InputField f, float fallback)
    {
        if (f == null) return fallback;
        return float.TryParse(f.text, out float v) ? v : fallback;
    }
}
