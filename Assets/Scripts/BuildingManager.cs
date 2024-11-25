using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private GridPathCreator gridManager;
    [SerializeField] private List<BuildingData> buildings;
    [SerializeField] private Transform buildingsContainer;
    [SerializeField] private int currentFloor = 0;
    [SerializeField] private int maxFloors = 3;
    [SerializeField] private GridPathCreator gridPathCreator;
    [SerializeField] private ResourceTotalsUI resourceTotalsUI;
    [SerializeField] private ResourceCalculator resourceCalculator;
    [SerializeField] private PowerSummaryUI powerSummaryUI;
    private Stack<(GameObject building, int floor)> buildingHistory = new Stack<(GameObject building, int floor)>();

    private void HandleUndo()
    {
        if (buildingHistory.Count > 0)
        {
            var (lastBuilding, floor) = buildingHistory.Pop();
            buildingsByFloor[floor].Remove(lastBuilding);
            placedBuildings.Remove(lastBuilding);
            Destroy(lastBuilding);
            UpdateResourceDisplay();
        }
    }


    // Add these variables at the top with other private fields
    private float currentRotation = 0f;
    private Quaternion baseRotation = Quaternion.Euler(-90f, 0f, 0f);

    // Dictionary to store buildings by floor
    private Dictionary<int, List<GameObject>> buildingsByFloor = new Dictionary<int, List<GameObject>>();

    private void UpdateResourceDisplay()
    {
        var (resources, powerGen, powerConsumption) = resourceCalculator.CalculateLayoutTotals(buildingsByFloor);
        resourceTotalsUI.DisplayResourceTotals(resources);
        powerSummaryUI.UpdatePowerDisplay(powerGen, powerConsumption);
    }

    private void Start()
    {
        // Initialize floors
        for (int i = 0; i < maxFloors; i++)
        {
            buildingsByFloor[i] = new List<GameObject>();
        }
    }

    public void SetCurrentFloor(int floor)
    {
        if (floor >= 0 && floor < maxFloors)
        {
            currentFloor = floor;
            UpdateBuildingVisibility();
            gridPathCreator.UpdateCurrentFloor(floor);
        }
    }

    public int GetCurrentFloor()
    {
        return currentFloor;
    }

    private void UpdateBuildingVisibility()
    {
        foreach (var floorKvp in buildingsByFloor)
        {
            foreach (var building in floorKvp.Value)
            {
                SpriteRenderer renderer = building.GetComponent<SpriteRenderer>();
                if (floorKvp.Key == currentFloor)
                {
                    building.SetActive(true);
                    renderer.color = new Color(1f, 1f, 1f, 1f);
                }
                else if (floorKvp.Key == currentFloor - 1)
                {
                    building.SetActive(true);
                    renderer.color = new Color(1f, 1f, 1f, 0.3f);
                }
                else
                {
                    building.SetActive(false);
                }
            }
        }
    }
    
    private BuildingData selectedBuilding;
    private GameObject previewBuilding;
    private bool canPlace = false;
    private SpriteRenderer previewRenderer;
    private List<GameObject> placedBuildings = new List<GameObject>();

    private void Update()
    {
        //Remove undo function
        /*
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            HandleUndo();
        }
        */
        
        if (Input.GetKeyDown(KeyCode.Escape) && selectedBuilding != null)
        {
            CancelBuildingPlacement();
        }

        if (selectedBuilding != null && previewBuilding != null)
        {
            // Handle rotation
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentRotation += 90f;
                if (currentRotation >= 360f) currentRotation = 0f;
                previewBuilding.transform.rotation = baseRotation * Quaternion.Euler(0f, 0f, currentRotation);
            }

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
        currentRotation = 0f;
        CreatePreview();
        gridPathCreator.StopPathCreation();
    }

    private int GetSortingOrder(GameObject building)
    {
        int floorBase = currentFloor * 1000;
    
        if (building.CompareTag("Foundation"))
        {
            return floorBase + 0;
        }
        else if (building.CompareTag("Conveyor"))
        {
            return floorBase + 100;
        }
        else
        {
            return floorBase + 200 + placedBuildings.Count;
        }
    }

    private void CreatePreview()
    {
        if (previewBuilding != null) 
            Destroy(previewBuilding);
        
        previewBuilding = Instantiate(selectedBuilding.prefab, buildingsContainer);
        previewBuilding.transform.rotation = baseRotation;
        Vector2 mousePos = GetMouseWorldPosition();
        previewBuilding.transform.position = new Vector3(mousePos.x, -90f, mousePos.y);
        previewRenderer = previewBuilding.GetComponent<SpriteRenderer>();
        previewRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        previewRenderer.sortingOrder = GetSortingOrder(previewBuilding);
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

        // Grid boundaries check
        float minX = gridOffset.x;
        float maxX = -gridOffset.x;
        float minZ = gridOffset.z;
        float maxZ = -gridOffset.z;

        if (position.x - halfWidth < minX || position.x + halfWidth > maxX ||
            position.y - halfHeight < minZ || position.y + halfHeight > maxZ)
        {
            return false;
        }

        // If placing a foundation, skip collision checks
        if (selectedBuilding.prefab.CompareTag("Foundation"))
        {
            return true;
        }

        // Create bounds for the new building using sprite size
        SpriteRenderer previewSprite = previewBuilding.GetComponent<SpriteRenderer>();
        Vector3 buildingPosition = new Vector3(position.x, -90f, position.y);
        Bounds newBuildingBounds = previewSprite.sprite.bounds;
        newBuildingBounds.center = buildingPosition;
        newBuildingBounds.size = new Vector3(
            previewSprite.sprite.bounds.size.x * previewBuilding.transform.localScale.x,
            1f,
            previewSprite.sprite.bounds.size.y * previewBuilding.transform.localScale.y
        );

        // Check against existing buildings
        if (buildingsByFloor.ContainsKey(currentFloor))
        {
            foreach (GameObject existingBuilding in buildingsByFloor[currentFloor])
            {
                if (existingBuilding.CompareTag("Foundation"))
                    continue;

                SpriteRenderer existingRenderer = existingBuilding.GetComponent<SpriteRenderer>();
                if (existingRenderer != null)
                {
                    Bounds existingBounds = existingRenderer.bounds;
                    if (existingBounds.Intersects(newBuildingBounds))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }



    private void PlaceBuilding(Vector3 mousePos)
    {
        Vector3 snappedPos = SnapBuildingToGrid(mousePos, selectedBuilding.size);
        Vector3 spawnPosition = new Vector3(snappedPos.x, -90f, snappedPos.z);
    
        // Apply both base rotation and current rotation
        Quaternion finalRotation = baseRotation * Quaternion.Euler(0f, 0f, currentRotation);
    
        GameObject newBuilding = Instantiate(selectedBuilding.prefab, spawnPosition, finalRotation);
        newBuilding.transform.SetParent(buildingsContainer);
        placedBuildings.Add(newBuilding);

        SpriteRenderer renderer = newBuilding.GetComponent<SpriteRenderer>();
        renderer.sortingOrder = GetSortingOrder(newBuilding);

        if (!buildingsByFloor.ContainsKey(currentFloor))
        {
            buildingsByFloor[currentFloor] = new List<GameObject>();
        }
        buildingsByFloor[currentFloor].Add(newBuilding);
        buildingHistory.Push((newBuilding, currentFloor));
        UpdateResourceDisplay();
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

    private void SetupBuildingRendering(GameObject building)
    {
        SpriteRenderer renderer = building.GetComponent<SpriteRenderer>();
    
        // Set the sorting layer based on type
        if (building.CompareTag("Foundation"))
        {
            renderer.sortingLayerName = "Buildings";
        }
        else if (building.CompareTag("Conveyor"))
        {
            renderer.sortingLayerName = "Buildings";
        }
        else
        {
            renderer.sortingLayerName = "Buildings";
        }
    
        // Keep your existing sorting order logic
        renderer.sortingOrder = GetSortingOrder(building);
    }

}