namespace Yakchonara.PortfolioSample.SceneFlow;

public sealed class SceneFlowCoordinator
{
    private readonly Dictionary<string, AdditiveSceneContext> _contexts = new();

    public event Action<string>? SceneLoadStarted;
    public event Action<string>? SceneLoaded;
    public event Action<string>? SceneUnloadStarted;
    public event Action<string>? SceneUnloaded;

    public string? ActiveScene { get; private set; }
    public bool IsInFacility => ActiveScene is not null;

    public void RegisterContext(AdditiveSceneContext context)
    {
        _contexts[context.SceneName] = context;
    }

    public SceneTransitionResult EnterScene(SceneTransitionRequest request)
    {
        if (ActiveScene is not null)
        {
            return new SceneTransitionResult(false, $"Already in {ActiveScene}. Exit first.", ActiveScene);
        }

        if (!_contexts.TryGetValue(request.SceneName, out AdditiveSceneContext? context))
        {
            return new SceneTransitionResult(false, $"No context registered for {request.SceneName}.", ActiveScene);
        }

        SceneLoadStarted?.Invoke(request.SceneName);
        context.Initialize();
        ActiveScene = request.SceneName;
        SceneLoaded?.Invoke(request.SceneName);
        return new SceneTransitionResult(true, $"Entered {request.SceneName} at {request.EntryPosition}.", ActiveScene);
    }

    public SceneTransitionResult ExitCurrentScene(string reason)
    {
        if (ActiveScene is null)
        {
            return new SceneTransitionResult(false, "No active additive scene.", ActiveScene);
        }

        string sceneName = ActiveScene;
        AdditiveSceneContext context = _contexts[sceneName];
        SceneUnloadStarted?.Invoke(sceneName);
        context.Cleanup();
        ActiveScene = null;
        SceneUnloaded?.Invoke(sceneName);
        return new SceneTransitionResult(true, $"Exited {sceneName}. Reason: {reason}.", ActiveScene);
    }
}
