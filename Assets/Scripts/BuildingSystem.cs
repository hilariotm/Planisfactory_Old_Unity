using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Buildings/Building Data")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public GameObject prefab;
    public Vector2Int size = Vector2Int.one;
    public Sprite icon;
    public BuildingCategory category;
    public BuildingSubCategory subCategory;
    public float powerConsumption;
    public float powerGeneration;
    public float GetNetPower()
    {
        return powerGeneration - powerConsumption;
    }
}

[System.Serializable]
public class CategoryInfo
{
    public BuildingCategory category;
    public Sprite icon;
}

public enum BuildingCategory
{
    Special,
    Production,
    Power,
    Logistics,
    Organization,
    Transport,
    Foundations,
    Walls,
    Architecture
}

public enum BuildingSubCategory
{
    // Special subcategories
    Progression,
    AWESOMEBonusProgram,
    FICSITBlueprintDesigners,
    Workstations,
    
    // Production subcategories
    Manufacturers,
    Smelters,
    Miners,
    FluidExtractors,
    
    // Power subcategories
    Generators,
    PowerDistribution,
    WallOutlets,
    
    // Logistics subcategories
    ConveyorBelts,
    ConveyorLifts,
    ConveyorCupports,
    Pipelines,
    PipelineSupports,
    Sorting,
    
    // Organization subcategories
    Lights,
    Signs,
    Storage,
    Towers,

    // Transport subcategories
    VehicleTransport,
    RailwayTransport,
    Hypertubes,
    JumpPads,

    // Foundations subcategories
    Foundations,
    Ramps,
    InvertedRamps,
    QuarterPipes,

    // Walls subcategories
    BasicWalls,
    ConveyorConnections,
    RampWalls,
    InvertedRampWalls,
    TiltedWalls,
    WindowWalls,
    FramedWindows,
    Doors,

    // Architecture subcategories
    Roofs,
    Frames,
    Beams,
    Pillars,
    Attachments,
    Catwalks,
    Walkways
}