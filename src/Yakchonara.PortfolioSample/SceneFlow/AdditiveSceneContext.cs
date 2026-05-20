using System.Collections.Generic;
using Yakchonara.PortfolioSample.Facility;

namespace Yakchonara.PortfolioSample.SceneFlow;

public sealed class AdditiveSceneContext
{
    private readonly List<FacilityBase> _facilities = new();
    private bool _initialized;

    public AdditiveSceneContext(string sceneName)
    {
        SceneName = sceneName;
    }

    public string SceneName { get; }
    public IReadOnlyCollection<FacilityBase> Facilities => _facilities.AsReadOnly();

    public void RegisterFacility(FacilityBase facility)
    {
        _facilities.Add(facility);
    }

    public void Initialize()
    {
        _initialized = true;
        foreach (FacilityBase facility in _facilities)
        {
            facility.Open();
        }
    }

    public void Cleanup()
    {
        foreach (FacilityBase facility in _facilities)
        {
            facility.Close();
        }

        _initialized = false;
    }

    public string Describe()
    {
        string state = _initialized ? "initialized" : "not-initialized";
        return $"{SceneName}: {state}, facilities={_facilities.Count}";
    }
}
