namespace Yakchonara.PortfolioSample.Facility;

public sealed record FacilityStateSnapshot(
    int FacilityId,
    string FacilityName,
    FacilityKind Kind,
    FacilityStatus Status,
    int Occupancy,
    int Capacity,
    int WaitingCount,
    int WaitingCapacity,
    int AssignedElementals,
    double? Temperature,
    double? Gauge,
    string StatusText);
