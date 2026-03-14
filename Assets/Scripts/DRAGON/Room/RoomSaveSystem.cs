using System.IO;
using UnityEngine;

public static class RoomSaveSystem
{
    private static string FileName = "rooms.json";

    private static string GetFilePath()
    {
        return Path.Combine(Application.persistentDataPath, FileName);
    }

    public static RoomSaveDatabase LoadDatabase()
    {
        string path = GetFilePath();
        if (!File.Exists(path))
        {
            return new RoomSaveDatabase();
        }

        string json = File.ReadAllText(path);
        if (string.IsNullOrEmpty(json))
        {
            return new RoomSaveDatabase();
        }

        RoomSaveDatabase db = JsonUtility.FromJson<RoomSaveDatabase>(json);
        if (db == null) db = new RoomSaveDatabase();
        return db;
    }

    public static void SaveDatabase(RoomSaveDatabase db)
    {
        string path = GetFilePath();
        string json = JsonUtility.ToJson(db, true);
        File.WriteAllText(path, json);
        Debug.Log($"[RoomSaveSystem] Saved {db.rooms.Count} rooms to {path}");
    }

    // เพิ่มห้องใหม่แล้วเซฟเลย (ใช้ตอนจบ Wizard ปุ่ม create)
    public static void AddRoom(RoomSaveData newRoom)
    {
        RoomSaveDatabase db = LoadDatabase();
        db.rooms.Add(newRoom);
        SaveDatabase(db);
    }

    // ลบห้อง (ใช้กับปุ่มถังขยะ)
    public static void DeleteRoom(string roomId)
    {
        RoomSaveDatabase db = LoadDatabase();
        db.rooms.RemoveAll(r => r.id == roomId);
        SaveDatabase(db);
    }
}
