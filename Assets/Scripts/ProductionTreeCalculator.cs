using UnityEngine;
using System.Collections.Generic;

public class ProductionTreeCalculator : MonoBehaviour
{
    public class ResourceNode
    {
        public ResourceData resource;
        public int totalAmount;
        public List<ResourceNode> ingredients = new List<ResourceNode>();
    }
    
    // Calculate for a single building
    public ResourceNode CalculateBuildingRequirements(BuildingData building)
    {
        ResourceNode rootNode = new ResourceNode();
        foreach (var requirement in building.requirements)
        {
            AddResourceBranch(requirement.resource, requirement.amount, rootNode);
        }
        return rootNode;
    }
    
    // Calculate for entire layout
    public ResourceNode CalculateLayoutRequirements(Dictionary<int, List<GameObject>> buildingsByFloor)
    {
        ResourceNode totalRequirements = new ResourceNode();
        
        foreach (var floorBuildings in buildingsByFloor.Values)
        {
            foreach (var building in floorBuildings)
            {
                BuildingData data = building.GetComponent<BuildingReference>().buildingData;
                MergeRequirements(totalRequirements, CalculateBuildingRequirements(data));
            }
        }
        
        return totalRequirements;
    }
    
    private void AddResourceBranch(ResourceData resource, int amount, ResourceNode parentNode)
    {
        ResourceNode node = new ResourceNode { resource = resource, totalAmount = amount };
        parentNode.ingredients.Add(node);
        
        if (!resource.isRawMaterial && resource.recipe != null)
        {
            foreach (var ingredient in resource.recipe)
            {
                float ratio = resource.GetIngredientRatio(ingredient.resource);
                int neededAmount = Mathf.CeilToInt(amount * ratio);
                AddResourceBranch(ingredient.resource, neededAmount, node);
            }
        }
    }
    
    private void MergeRequirements(ResourceNode target, ResourceNode source)
    {
        foreach (var ingredient in source.ingredients)
        {
            var existing = target.ingredients.Find(x => x.resource == ingredient.resource);
            if (existing != null)
            {
                existing.totalAmount += ingredient.totalAmount;
            }
            else
            {
                target.ingredients.Add(ingredient);
            }
        }
    }
}
