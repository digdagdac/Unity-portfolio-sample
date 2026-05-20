using System;
using System.Collections.Generic;
using System.Linq;
using Yakchonara.PortfolioSample.Elemental;

namespace Yakchonara.PortfolioSample.Facility;

public abstract class FacilityBase
{
    private readonly List<ElementalAgent> _assignedElementals = new();

    protected FacilityBase(int id, string name, FacilityKind kind, int capacity, int lineupCapacity)
    {
        FacilityId = id;
        FacilityName = name;
        Kind = kind;
        RuntimeState = new FacilityRuntimeState();
        Occupancy = new FacilityOccupancy(capacity);
        Lineup = new FacilityLineup(lineupCapacity);
        RuntimeState.Changed += PublishStateChanged;
    }

    public event Action<FacilityStateSnapshot>? StateChanged;

    public int FacilityId { get; }
    public string FacilityName { get; }
    public FacilityKind Kind { get; }
    public FacilityRuntimeState RuntimeState { get; }
    public FacilityOccupancy Occupancy { get; }
    public FacilityLineup Lineup { get; }
    public IReadOnlyCollection<ElementalAgent> AssignedElementals => _assignedElementals.AsReadOnly();

    public FacilityAssignmentResult EnqueueHerb(HerbRequest herb)
    {
        if (!RuntimeState.IsOpen)
        {
            return Result(false, $"{FacilityName} is closed.");
        }

        if (!Lineup.TryEnqueue(herb))
        {
            return Result(false, $"{FacilityName} lineup is full.");
        }

        RuntimeState.SetStatus(FacilityStatus.Waiting);
        return Result(true, $"{herb.HerbId} queued for {FacilityName}.");
    }

    public FacilityAssignmentResult TryAssignNextHerb()
    {
        if (!Lineup.TryDequeue(out HerbRequest? herb) || herb is null)
        {
            return Result(false, "No herb is waiting.");
        }

        return TryAssignHerb(herb);
    }

    public FacilityAssignmentResult TryAssignHerb(HerbRequest herb)
    {
        if (!RuntimeState.IsOpen)
        {
            return Result(false, $"{FacilityName} is closed.");
        }

        if (!Occupancy.TryAssign(herb))
        {
            return Result(false, $"{FacilityName} has no empty slot for {herb.HerbId}.");
        }

        OnHerbAssigned(herb);
        RefreshStatus();
        return Result(true, $"{herb.HerbId} assigned to {FacilityName}.");
    }

    public FacilityAssignmentResult ReleaseHerb(string herbId)
    {
        if (!Occupancy.Release(herbId))
        {
            return Result(false, $"{herbId} was not assigned to {FacilityName}.");
        }

        OnHerbReleased(herbId);
        RefreshStatus();
        return Result(true, $"{herbId} released from {FacilityName}.");
    }

    public bool TryAssignElemental(ElementalAgent elemental)
    {
        if (_assignedElementals.Any(current => current.ElementalId == elemental.ElementalId))
        {
            return false;
        }

        _assignedElementals.Add(elemental);
        elemental.DockToFacility(FacilityId, FacilityName);
        OnElementalAssigned(elemental);
        PublishStateChanged();
        return true;
    }

    public bool ReleaseElemental(string elementalId)
    {
        ElementalAgent? elemental = _assignedElementals.FirstOrDefault(current => current.ElementalId == elementalId);
        if (elemental is null)
        {
            return false;
        }

        _assignedElementals.Remove(elemental);
        elemental.ReleaseToWorld();
        PublishStateChanged();
        return true;
    }

    public void Close()
    {
        Occupancy.Clear();
        Lineup.Clear();
        _assignedElementals.ForEach(elemental => elemental.ReleaseToWorld());
        _assignedElementals.Clear();
        RuntimeState.SetOpen(false);
    }

    public void Open() => RuntimeState.SetOpen(true);

    public virtual FacilityStateSnapshot GetSnapshot()
    {
        return new FacilityStateSnapshot(
            FacilityId,
            FacilityName,
            Kind,
            RuntimeState.Status,
            Occupancy.Count,
            Occupancy.Capacity,
            Lineup.Count,
            Lineup.Capacity,
            _assignedElementals.Count,
            Temperature: null,
            Gauge: null,
            BuildStatusText());
    }

    protected virtual void OnHerbAssigned(HerbRequest herb)
    {
    }

    protected virtual void OnHerbReleased(string herbId)
    {
    }

    protected virtual void OnElementalAssigned(ElementalAgent elemental)
    {
    }

    protected virtual string BuildStatusText()
    {
        return RuntimeState.Status switch
        {
            FacilityStatus.Closed => "closed",
            FacilityStatus.Full => "full",
            FacilityStatus.Waiting => "waiting",
            FacilityStatus.Occupied => "occupied",
            _ => "idle"
        };
    }

    protected void PublishStateChanged() => StateChanged?.Invoke(GetSnapshot());

    private FacilityAssignmentResult Result(bool success, string message)
    {
        return new FacilityAssignmentResult(success, message, GetSnapshot());
    }

    private void RefreshStatus()
    {
        if (!RuntimeState.IsOpen)
        {
            RuntimeState.SetStatus(FacilityStatus.Closed);
        }
        else if (Occupancy.IsFull)
        {
            RuntimeState.SetStatus(FacilityStatus.Full);
        }
        else if (Occupancy.Count > 0)
        {
            RuntimeState.SetStatus(FacilityStatus.Occupied);
        }
        else if (Lineup.Count > 0)
        {
            RuntimeState.SetStatus(FacilityStatus.Waiting);
        }
        else
        {
            RuntimeState.SetStatus(FacilityStatus.Idle);
        }
    }
}
