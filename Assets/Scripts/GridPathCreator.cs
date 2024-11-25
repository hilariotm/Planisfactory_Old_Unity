using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GridPathCreator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cellSize = 1f;

    [Header("Grid Visualization")]
    public Color cellLineColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    public Color sectionLineColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
    public float cellLineWidth = 0.02f;
    public float sectionLineWidth = 0.05f;
    
    [Header("Path Settings")]
    public Color pathColor = Color.white;
    public Color previewColor = new Color(1f, 1f, 1f, 0.5f);
    public float lineWidth = 0.1f;
    [Range(0, 1f)]
    public float cornerSmoothness = 0.5f;
    public int cornerDetail = 8;
    
    [Header("Texture Settings")]
    public Texture2D pathTexture;
    public float textureScale = 1f;
    public bool stretchTexture = false;

    [Header("Camera Controls")]
    public float zoomSpeed = 0.15f;
    public float minZoom = 2f;
    public float maxZoom = 40f;
    public float panSpeed = 10f;
    public float zoomSmoothness = 10f;
    public float panSmoothness = 10f;

    [Header("UI References")]
    [SerializeField] private Button startPathButton;
    [SerializeField] private Button stopPathButton;
    [SerializeField] private BuildingManager buildingManager;  // Add reference to BuildingManager
    [SerializeField] private bool isPathCreationMode;
    [SerializeField] private int currentFloor = 0;
    private Dictionary<int, List<LineRenderer>> pathsByFloor = new Dictionary<int, List<LineRenderer>>();
    
    private List<Vector2> currentPathPoints;
    private List<LineRenderer> pathLines;
    private LineRenderer currentLine;
    private LineRenderer previewLine;
    private Material lineMaterial;
    private GameObject pathsContainer;
    private Vector3 targetPanPosition;
    private Vector3 lastMousePosition;
    private float targetZoom;

    private Stack<Vector2> undoStack = new Stack<Vector2>();
    private Stack<Vector2> redoStack = new Stack<Vector2>();

    // Check if pointer is over UI elements
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void Start()
    {
        InitializeComponents();
        SetupCamera();
        UpdateButtonVisibility();
        CreateGrid();
        targetZoom = Camera.main.orthographicSize;
        targetPanPosition = Camera.main.transform.position;
        currentFloor = buildingManager.GetCurrentFloor();
    }

    public Vector3 GetGridOffset()
    {
        float offsetX = -(gridWidth * cellSize) / 2f;
        float offsetZ = -(gridHeight * cellSize) / 2f;
        return new Vector3(offsetX, 0, offsetZ);
    }

    private void CreateGrid()
    {
        GameObject gridContainer = new GameObject("GridContainer");
        gridContainer.transform.parent = transform;

        float startX = -(gridWidth * cellSize) / 2f;
        float startZ = -(gridHeight * cellSize) / 2f;

        for (int x = 0; x <= gridWidth; x++)
        {
            float xPos = startX + (x * cellSize);
            CreateGridLine(gridContainer, 
                new Vector3(xPos, 0, startZ), 
                new Vector3(xPos, 0, -startZ), 
                x % 8 == 0 ? sectionLineColor : cellLineColor, 
                x % 8 == 0 ? sectionLineWidth : cellLineWidth);
        }

        for (int z = 0; z <= gridHeight; z++)
        {
            float zPos = startZ + (z * cellSize);
            CreateGridLine(gridContainer, 
                new Vector3(startX, 0, zPos), 
                new Vector3(-startX, 0, zPos), 
                z % 8 == 0 ? sectionLineColor : cellLineColor, 
                z % 8 == 0 ? sectionLineWidth : cellLineWidth);
        }
    }

    private void CreateGridLine(GameObject container, Vector3 start, Vector3 end, Color color, float width)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.parent = container.transform;
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
    
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = color;
        line.startWidth = line.endWidth = width;
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.allowOcclusionWhenDynamic = false;
        line.generateLightingData = false;
        line.sortingOrder = -1;  // This will place the grid behind UI elements
    
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    private void UpdateButtonVisibility()
    {
        startPathButton.gameObject.SetActive(!isPathCreationMode);
        stopPathButton.gameObject.SetActive(isPathCreationMode);
    }

    private void InitializeComponents()
    {
        currentPathPoints = new List<Vector2>();
        pathLines = new List<LineRenderer>();
        
        pathsContainer = new GameObject("PathsContainer");
        pathsContainer.transform.parent = transform;
        
        CreateLineMaterial();
        CreatePreviewLine();
    }

    private void CreateLineMaterial()
    {
        lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = Color.white;
        
        if (pathTexture != null)
        {
            lineMaterial.mainTexture = pathTexture;
            lineMaterial.mainTextureScale = new Vector2(1f, 1f);
            lineMaterial.mainTextureOffset = new Vector2(0f, 0f);
        }
    }

    private void CreatePreviewLine()
    {
        GameObject previewObject = new GameObject("PathPreview");
        previewObject.transform.parent = transform;
        previewLine = previewObject.AddComponent<LineRenderer>();
        SetupLineRenderer(previewLine);
        previewLine.startColor = previewColor;
        previewLine.endColor = previewColor;
        previewLine.positionCount = 2;
        previewLine.enabled = false;
    }

    private void SetupCamera()
    {
        if (Camera.main != null)
        {
            Camera.main.orthographic = true;
            Camera.main.transform.position = new Vector3(0, 10, 0);
            Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);
            Camera.main.orthographicSize = Mathf.Max(gridWidth, gridHeight) * cellSize / 2f;
        }
    }

    private void UpdateCameraControls()
    {
        // Smooth Zoom with non-linear scaling
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            float currentZoom = targetZoom;
            float zoomDelta = scroll * zoomSpeed;
        
            // Apply larger steps when zoomed out, smaller steps when zoomed in
            if (currentZoom < 10f)
            {
                zoomDelta *= 0.3f; // Finer control in close-up view
            }
            else if (currentZoom < 20f)
            {
                zoomDelta *= 0.6f; // Medium steps in middle range
            }
            // Default step size for zoomed out views
        
            targetZoom = Mathf.Clamp(targetZoom - zoomDelta, minZoom, maxZoom);
        }

        Camera.main.orthographicSize = Mathf.Lerp(
            Camera.main.orthographicSize,
            targetZoom,
            Time.deltaTime * zoomSmoothness
        );

        // Smooth Pan
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }
    
        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x, 0, -delta.y) * panSpeed * Time.deltaTime;
        
            targetPanPosition += move;
            lastMousePosition = Input.mousePosition;
        }

        // Smooth camera position interpolation
        Camera.main.transform.position = Vector3.Lerp(
            Camera.main.transform.position,
            new Vector3(targetPanPosition.x, Camera.main.transform.position.y, targetPanPosition.z),
            Time.deltaTime * panSmoothness
        );
    }

    public void ResetCameraPosition()
    {
        targetPanPosition = new Vector3(0f, Camera.main.transform.position.y, 0f);
        targetZoom = Mathf.Max(gridWidth, gridHeight) * cellSize / 2f; // Reset to initial zoom level
    }

    private void SetupLineRenderer(LineRenderer line)
    {
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = lineMaterial;
        // Set sorting order for conveyors
        int floorBase = currentFloor * 1000;
        line.sortingOrder = floorBase + 100;  // Conveyors: above foundations, below other buildings
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.allowOcclusionWhenDynamic = false;
        line.generateLightingData = false;
        line.useWorldSpace = true;
        line.alignment = LineAlignment.View;
        line.numCapVertices = 4;
        line.numCornerVertices = cornerDetail;
        line.textureMode = LineTextureMode.Tile;
    }

    private void Update()
    {
        UpdateCameraControls();
        
        if (isPathCreationMode)
        {
            HandlePathCreation();
            HandleUndoRedo();
        }
        else if (previewLine.enabled)
        {
            previewLine.enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isPathCreationMode)
        {
            CancelCurrentPath();
        }
    }

    private void CancelCurrentPath()
    {
        if (currentPathPoints.Count > 0)
        {
            currentPathPoints = new List<Vector2>();
            previewLine.enabled = false;
        }
    }

    private void HandlePathCreation()
    {
        // Skip path creation if pointer is over UI
        if (IsPointerOverUI()) return;

        Vector3 mousePos = GetMouseWorldPosition();
        Vector2 snappedPos = SnapToGrid(mousePos);

        // Show preview line if we have at least one point
        if (currentPathPoints.Count > 0)
        {
            previewLine.enabled = true;
            Vector2 lastPoint = currentPathPoints[currentPathPoints.Count - 1];
            previewLine.SetPosition(0, new Vector3(lastPoint.x, 0, lastPoint.y));
            previewLine.SetPosition(1, new Vector3(snappedPos.x, 0, snappedPos.y));
            // Set preview sorting order based on current floor
            int floorBase = currentFloor * 1000;
            previewLine.sortingOrder = floorBase + 100;
        }
        else
        {
            previewLine.enabled = false;
        }

        // Handle mouse click for adding points
        if (Input.GetMouseButtonDown(0))
        {
            if (currentPathPoints.Count == 0)
            {
                CreateNewPath();
            }
            
            if (currentPathPoints.Count == 0 || currentPathPoints[currentPathPoints.Count - 1] != snappedPos)
            {
                currentPathPoints.Add(snappedPos);
                redoStack.Clear();
                UpdateCurrentPath();
            }
        }
    }

    private void HandleUndoRedo()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z) && currentPathPoints.Count > 0)
        {
            Vector2 removedPoint = currentPathPoints[currentPathPoints.Count - 1];
            currentPathPoints.RemoveAt(currentPathPoints.Count - 1);
            undoStack.Push(removedPoint);
            UpdateCurrentPath();
        }
        
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y) && undoStack.Count > 0)
        {
            Vector2 redoPoint = undoStack.Pop();
            currentPathPoints.Add(redoPoint);
            UpdateCurrentPath();
        }
    }

    private void CreateNewPath()
    {
        currentPathPoints = new List<Vector2>();
        GameObject newPathObject = new GameObject($"Path_Floor{currentFloor}_{pathLines.Count}");
        newPathObject.transform.parent = pathsContainer.transform;
        currentLine = newPathObject.AddComponent<LineRenderer>();
        SetupLineRenderer(currentLine);
        currentLine.startColor = pathColor;
        currentLine.endColor = pathColor;
    
        Debug.Log($"Creating new path on floor {currentFloor}");
    
        if (!pathsByFloor.ContainsKey(currentFloor))
        {
            pathsByFloor[currentFloor] = new List<LineRenderer>();
            Debug.Log($"Created new floor list for floor {currentFloor}");
        }
    
        pathsByFloor[currentFloor].Add(currentLine);
        pathLines.Add(currentLine);
    
        Debug.Log($"Total paths on floor {currentFloor}: {pathsByFloor[currentFloor].Count}");
    }

    // Add method to change floors
    public void SetCurrentFloor(int floor)
    {
        currentFloor = floor;
        UpdatePathVisibility();
    }

    private void UpdatePathVisibility()
    {
        foreach (var floorPaths in pathsByFloor)
        {
            bool isCurrentFloor = floorPaths.Key == currentFloor;
            bool isFloorBelow = floorPaths.Key == currentFloor - 1;
        
            foreach (var path in floorPaths.Value)
            {
                if (path != null)
                {
                    if (isCurrentFloor)
                    {
                        path.enabled = true;
                        SetPathOpacity(path, 1f);
                    }
                    else if (isFloorBelow)
                    {
                        path.enabled = true;
                        SetPathOpacity(path, 0.3f);
                    }
                    else
                    {
                        path.enabled = false;
                    }
                }
            }
        }
    }

    private void SetPathOpacity(LineRenderer path, float opacity)
    {
        Color color = pathColor;
        color.a = opacity;
        path.startColor = color;
        path.endColor = color;
    }

    public void UpdateCurrentFloor(int floor)
    {
        currentFloor = floor;
        UpdatePathVisibility();
    }

    public void StartPathCreation()
    {
        isPathCreationMode = true;
        currentPathPoints = new List<Vector2>();
        previewLine.enabled = false;
        undoStack.Clear();
        redoStack.Clear();
        UpdateButtonVisibility();
    }

    public void StopPathCreation()
    {
        if (currentPathPoints.Count > 0)
        {
            currentPathPoints = new List<Vector2>();
        }
        isPathCreationMode = false;
        previewLine.enabled = false;
        UpdateButtonVisibility();
    }

    private void UpdateCurrentPath()
    {
        if (currentLine != null && currentPathPoints.Count > 0)
        {
            Vector3[] points = new Vector3[currentPathPoints.Count];
            for (int i = 0; i < currentPathPoints.Count; i++)
            {
                points[i] = new Vector3(currentPathPoints[i].x, 0, currentPathPoints[i].y);
            }
            
            currentLine.positionCount = points.Length;
            currentLine.SetPositions(points);
            
            UpdateTextureMapping(points);
        }
    }

    private void UpdateTextureMapping(Vector3[] points)
    {
        if (points.Length < 2) return;

        float totalLength = 0f;
        for (int i = 0; i < points.Length - 1; i++)
        {
            totalLength += Vector3.Distance(points[i], points[i + 1]);
        }

        if (stretchTexture)
        {
            currentLine.material.mainTextureScale = new Vector2(totalLength, 1f);
        }
        else
        {
            currentLine.material.mainTextureScale = new Vector2(totalLength * textureScale, 1f);
        }
    }

    // Modify ClearPath()
    public void ClearPath()
    {
        if (pathsByFloor.ContainsKey(currentFloor))
        {
            foreach (var path in pathsByFloor[currentFloor])
            {
                if (path != null)
                {
                    Destroy(path.gameObject);
                }
            }
            pathsByFloor[currentFloor].Clear();
        }
    
        pathLines.Clear();
        currentPathPoints.Clear();
        currentLine = null;
        undoStack.Clear();
        redoStack.Clear();
    }

    public void SetPathTexture(Texture2D newTexture)
    {
        pathTexture = newTexture;
        if (lineMaterial != null)
        {
            lineMaterial.mainTexture = newTexture;
        }
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        float distance;
        
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        
        return Vector3.zero;
    }

    private Vector2 SnapToGrid(Vector3 worldPosition)
    {
        float halfGridWidth = (gridWidth * cellSize) / 2f;
        float halfGridHeight = (gridHeight * cellSize) / 2f;
        
        float x = Mathf.Round(worldPosition.x / cellSize) * cellSize;
        float z = Mathf.Round(worldPosition.z / cellSize) * cellSize;
        
        x = Mathf.Clamp(x, -halfGridWidth + cellSize/2f, halfGridWidth - cellSize/2f);
        z = Mathf.Clamp(z, -halfGridHeight + cellSize/2f, halfGridHeight - cellSize/2f);
        
        return new Vector2(x, z);
    }
}
