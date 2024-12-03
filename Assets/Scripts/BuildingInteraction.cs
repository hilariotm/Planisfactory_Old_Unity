using UnityEngine;

public class BuildingInteraction : MonoBehaviour
{
    private BuildingManager buildingManager;
    private GameObject currentHoverIcon;
    private Canvas hoverCanvas;

    private void Start()
    {
        buildingManager = FindObjectOfType<BuildingManager>();
        Debug.Log($"BuildingInteraction started on {gameObject.name}");
    }

    private void OnMouseOver()
    {
        if (currentHoverIcon == null && !buildingManager.IsPlacingBuilding())
        {
            currentHoverIcon = Instantiate(buildingManager.GetHoverIconPrefab());
            Canvas canvas = currentHoverIcon.GetComponentInChildren<Canvas>();
            canvas.worldCamera = Camera.main;
        
            currentHoverIcon.transform.position = transform.position + Vector3.up * 2f;
            currentHoverIcon.transform.localScale = Vector3.one * 0.01f;
            currentHoverIcon.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        }
    }


    private void OnMouseExit()
    {
        if (currentHoverIcon != null)
        {
            Destroy(currentHoverIcon);
            currentHoverIcon = null;
            Debug.Log("Hover icon destroyed");
        }
    }


    private void OnMouseDown()
    {
        if (currentHoverIcon != null)
        {
            buildingManager.ShowEditMenu(gameObject);
        }
    }
}
