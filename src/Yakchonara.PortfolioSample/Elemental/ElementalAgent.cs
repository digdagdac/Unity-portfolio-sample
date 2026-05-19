namespace Yakchonara.PortfolioSample.Elemental;

public sealed class ElementalAgent
{
    public ElementalAgent(string elementalId, ElementalType type, double defaultScale = 1)
    {
        ElementalId = elementalId;
        Type = type;
        DefaultScale = defaultScale;
        CurrentScale = defaultScale;
    }

    public string ElementalId { get; }
    public ElementalType Type { get; }
    public ElementalStatus Status { get; private set; } = ElementalStatus.Idle;
    public int? AssignedFacilityId { get; private set; }
    public string? AssignedFacilityName { get; private set; }
    public double DefaultScale { get; }
    public double CurrentScale { get; private set; }

    public void Capture()
    {
        AssignedFacilityId = null;
        AssignedFacilityName = null;
        Status = ElementalStatus.Captured;
    }

    public void DockToFacility(int facilityId, string facilityName)
    {
        AssignedFacilityId = facilityId;
        AssignedFacilityName = facilityName;
        Status = ElementalStatus.Docked;
        CurrentScale = Math.Round(DefaultScale * 0.75, 2);
    }

    public void ReleaseToWorld()
    {
        AssignedFacilityId = null;
        AssignedFacilityName = null;
        Status = ElementalStatus.Idle;
        CurrentScale = DefaultScale;
    }
}
