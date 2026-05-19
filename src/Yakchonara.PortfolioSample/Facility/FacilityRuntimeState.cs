namespace Yakchonara.PortfolioSample.Facility;

public sealed class FacilityRuntimeState
{
    public event Action? Changed;

    public bool IsOpen { get; private set; } = true;
    public FacilityStatus Status { get; private set; } = FacilityStatus.Idle;

    public void SetOpen(bool isOpen)
    {
        IsOpen = isOpen;
        Status = isOpen ? FacilityStatus.Idle : FacilityStatus.Closed;
        Changed?.Invoke();
    }

    public void SetStatus(FacilityStatus status)
    {
        if (!IsOpen && status != FacilityStatus.Closed)
        {
            throw new InvalidOperationException("A closed facility cannot move to an active status.");
        }

        Status = status;
        Changed?.Invoke();
    }
}
