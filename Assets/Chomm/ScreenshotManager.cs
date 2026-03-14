using UnityEngine;
using System.IO;

public class ScreenshotManager : MonoBehaviour
{
    public static ScreenshotManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CaptureAndSave(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("Cannot save screenshot: Room name is empty.");
            return;
        }

        StartCoroutine(CaptureRoutine(roomName));
    }

    private System.Collections.IEnumerator CaptureRoutine(string roomName)
    {
        // Wait for end of frame to ensure UI is rendered (or hidden if we want to hide it)
        // Ideally we might want to hide UI here
        yield return new WaitForEndOfFrame();

        Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
        
        // Save as PNG
        byte[] bytes = texture.EncodeToPNG();
        string filename = roomName + ".png";
        string path = Path.Combine(Application.persistentDataPath, filename);

        File.WriteAllBytes(path, bytes);
        Debug.Log($"Screenshot saved to: {path}");

        // Cleanup
        Destroy(texture);
    }
}
