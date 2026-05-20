using System;
using System.Collections.Generic;

namespace Yakchonara.PortfolioSample.Facility;

public sealed class FacilityLineup
{
    private readonly Queue<HerbRequest> _queue = new();

    public FacilityLineup(int capacity)
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Lineup capacity cannot be negative.");
        }

        Capacity = capacity;
    }

    public int Capacity { get; }
    public int Count => _queue.Count;

    public IReadOnlyCollection<HerbRequest> WaitingHerbs => _queue.ToArray();

    public bool TryEnqueue(HerbRequest herb)
    {
        if (_queue.Count >= Capacity)
        {
            return false;
        }

        _queue.Enqueue(herb);
        return true;
    }

    public bool TryDequeue(out HerbRequest? herb)
    {
        if (_queue.Count == 0)
        {
            herb = null;
            return false;
        }

        herb = _queue.Dequeue();
        return true;
    }

    public void Clear() => _queue.Clear();
}
