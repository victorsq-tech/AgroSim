namespace AgroSim.UI
{
    public enum FarmScreen { MainMenu, FarmMap, Garage, Workshop, Fuel, Field, Operation }
    public sealed class FarmNavigation
    {
        public FarmScreen Current { get; private set; } = FarmScreen.MainMenu;
        public bool Open(FarmScreen target)
        {
            if(Current==FarmScreen.Operation || target==FarmScreen.Operation) return false;
            bool allowed=target==Current ||
                (Current==FarmScreen.MainMenu && target==FarmScreen.FarmMap) ||
                (Current==FarmScreen.FarmMap && target!=FarmScreen.Operation) ||
                (Current!=FarmScreen.MainMenu && target==FarmScreen.FarmMap) ||
                (Current==FarmScreen.Field && target==FarmScreen.Garage);
            if(allowed) Current=target;
            return allowed;
        }
        public bool BeginOperation()
        { if(Current!=FarmScreen.Garage) return false; Current=FarmScreen.Operation; return true; }
        public void ReturnToGarage() { Current=FarmScreen.Garage; }
    }
}
