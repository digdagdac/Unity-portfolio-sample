namespace Yakchonara.PortfolioSample.Facility;

public sealed class FacilityOccupancy
{
    private readonly Dictionary<string, HerbRequest> _assignedHerbs = new();

    public FacilityOccupancy(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Facility capacity must be positive.");
        }

        Capacity = capacity;
    }

    public int Capacity { get; }
    public int Count => _assignedHerbs.Count;
    public bool IsFull => Count >= Capacity;

    public IReadOnlyCollection<HerbRequest> AssignedHerbs => _assignedHerbs.Values.ToArray();

    public bool TryAssign(HerbRequest herb)
    {
        if (IsFull || _assignedHerbs.ContainsKey(herb.HerbId))
        {
            return false;
        }

        _assignedHerbs.Add(herb.HerbId, herb);
        return true;
    }

    public bool Release(string herbId) => _assignedHerbs.Remove(herbId);

    public void Clear() => _assignedHerbs.Clear();
}
