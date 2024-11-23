using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private GridPathCreator gridManager;
    [SerializeField] private List<BuildingData> buildings;
    [SerializeField] private Transform buildingsContainer;
    
    private BuildingData selectedBuilding;
    private GameObject previewBuilding;
    private bool canPlace = false;
    private SpriteRenderer previewRenderer;
    private List<GameObject> placedBuildings = new List<GameObject>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && selectedBuilding != null)
        {
            CancelBuildingPlacement();
        }

        if (selectedBuilding != null && previewBuilding != null)
        {
            Vector3 mousePos = GetMouseWorldPosition();
            Vector3 snappedPos = SnapBuildingToGrid(mousePos, selectedBuilding.size);
            previewBuilding.transform.position = new Vector3(snappedPos.x, -90f, snappedPos.z);
        
            canPlace = CheckPlacementValidity(new Vector2(mousePos.x, mousePos.z), selectedBuilding.size);
            UpdatePreviewColor(canPlace);

            if (Input.GetMouseButtonDown(0) && canPlace && !IsPointerOverUI())
            {
                PlaceBuilding(mousePos);
            }
        }
    }

    public void SelectBuilding(BuildingData building)
    {
        selectedBuilding = building;
        CreatePreview();
    }

    private void CreatePreview()
    {
        if (previewBuilding != null) 
            Destroy(previewBuilding);
            
        previewBuilding = Instantiate(selectedBuilding.prefab, buildingsContainer);
        previewBuilding.transform.rotation = Quaternion.Euler(-90, 0, 0);
        Vector2 mousePos = GetMouseWorldPosition();
        previewBuilding.transform.position = new Vector3(mousePos.x, -90f, mousePos.y);
        previewRenderer = previewBuilding.GetComponent<SpriteRenderer>();
        previewRenderer.color = new Color(1f, 1f, 1f, 0.5f);
    }

    private Vector3 SnapBuildingToGrid(Vector3 worldPosition, Vector2Int size)
    {
        Vector3 gridOffset = gridManager.GetGridOffset();
    
        float x = Mathf.Round((worldPosition.x - gridOffset.x) / gridManager.cellSize) * gridManager.cellSize + gridOffset.x;
        float z = Mathf.Round((worldPosition.z - gridOffset.z) / gridManager.cellSize) * gridManager.cellSize + gridOffset.z;

        return new Vector3(x, -90f, z);
    }

    private bool CheckPlacementValidity(Vector2 position, Vector2Int size)
    {
        Vector3 gridOffset = gridManager.GetGridOffset();
        float halfWidth = (size.x * gridManager.cellSize) / 2f;
        float halfHeight = (size.y * gridManager.cellSize) / 2f;
    
        // Check grid boundaries
        float minX = gridOffset.x;
        float maxX = -gridOffset.x;
        float minZ = gridOffset.z;
        float maxZ = -gridOffset.z;

        // First check: Grid boundaries
        if (position.x - halfWidth < minX || position.x + halfWidth > maxX ||
            position.y - halfHeight < minZ || position.y + halfHeight > maxZ)
        {
            return false;
        }

        // Second check: Building overlaps
        Vector3 buildingPosition = new Vector3(position.x, -90f, position.y);
        Vector3 buildingSize = new Vector3(size.x * gridManager.cellSize, 1f, size.y * gridManager.cellSize);
    
        foreach (GameObject existingBuilding in placedBuildings)
        {
            SpriteRenderer existingRenderer = existingBuilding.GetComponent<SpriteRenderer>();
            if (existingRenderer != null)
            {
                Bounds existingBounds = existingRenderer.bounds;
                Bounds newBuildingBounds = new Bounds(buildingPosition, buildingSize);
            
                if (existingBounds.Intersects(newBuildingBounds))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void PlaceBuilding(Vector3 mousePos)
    {
        Vector3 snappedPos = SnapBuildingToGrid(mousePos, selectedBuilding.size);
        Vector3 spawnPosition = new Vector3(snappedPos.x, -90f, snappedPos.z);
        Quaternion rotation = Quaternion.Euler(-90, 0, 0);
        GameObject newBuilding = Instantiate(selectedBuilding.prefab, spawnPosition, rotation);
        newBuilding.transform.SetParent(buildingsContainer);
        placedBuildings.Add(newBuilding);
    }

    private void CancelBuildingPlacement()
    {
        if (previewBuilding != null)
        {
            Destroy(previewBuilding);
        }
        selectedBuilding = null;
    }

    private void UpdatePreviewColor(bool isValid)
    {
        previewRenderer.color = isValid ? 
            new Color(0f, 1f, 0f, 0.5f) : 
            new Color(1f, 0f, 0f, 0.5f);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.y;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}