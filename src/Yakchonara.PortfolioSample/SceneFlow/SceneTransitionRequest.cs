using Yakchonara.PortfolioSample.Common;

namespace Yakchonara.PortfolioSample.SceneFlow;

public sealed record SceneTransitionRequest(
    string SceneName,
    GridPosition EntryPosition,
    string Reason);
