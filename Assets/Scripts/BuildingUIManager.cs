using UnityEngine;
using System.Collections.Generic;

public class BuildingUIManager : MonoBehaviour
{
    [SerializeField] private GameObject categoryButtonPrefab;
    [SerializeField] private GameObject buildingButtonPrefab;
    [SerializeField] private Transform categoriesContainer;
    [SerializeField] private Transform buildingsContainer;
    
    private Dictionary<BuildingCategory, List<BuildingData>> categorizedBuildings;
    
    public void CreateCategoryMenu()
    {
        foreach(BuildingCategory category in System.Enum.GetValues(typeof(BuildingCategory)))
        {
            // Create category buttons
            // When clicked, show buildings in that category
        }
    }
}
