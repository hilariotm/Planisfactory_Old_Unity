using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;


public class BuildingActionMenu : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform menuContainer;
    [SerializeField] private float menuOffset = 1f;
    
    private BuildingManager buildingManager;
    private GameObject targetBuilding;

    private void CreateMenuButton(string actionName, UnityAction action)
    {
        GameObject buttonObj = Instantiate(buttonPrefab, menuContainer);
        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        
        buttonText.text = actionName;
        button.onClick.AddListener(action);
        button.onClick.AddListener(() => Destroy(gameObject));
    }

    public void Initialize(GameObject building, BuildingManager manager)
    {
        targetBuilding = building;
        buildingManager = manager;

        CreateMenuButton("Delete", () => buildingManager.DeleteBuilding(targetBuilding));
        CreateMenuButton("Rotate", () => buildingManager.RotateBuilding(targetBuilding));
        CreateMenuButton("Move", () => buildingManager.StartMovingBuilding(targetBuilding));
        CreateMenuButton("Copy", () => buildingManager.CopyBuilding(targetBuilding));

        PositionMenu();
    }

    private void PositionMenu()
    {
        Vector3 buildingPosition = targetBuilding.transform.position;
        transform.position = buildingPosition + Vector3.right * menuOffset;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}