using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloorUIManager : MonoBehaviour
{
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private GameObject floorButtonPrefab;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private int maxFloors = 3;

    void Start()
    {
        Debug.Log("Building Manager reference: " + (buildingManager != null));
        Debug.Log("Button Prefab reference: " + (floorButtonPrefab != null));
        Debug.Log("Button Container reference: " + (buttonContainer != null));
        CreateFloorButtons();
    }

    private void CreateFloorButtons()
    {
        for (int i = 0; i < maxFloors; i++)
        {
            GameObject buttonObj = Instantiate(floorButtonPrefab, buttonContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            int floorNumber = i;
            buttonText.text = $"Floor {i}";
            button.onClick.AddListener(() => buildingManager.SetCurrentFloor(floorNumber));
        }
    }
}
