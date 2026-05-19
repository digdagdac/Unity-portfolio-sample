using Yakchonara.PortfolioSample.Elemental;

namespace Yakchonara.PortfolioSample.Facility;

public sealed class BathFacility : FacilityBase
{
    public BathFacility(int id, string name, int capacity, int lineupCapacity, BathTemperatureController temperature)
        : base(id, name, FacilityKind.Bath, capacity, lineupCapacity)
    {
        Temperature = temperature;
        Sessions = new BathSessionController();
    }

    public BathTemperatureController Temperature { get; }
    public BathSessionController Sessions { get; }

    public bool CompleteBath(string herbId)
    {
        if (!Sessions.CompleteSession(herbId, out _))
        {
            return false;
        }

        ReleaseHerb(herbId);
        return true;
    }

    public override FacilityStateSnapshot GetSnapshot()
    {
        FacilityStateSnapshot snapshot = base.GetSnapshot();
        return snapshot with
        {
            Temperature = Math.Round(Temperature.CurrentTemperature, 1),
            Gauge = Math.Round(Temperature.ComfortGauge, 2),
            StatusText = BuildStatusText()
        };
    }

    protected override void OnHerbAssigned(HerbRequest herb)
    {
        Sessions.BeginSession(herb, Temperature.CurrentTemperature);
    }

    protected override void OnHerbReleased(string herbId)
    {
        Sessions.CompleteSession(herbId, out _);
    }

    protected override void OnElementalAssigned(ElementalAgent elemental)
    {
        Temperature.ApplyElemental(elemental.Type);
    }

    protected override string BuildStatusText()
    {
        if (!RuntimeState.IsOpen)
        {
            return "closed";
        }

        string comfort = Temperature.IsComfortable ? "comfortable" : "temperature-adjusting";
        return $"{comfort}, sessions={Sessions.ActiveSessions.Count}";
    }
}
