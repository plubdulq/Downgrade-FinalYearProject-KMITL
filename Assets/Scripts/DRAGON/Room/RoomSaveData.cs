using System;
using System.Collections.Generic;

[Serializable]
public class RoomSaveData
{
    public string id;          // GUID
    public string name;        // ชื่อห้องที่ user ตั้ง

    // ขนาดห้อง
    public float width;
    public float length;
    public float height;

    // ข้อมูล preset / รูปทรง
    public string presetName;                 // เช่น "Compact Node Room"
    public RoomPreset.RoomShape shape;

    // ตัวเลือกภายในห้อง (หน้า Room Option)
    public InteriorContent interiorContent;   // ห้องเปล่า / มีอุปกรณ์ตัวอย่างให้

    // เวลาเซฟ
    public long createdAtUnix;

    public RoomSaveData() { }

    public RoomSaveData(
        string name,
        float width,
        float length,
        float height,
        string presetName,
        RoomPreset.RoomShape shape,
        InteriorContent interiorContent)
    {
        this.id = Guid.NewGuid().ToString();
        this.name = name;
        this.width = width;
        this.length = length;
        this.height = height;
        this.presetName = presetName;
        this.shape = shape;
        this.interiorContent = interiorContent;
        this.createdAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

// ===== option สำหรับ “ภายในห้อง” =====
[Serializable]
public enum InteriorContent
{
    EmptyRoom,       // ห้องเปล่า
    SampleEquipments // มีอุปกรณ์ตัวอย่างให้
}

// ===== database wrapper สำหรับเซฟลง JSON =====
[Serializable]
public class RoomSaveDatabase
{
    public List<RoomSaveData> rooms = new List<RoomSaveData>();
}
