using System;
using System.Globalization;

namespace AgroSim.Data
{
    public static class GarageEconomy
    {
        public const long InitialBalanceCents=500000;
        public const int FuelPriceCents=0;
        public const int WearPointPriceCents=0;
        private static readonly CultureInfo Currency=CultureInfo.GetCultureInfo("pt-BR");
        public static long RefuelCost(float fuel) => CalculateCost(100f-fuel,FuelPriceCents);
        public static long RepairCost(float wear) => CalculateCost(wear,WearPointPriceCents);
        public static long CalculateCost(float quantity,int cents)
        {
            if(float.IsNaN(quantity) || float.IsInfinity(quantity) || quantity<0 || quantity>100) throw new ArgumentOutOfRangeException(nameof(quantity));
            return (long)Math.Round((decimal)quantity*cents,0,MidpointRounding.AwayFromZero);
        }
        public static string Money(long cents) => (cents/100m).ToString("C2",Currency);
    }
}
