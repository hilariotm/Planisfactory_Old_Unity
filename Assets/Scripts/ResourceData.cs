using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Factory/Resource Data")]
public class ResourceData : ScriptableObject
{
    public string resourceName;
    public Sprite icon;
    [Tooltip("If true, this is a raw material that cannot be crafted")]
    public bool isRawMaterial;
    
    [System.Serializable]
    public class RecipeIngredient
    {
        public ResourceData resource;
        public int amount;
    }
    
    [Header("Crafting Recipe")]
    [Tooltip("How many items are produced by one craft operation")]
    public int outputAmount = 1;
    public List<RecipeIngredient> recipe;

    public float GetIngredientRatio(ResourceData ingredient)
    {
        RecipeIngredient found = recipe.Find(r => r.resource == ingredient);
        if (found != null)
        {
            return (float)found.amount / outputAmount;
        }
        return 0f;
    }
}