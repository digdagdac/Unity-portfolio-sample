using System.Collections.Generic;
using System.Linq;

namespace Yakchonara.PortfolioSample.Facility;

public sealed class BathSessionController
{
    private readonly Dictionary<string, BathSession> _sessions = new();

    public IReadOnlyCollection<BathSession> ActiveSessions => _sessions.Values.ToArray();

    public void BeginSession(HerbRequest herb, double startTemperature)
    {
        _sessions[herb.HerbId] = new BathSession(herb.HerbId, herb.DesiredService, startTemperature);
    }

    public bool CompleteSession(string herbId, out BathSession? session)
    {
        if (!_sessions.Remove(herbId, out session))
        {
            return false;
        }

        session = session with { Completed = true };
        return true;
    }

    public void Clear() => _sessions.Clear();
}

public sealed record BathSession(
    string HerbId,
    string DesiredService,
    double StartTemperature,
    bool Completed = false);
