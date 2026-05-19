namespace Yakchonara.PortfolioSample.Common;

public readonly record struct GridPosition(int X, int Y)
{
    public static GridPosition Origin => new(0, 0);

    public override string ToString() => $"({X}, {Y})";
}
