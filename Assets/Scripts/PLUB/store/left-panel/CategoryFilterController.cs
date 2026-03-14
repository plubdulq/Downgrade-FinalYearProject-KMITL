using System.Net;
using UnityEngine;

public class CategoryFilterController : MonoBehaviour
{
    public static CategoryFilterController Instance;

    public void Awake()
    {
        Instance = this;
    }

    public void UpdateStoreByCategory(string categoryName)
    {
        Debug.Log($"Store updated to show category: {categoryName}");
        // Add logic to update the store display based on the selected category
    }
}
