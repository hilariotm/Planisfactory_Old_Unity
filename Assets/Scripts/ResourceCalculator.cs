using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCalculator : MonoBehaviour
{
    public (Dictionary<ResourceData, int> resources, float powerGeneration, float powerConsumption) CalculateLayoutTotals(Dictionary<int, List<GameObject>> buildingsByFloor)
    {
        Dictionary<ResourceData, int> totals = new Dictionary<ResourceData, int>();
        float totalPowerGeneration = 0f;
        float totalPowerConsumption = 0f;
        
        foreach (var floorBuildings in buildingsByFloor.Values)
        {
            foreach (var building in floorBuildings)
            {
                BuildingData data = building.GetComponent<BuildingReference>().buildingData;
                
                // Add power values
                totalPowerGeneration += data.powerGeneration;
                totalPowerConsumption += data.powerConsumption;
                
                // Add resources
                foreach (var requirement in data.requirements)
                {
                    AddResourceAndIngredients(requirement.resource, requirement.amount, totals);
                }
            }
        }
        
        return (totals, totalPowerGeneration, totalPowerConsumption);
    }

    private void AddResourceAndIngredients(ResourceData resource, int amount, Dictionary<ResourceData, int> totals)
    {
        // Add the resource itself
        if (!totals.ContainsKey(resource))
        {
            totals[resource] = 0;
        }
        totals[resource] += amount;

        // If not a raw material, add its ingredients recursively
        if (!resource.isRawMaterial && resource.recipe != null)
        {
            foreach (var ingredient in resource.recipe)
            {
                float ratio = resource.GetIngredientRatio(ingredient.resource);
                int neededAmount = Mathf.CeilToInt(amount * ratio);
                AddResourceAndIngredients(ingredient.resource, neededAmount, totals);
            }
        }
    }
}
