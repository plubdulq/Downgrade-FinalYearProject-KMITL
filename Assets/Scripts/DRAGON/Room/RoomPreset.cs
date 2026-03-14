using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomPreset", menuName = "ServerRoom/RoomPreset")]
public class RoomPreset : ScriptableObject
{
    [Header("UI")]
    public string presetName;
    public Sprite thumbnail;

    [Header("Default Size (meters)")]
    public float width  = 4f;
    public float length = 6f;
    public float height = 2.5f;

    public enum RoomShape
    {
        Rectangle,
        LShape,
        Modular
    }

    [Header("Room Shape")]
    public RoomShape shape = RoomShape.Rectangle;

    [Header("Default Interior Option")]
    public bool hasSampleEquipments = false;
}
