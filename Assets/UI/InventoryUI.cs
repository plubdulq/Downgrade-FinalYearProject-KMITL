using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private PlacementPreview placementPreview;

    [Header("Prefab Buttons")]
    [SerializeField] private Button rackButton;
    [SerializeField] private Button switchButton;
    [SerializeField] private Button serverButton;

    [Header("Prefabs")]
    [SerializeField] private GameObject rackPrefab;
    [SerializeField] private GameObject switchPrefab;
    [SerializeField] private GameObject serverPrefab;

    private void Start()
    {
        rackButton.onClick.AddListener(() => SelectPrefab(rackPrefab));
        switchButton.onClick.AddListener(() => SelectPrefab(switchPrefab));
        serverButton.onClick.AddListener(() => SelectPrefab(serverPrefab));
    }

    private void SelectPrefab(GameObject prefab)
    {
        placementPreview.BeginPlacement(prefab);
    }
}
