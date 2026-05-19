namespace Yakchonara.PortfolioSample.Facility;

public sealed record SaunaSession(string HerbId, string SlotId, bool IsPaused = false, bool IsCompleted = false);
