using Yakchonara.PortfolioSample.Elemental;

namespace Yakchonara.PortfolioSample.Facility;

public sealed class BathTemperatureController
{
    public BathTemperatureController(double currentTemperature, double targetTemperature, double tolerance)
    {
        CurrentTemperature = currentTemperature;
        TargetTemperature = targetTemperature;
        Tolerance = tolerance;
    }

    public double CurrentTemperature { get; private set; }
    public double TargetTemperature { get; }
    public double Tolerance { get; }
    public bool IsComfortable => Math.Abs(CurrentTemperature - TargetTemperature) <= Tolerance;
    public double ComfortGauge => Math.Max(0, 1 - Math.Abs(CurrentTemperature - TargetTemperature) / TargetTemperature);

    public void ApplyElemental(ElementalType elementType)
    {
        CurrentTemperature += elementType switch
        {
            ElementalType.Fire => 4,
            ElementalType.Water => -3,
            ElementalType.Wind => -1,
            ElementalType.Earth => 1,
            _ => 0
        };
    }

    public void CoolDown(double amount)
    {
        CurrentTemperature = Math.Max(0, CurrentTemperature - amount);
    }
}
