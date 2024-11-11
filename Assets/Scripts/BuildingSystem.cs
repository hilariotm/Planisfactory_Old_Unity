using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Buildings/Building Data")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public GameObject prefab;
    public Vector2Int size = Vector2Int.one;
    public Sprite icon;
    public BuildingCategory category;
}

public enum BuildingCategory
{
    Production,
    Storage,
    Housing,
    Military,
    Infrastructure
}
