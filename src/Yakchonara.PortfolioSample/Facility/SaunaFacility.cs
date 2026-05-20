using System;
using System.Collections.Generic;
using System.Linq;
using Yakchonara.PortfolioSample.Elemental;

namespace Yakchonara.PortfolioSample.Facility;

public sealed class SaunaFacility : FacilityBase
{
    private readonly Dictionary<string, SaunaSession> _sessions = new();

    public SaunaFacility(int id, string name, int capacity, int lineupCapacity)
        : base(id, name, FacilityKind.Sauna, capacity, lineupCapacity)
    {
    }

    public double HeatGauge { get; private set; } = 0.25;
    public IReadOnlyCollection<SaunaSession> Sessions => _sessions.Values.ToArray();

    public void TickHeat(double delta)
    {
        HeatGauge = Math.Max(0, Math.Min(1, HeatGauge + delta));
        PublishStateChanged();
    }

    public override FacilityStateSnapshot GetSnapshot()
    {
        FacilityStateSnapshot snapshot = base.GetSnapshot();
        return snapshot with
        {
            Gauge = Math.Round(HeatGauge, 2),
            StatusText = BuildStatusText()
        };
    }

    protected override void OnHerbAssigned(HerbRequest herb)
    {
        string slotId = $"slot-{_sessions.Count + 1}";
        _sessions[herb.HerbId] = new SaunaSession(herb.HerbId, slotId);
    }

    protected override void OnHerbReleased(string herbId)
    {
        _sessions.Remove(herbId);
    }

    protected override void OnElementalAssigned(ElementalAgent elemental)
    {
        double contribution = elemental.Type == ElementalType.Fire ? 0.20 : 0.05;
        TickHeat(contribution);
    }

    protected override string BuildStatusText()
    {
        if (!RuntimeState.IsOpen)
        {
            return "closed";
        }

        return $"heat={HeatGauge:0.00}, sessions={_sessions.Count}";
    }
}
