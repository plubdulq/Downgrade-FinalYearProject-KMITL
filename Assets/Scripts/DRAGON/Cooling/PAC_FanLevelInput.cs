using UnityEngine;

public class PAC_FanLevelInput : MonoBehaviour
{
    public PAC_AirflowEmitter emitter;

    void Update()
    {
        if (!emitter) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) emitter.fanLevel = 1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) emitter.fanLevel = 2;
        if (Input.GetKeyDown(KeyCode.Alpha3)) emitter.fanLevel = 3;
        if (Input.GetKeyDown(KeyCode.Alpha4)) emitter.fanLevel = 4;
        if (Input.GetKeyDown(KeyCode.Alpha5)) emitter.fanLevel = 5;

        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            emitter.fanLevel = Mathf.Clamp(emitter.fanLevel + 1, 1, 5);

        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            emitter.fanLevel = Mathf.Clamp(emitter.fanLevel - 1, 1, 5);
    }
}
