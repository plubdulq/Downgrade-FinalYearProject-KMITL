using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEngine;

public class SelectCategoryButton : MonoBehaviour
{
    [SerializeField] private string categoryName;
    [SerializeField] private StoreController storeController;
    public async void OnClick()
    {
        //CategoryFilterController.Instance.UpdateStoreByCategory(categoryName);
        await storeController.UpdateStoreByCategory(categoryName);
        Debug.Log($"Category button clicked: {categoryName}");
    }
}
