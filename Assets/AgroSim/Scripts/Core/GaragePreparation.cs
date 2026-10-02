namespace AgroSim.Core
{
    public sealed class GaragePreparation
    {
        public bool TractorSelected { get; private set; }
        public bool ImplementSelected { get; private set; }
        public bool CanStart => TractorSelected && ImplementSelected;

        public void ToggleTractor()
        {
            TractorSelected = !TractorSelected;
            if (!TractorSelected) ImplementSelected = false;
        }

        public void ToggleImplement()
        {
            if (TractorSelected) ImplementSelected = !ImplementSelected;
        }

        public void Reset() { TractorSelected = ImplementSelected = false; }
    }
}
