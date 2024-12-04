using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;


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
    [SerializeField] private GameObject hoverIconPrefab;

    [Header("Building Interaction")]
    [SerializeField] private GameObject actionMenuPrefab;
    [SerializeField] private Canvas mainCanvas; // Reference to your existing canvas

    private GameObject ghostBuilding;
    private bool isCopyingBuilding;

    private GameObject selectedForEdit;
    public bool isMovingBuilding;

    private GameObject currentActionMenu;

    public void ShowBuildingMenu(GameObject building)
    {
        if (currentActionMenu != null)
        {
            Destroy(currentActionMenu);
        }
    
        currentActionMenu = Instantiate(actionMenuPrefab, mainCanvas.transform);
        currentActionMenu.GetComponent<BuildingActionMenu>().Initialize(building, this);
    }


    // Add these variables at the top with other private fields
    private float currentRotation = 0f;
    private Quaternion baseRotation = Quaternion.Euler(-90f, 0f, 0f);

    // Dictionary to store buildings by floor
    private Dictionary<int, List<GameObject>> buildingsByFloor = new Dictionary<int, List<GameObject>>();

    public GameObject GetHoverIconPrefab() => hoverIconPrefab;
    public bool IsPlacingBuilding() => selectedBuilding != null;


    private void HandleBuildingEdit()
    {
        if (isMovingBuilding && selectedForEdit != null)
        {
            Vector3 mousePos = GetMouseWorldPosition();
            Vector3 snappedPos = SnapBuildingToGrid(mousePos, selectedForEdit.GetComponent<BuildingReference>().buildingData.size);
            selectedForEdit.transform.position = new Vector3(snappedPos.x, -90f, snappedPos.z);

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                FinishMovingBuilding();
            }
        }
    }


    public void DeleteBuilding(GameObject building)
    {
        buildingsByFloor[currentFloor].Remove(building);
        placedBuildings.Remove(building);
        Destroy(building);
        UpdateResourceDisplay();
    }

    public void RotateBuilding(GameObject building)
    {
        Quaternion currentRotation = building.transform.rotation;
        building.transform.rotation = currentRotation * Quaternion.Euler(0f, 0f, 90f);
    }



    public void StartMovingBuilding(GameObject building)
    {
        selectedForEdit = building;
        isMovingBuilding = true;
    }

    public void FinishMovingBuilding()
    {
        isMovingBuilding = false;
        selectedForEdit = null;
        UpdateResourceDisplay();
    }


    public void CopyBuilding(GameObject building)
    {
        BuildingData buildingData = building.GetComponent<BuildingReference>().buildingData;
        selectedBuilding = buildingData;
        currentRotation = building.transform.rotation.eulerAngles.z;
        CreatePreview();
    }


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
        HandleBuildingEdit();

        // Modified menu closing check
        if ((Input.GetMouseButtonDown(0) && !IsPointerOverUI()) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentActionMenu != null)
            {
                Destroy(currentActionMenu);
                currentActionMenu = null;
            }
        }

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
        // Destroy any existing hover icons in the scene
        GameObject[] existingHoverIcons = GameObject.FindGameObjectsWithTag("HoverIcon");
        foreach (GameObject icon in existingHoverIcons)
        {
            Destroy(icon);
        }

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
        // Destroy any hover icons before placing the building
        GameObject[] existingHoverIcons = GameObject.FindGameObjectsWithTag("HoverIcon");
        foreach (GameObject icon in existingHoverIcons)
        {
            Destroy(icon);
        }
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