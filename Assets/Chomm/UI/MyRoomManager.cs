using UnityEngine;

public class MyRoomManager : MonoBehaviour
{
    [SerializeField] GameObject roomPrefab;
    [SerializeField] Transform roomContainer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CreateRoom()
    {
        GameObject go =  Instantiate(roomPrefab, roomContainer);
        RoomButton rb =  go.GetComponent<RoomButton>();
        if (rb != null)
        {

        }
    }
}
