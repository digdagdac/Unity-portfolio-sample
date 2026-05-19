using Yakchonara.PortfolioSample.Facility;

namespace Yakchonara.PortfolioSample.Elemental;

public sealed class ElementalActionController
{
    public bool Capture(ElementalAgent elemental)
    {
        if (elemental.Status == ElementalStatus.Docked)
        {
            return false;
        }

        elemental.Capture();
        return true;
    }

    public bool AssignToFacility(ElementalAgent elemental, FacilityBase facility)
    {
        if (elemental.Status != ElementalStatus.Captured)
        {
            return false;
        }

        return facility.TryAssignElemental(elemental);
    }

    public void Release(ElementalAgent elemental)
    {
        elemental.ReleaseToWorld();
    }
}
