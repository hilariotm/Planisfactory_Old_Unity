using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingUIManager : MonoBehaviour
{
    [System.Serializable]
    public class CategoryInfo
    {
        public BuildingCategory category;
        public Sprite icon;
    }

    [SerializeField] private List<CategoryInfo> categoryIcons;
    [SerializeField] private GameObject categoryButtonPrefab;
    [SerializeField] private GameObject buildingButtonPrefab;
    [SerializeField] private Transform categoriesContainer;
    [SerializeField] private Transform buildingsContainer;
    [SerializeField] private List<BuildingData> allBuildings;
    [SerializeField] private BuildingManager buildingManager;

    private Dictionary<BuildingCategory, List<BuildingData>> categorizedBuildings;

    private void Start()
    {
        InitializeDictionary();
        CreateCategoryButtons();
    }

    private void InitializeDictionary()
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
            TMPro.TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            Image buttonImage = buttonObj.GetComponent<Image>();
            
            buttonText.text = category.ToString();
            
            // Set the category icon
            CategoryInfo categoryInfo = categoryIcons.Find(info => info.category == category);
            if (categoryInfo != null && categoryInfo.icon != null)
            {
                buttonImage.sprite = categoryInfo.icon;
            }
            
            button.onClick.AddListener(() => ShowBuildingsForCategory(category));
        }
    }

    private void ShowBuildingsForCategory(BuildingCategory category)
    {
        foreach (Transform child in buildingsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (BuildingData building in categorizedBuildings[category])
        {
            GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingsContainer);
            Button button = buttonObj.GetComponent<Button>();
            TMPro.TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            buttonText.text = building.buildingName;
        
            if (building.icon != null)
            {
                buttonObj.GetComponent<Image>().sprite = building.icon;
            }
        
            button.onClick.AddListener(() => buildingManager.SelectBuilding(building));
        }
    }

    private void ClearBuildingButtons()
    {
        foreach (Transform child in buildingsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateBuildingButton(BuildingData building)
    {
        GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingsContainer);
        
        // Set building icon
        Image buttonImage = buttonObj.GetComponent<Image>();
        if (building.icon != null)
        {
            buttonImage.sprite = building.icon;
        }
        
        // Set building name
        Text buttonText = buttonObj.GetComponentInChildren<Text>();
        buttonText.text = building.buildingName;
        
        // Add click handler
        Button button = buttonObj.GetComponent<Button>();
        button.onClick.AddListener(() => buildingManager.SelectBuilding(building));
    }
}
