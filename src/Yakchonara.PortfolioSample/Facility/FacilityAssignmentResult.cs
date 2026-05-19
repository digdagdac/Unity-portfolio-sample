namespace Yakchonara.PortfolioSample.Facility;

public sealed record FacilityAssignmentResult(
    bool Success,
    string Message,
    FacilityStateSnapshot Snapshot);
