using UnityEngine;
using Firebase;
using Firebase.Firestore;

public class FirebaseInitializer : MonoBehaviour
{
    public static bool IsReady { get; private set; }
    public static FirebaseFirestore Firestore { get; private set; }

    private static FirebaseApp app;

    private async void Awake()
    {
        DontDestroyOnLoad(gameObject);

        try
        {
            var options = new AppOptions
            {
                ApiKey = "AIzaSyCdPPj18ABteXYYSgBY1jGnMOSDbdu_olw",
                AppId = "1:736576702621:android:7f3c37fc87668dd1c82c74",
                ProjectId = "vr-server-room-app",
                StorageBucket = "vr-server-room-app.firebasestorage.app",
            };

            app = FirebaseApp.Create(options);

            var status = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (status == DependencyStatus.Available)
            {
                Firestore = FirebaseFirestore.GetInstance(app);
                IsReady = true;
                Debug.Log("Firebase initialized successfully with explicit AppOptions.");
            }
            else
            {
                IsReady = false;
                Debug.LogError("Firebase dependency error: " + status);
            }
        }
        catch (System.Exception ex)
        {
            IsReady = false;
            Debug.LogError("Firebase init exception: " + ex);
        }
    }
}