using System;
using UnityEngine;

namespace ARIA.Power
{
    [Serializable]
    public class PowerNetworkData
    {
        public int TotalGeneration;
        public int TotalConsumption;
        public int PowerSurplus;
        public int GeneratorCount;
        public int ConsumerCount;

        public float GetEfficiency()
        {
            if (TotalConsumption <= 0) return 1f;
            return Mathf.Clamp01((float)TotalGeneration / TotalConsumption);
        }

        public bool IsBalanced()
        {
            return PowerSurplus >= 0;
        }
    }

    public static class PowerConstants
    {
        public const int STEAM_GENERATOR_OUTPUT = 500;
        public const int SOLAR_PANEL_OUTPUT = 200;
        public const int NUCLEAR_PLANT_OUTPUT = 5000;

        public const int MINER_CONSUMPTION = 100;
        public const int FURNACE_CONSUMPTION = 50;
        public const int FACTORY_CONSUMPTION = 150;
        public const int LAB_CONSUMPTION = 200;
        public const int TURRET_CONSUMPTION = 100;
    }
}
