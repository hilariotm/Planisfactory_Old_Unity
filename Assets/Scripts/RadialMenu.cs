using UnityEngine;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    [SerializeField] private GameObject quadrantButtonPrefab;
    [SerializeField] private float radius = 100f;
    [SerializeField] private Transform menuContainer;

    private GameObject targetBuilding;
    private BuildingManager buildingManager;

    public void Initialize(GameObject building, BuildingManager manager)
    {
        targetBuilding = building;
        buildingManager = manager;
        CreateRadialMenu(4);
    }

    private void CreateRadialMenu(int buttonCount)
    {
        float[] angles = { 0f, 90f, 180f, 270f };
        string[] actions = { "Delete", "Rotate", "Move", "Copy" };

        for (int i = 0; i < buttonCount; i++)
        {
            GameObject quadrant = Instantiate(quadrantButtonPrefab, menuContainer);
            RectTransform rectTransform = quadrant.GetComponent<RectTransform>();
        
            // Position and rotate the button
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localRotation = Quaternion.Euler(0, 0, angles[i]);
            rectTransform.anchoredPosition = GetPositionOnCircle(angles[i]);

            // Set up the button action
            Button button = quadrant.GetComponent<Button>();
            string action = actions[i];
            button.onClick.AddListener(() => HandleQuadrantClick(action));
        }
    }

    private Vector2 GetPositionOnCircle(float angleDegrees)
    {
        float radius = 0f; // Reduce this value to bring buttons closer together
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float x = radius * Mathf.Cos(angleRadians);
        float y = radius * Mathf.Sin(angleRadians);
        return new Vector2(x, y);
    }


    private void HandleQuadrantClick(string action)
    {
        switch (action)
        {
            case "Delete":
                buildingManager.PublicDeleteBuilding(targetBuilding);
                break;
            case "Rotate":
                buildingManager.PublicRotateBuilding(targetBuilding);
                break;
            case "Move":
                buildingManager.PublicStartMovingBuilding(targetBuilding);
                break;
            case "Copy":
                buildingManager.PublicCopyBuilding(targetBuilding);
                break;
        }
        Destroy(gameObject);
    }
}
