using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static ProductionTreeCalculator;

public class ProductionChainUI : MonoBehaviour
{
    [SerializeField] private GameObject nodeEntryPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float indentSize = 40f;
    
    private RectTransform viewport;
    private RectTransform content;
    
    private void Awake()
    {
        // Get references
        viewport = transform.Find("Viewport").GetComponent<RectTransform>();
        content = viewport.Find("Content").GetComponent<RectTransform>();
        
        // Setup ScrollRect references
        scrollRect.viewport = viewport;
        scrollRect.content = content;
    }
    
    public void DisplayChain(ResourceNode root)
    {
        ClearDisplay();
        CreateNodes(root, 0);
    }
    
    private void CreateNodes(ResourceNode node, float indent)
    {
        foreach (var ingredient in node.ingredients)
        {
            GameObject entry = Instantiate(nodeEntryPrefab);
            entry.transform.SetParent(content, false);
        
            // Add Layout Element for indentation
            LayoutElement layoutElement = entry.AddComponent<LayoutElement>();
            layoutElement.minWidth = indent;
        
            Image icon = entry.GetComponentInChildren<Image>();
            TextMeshProUGUI text = entry.GetComponentInChildren<TextMeshProUGUI>();
        
            icon.sprite = ingredient.resource.icon;
            text.text = $"{ingredient.resource.resourceName} x{ingredient.totalAmount}";
        
            if (ingredient.ingredients.Count > 0)
            {
                CreateNodes(ingredient, indent + indentSize);
            }
        }
    }

    
    private void ClearDisplay()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }
}
