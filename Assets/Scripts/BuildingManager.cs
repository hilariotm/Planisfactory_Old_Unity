using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private List<BuildingData> buildings;
    [SerializeField] private Transform buildingsContainer;
    [SerializeField] private float cellSize = 1f;
    
    private BuildingData selectedBuilding;
    private GameObject previewBuilding;
    private bool canPlace = false;
    private Material previewMaterial;

    private void Start()
    {
        InitializePreviewMaterial();
    }

    private void InitializePreviewMaterial()
    {
        previewMaterial = new Material(Shader.Find("Standard"));
        previewMaterial.color = new Color(1f, 1f, 1f, 0.5f);
    }

    private void Update()
    {
        if (selectedBuilding != null && previewBuilding != null)
        {
            Vector3 mousePos = GetMouseWorldPosition();
            Vector3 snappedPos = SnapBuildingToGrid(mousePos, selectedBuilding.size);
            previewBuilding.transform.position = snappedPos;
            
            canPlace = CheckPlacementValidity(snappedPos, selectedBuilding.size);
            UpdatePreviewColor(canPlace);

            if (Input.GetMouseButtonDown(0) && canPlace && !IsPointerOverUI())
            {
                PlaceBuilding(snappedPos);
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
        SetPreviewMaterial(previewBuilding);
    }

    private void SetPreviewMaterial(GameObject preview)
    {
        Renderer[] renderers = preview.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = previewMaterial;
            }
            renderer.materials = materials;
        }
    }

    private Vector3 SnapBuildingToGrid(Vector3 worldPosition, Vector2Int size)
    {
        float xOffset = (size.x * cellSize) / 2f;
        float zOffset = (size.y * cellSize) / 2f;

        float x = Mathf.Round(worldPosition.x / cellSize) * cellSize;
        float z = Mathf.Round(worldPosition.z / cellSize) * cellSize;

        return new Vector3(x, 0, z);
    }

    private bool CheckPlacementValidity(Vector3 position, Vector2Int size)
    {
        // Add your validation logic here
        // Check grid boundaries
        // Check for overlapping buildings
        // Check for valid terrain
        return true;
    }

    private void PlaceBuilding(Vector3 position)
    {
        GameObject newBuilding = Instantiate(selectedBuilding.prefab, position, Quaternion.identity, buildingsContainer);
        // Add any additional building setup here
    }

    private void UpdatePreviewColor(bool isValid)
    {
        previewMaterial.color = isValid ? 
            new Color(0f, 1f, 0f, 0.5f) : // Green for valid
            new Color(1f, 0f, 0f, 0.5f);  // Red for invalid
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }
        
        return Vector3.zero;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
