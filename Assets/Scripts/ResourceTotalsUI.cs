using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;


public class ResourceTotalsUI : MonoBehaviour
{
    [SerializeField] private GameObject resourceEntryPrefab;
    [SerializeField] private Transform contentContainer;
    
    private Dictionary<ResourceData, int> resourceTotals = new Dictionary<ResourceData, int>();
    
    public void DisplayResourceTotals(Dictionary<ResourceData, int> totals)
    {
        ClearDisplay();
        resourceTotals = totals;
    
        // Order by isRawMaterial first (raw materials come first), then by name
        var orderedResources = resourceTotals
            .OrderBy(x => !x.Key.isRawMaterial)  // '!' makes raw materials come first
            .ThenBy(x => x.Key.resourceName);
        
        foreach (var kvp in orderedResources)
        {
            GameObject entry = Instantiate(resourceEntryPrefab, contentContainer);
        
            Image icon = entry.GetComponentInChildren<Image>();
            TextMeshProUGUI text = entry.GetComponentInChildren<TextMeshProUGUI>();
        
            icon.sprite = kvp.Key.icon;
            text.text = $"{kvp.Key.resourceName}: {kvp.Value}";
        }
    }

    
    private void ClearDisplay()
    {
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }
    }
}