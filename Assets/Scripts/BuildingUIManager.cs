using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingUIManager : MonoBehaviour
{
    [SerializeField] private GameObject categoryButtonPrefab;
    [SerializeField] private GameObject buildingButtonPrefab;
    [SerializeField] private Transform categoriesContainer;
    [SerializeField] private Transform buildingsContainer;
    [SerializeField] private List<BuildingData> allBuildings;
    [SerializeField] private BuildingManager buildingManager;

    private Dictionary<BuildingCategory, List<BuildingData>> categorizedBuildings;

    private void Start()
    {
        CategorizeBuildings();
        CreateCategoryButtons();
    }

    private void CategorizeBuildings()
    {
        Debug.Log("Starting categorization");
        categorizedBuildings = new Dictionary<BuildingCategory, List<BuildingData>>();
    
        Debug.Log($"Number of buildings: {allBuildings.Count}");
        foreach (BuildingData building in allBuildings)
        {
            Debug.Log($"Processing building: {building.buildingName}, Category: {building.category}");
            if (!categorizedBuildings.ContainsKey(building.category))
            {
                categorizedBuildings[building.category] = new List<BuildingData>();
            }
            categorizedBuildings[building.category].Add(building);
        }
        Debug.Log($"Categories created: {categorizedBuildings.Count}");
    }

    private void CreateCategoryButtons()
    {
        Debug.Log($"Creating buttons for {categorizedBuildings.Keys.Count} categories");
        Debug.Log($"Category Button Prefab: {categoryButtonPrefab}");
        Debug.Log($"Categories Container: {categoriesContainer}");

        foreach (BuildingCategory category in categorizedBuildings.Keys)
        {
            Debug.Log($"Creating button for category: {category}");
            GameObject buttonObj = Instantiate(categoryButtonPrefab, categoriesContainer);
            Debug.Log($"Button object created: {buttonObj}, Has Button: {buttonObj.GetComponent<Button>()}, Has Text: {buttonObj.GetComponentInChildren<Text>()}");
            Button button = buttonObj.GetComponent<Button>();
            button.GetComponentInChildren<Text>().text = category.ToString();
            
            button.onClick.AddListener(() => ShowBuildingsInCategory(category));
        }
    }

    private void ShowBuildingsInCategory(BuildingCategory category)
    {
        // Clear existing building buttons
        foreach (Transform child in buildingsContainer)
        {
            Destroy(child.gameObject);
        }

        // Create buttons for buildings in selected category
        foreach (BuildingData building in categorizedBuildings[category])
        {
            GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingsContainer);
            Button button = buttonObj.GetComponent<Button>();
            
            // Set button icon
            if (building.icon != null)
            {
                buttonObj.GetComponent<Image>().sprite = building.icon;
            }
            
            // Set building name
            buttonObj.GetComponentInChildren<Text>().text = building.buildingName;
            
            // Add click handler
            button.onClick.AddListener(() => buildingManager.SelectBuilding(building));
        }
    }
}
