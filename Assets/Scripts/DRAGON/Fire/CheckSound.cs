using UnityEngine;

public class AudioSourceHunter : MonoBehaviour
{
    [Tooltip("Log เฉพาะ audio ที่กำลังเล่น หรือ time>0 (บางคลิปเริ่มแล้วหยุด)")]
    public bool logOnlyIfPlayingOrTime = true;

    void Update()
    {
        // Unity 6 แนะนำ FindObjectsByType
        var sources = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var s in sources)
        {
            if (!s) continue;

            bool interesting = s.isPlaying || s.time > 0f;
            if (logOnlyIfPlayingOrTime && !interesting) continue;

            Debug.Log(
                $"[AUDIO] {GetPath(s.transform)} | playing={s.isPlaying} | " +
                $"clip={(s.clip ? s.clip.name : "NULL")} | playOnAwake={s.playOnAwake} | loop={s.loop} | " +
                $"spatialBlend={s.spatialBlend}"
            );
        }
    }

    string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}