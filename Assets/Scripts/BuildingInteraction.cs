using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;



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
        if (currentHoverIcon == null && !buildingManager.IsPlacingBuilding() && !buildingManager.isMovingBuilding)
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
        if (currentHoverIcon != null && !buildingManager.isMovingBuilding && !IsClickingOnActionMenu())
        {
            buildingManager.ShowBuildingMenu(gameObject);
        }
    }

    private bool IsClickingOnActionMenu()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponentInParent<BuildingActionMenu>() != null)
            {
                return true;
            }
        }
        return false;
    }

}
