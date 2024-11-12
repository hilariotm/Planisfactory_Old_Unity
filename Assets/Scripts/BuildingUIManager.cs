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
        categorizedBuildings = new Dictionary<BuildingCategory, List<BuildingData>>();
        
        foreach (BuildingData building in allBuildings)
        {
            if (!categorizedBuildings.ContainsKey(building.category))
            {
                categorizedBuildings[building.category] = new List<BuildingData>();
            }
            categorizedBuildings[building.category].Add(building);
        }
    }

    private void CreateCategoryButtons()
    {
        foreach (BuildingCategory category in categorizedBuildings.Keys)
        {
            GameObject buttonObj = Instantiate(categoryButtonPrefab, categoriesContainer);
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
