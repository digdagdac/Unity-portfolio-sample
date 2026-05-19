using Yakchonara.PortfolioSample.Facility;

namespace Yakchonara.PortfolioSample.UI;

public sealed class FacilityStatusBinder
{
    private FacilityBase? _facility;

    public FacilityViewModel? CurrentView { get; private set; }

    public void Bind(FacilityBase facility)
    {
        Unbind();
        _facility = facility;
        _facility.StateChanged += ApplySnapshot;
        ApplySnapshot(_facility.GetSnapshot());
    }

    public void Unbind()
    {
        if (_facility is not null)
        {
            _facility.StateChanged -= ApplySnapshot;
            _facility = null;
        }
    }

    private void ApplySnapshot(FacilityStateSnapshot snapshot)
    {
        CurrentView = new FacilityViewModel(
            snapshot.FacilityName,
            snapshot.StatusText,
            $"{snapshot.Occupancy}/{snapshot.Capacity}",
            $"{snapshot.WaitingCount}/{snapshot.WaitingCapacity}",
            snapshot.AssignedElementals.ToString(),
            snapshot.Temperature?.ToString("0.0") ?? "-",
            snapshot.Gauge?.ToString("0.00") ?? "-");
    }
}
