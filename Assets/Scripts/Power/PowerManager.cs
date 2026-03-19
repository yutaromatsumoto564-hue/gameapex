using UnityEngine;
using System;
using System.Collections.Generic;
using ARIA.Core;

namespace ARIA.Power
{
    public class PowerManager : MonoBehaviour
    {
        public static PowerManager Instance { get; private set; }

        [Header("Power Stats")]
        public int TotalGeneration;
        public int TotalConsumption;
        public int PowerSurplus;

        private Dictionary<int, int> generators = new Dictionary<int, int>();
        private Dictionary<int, int> consumers = new Dictionary<int, int>();

        public event Action<int, int> OnPowerChanged;
        public event Action OnPowerShortage;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddPowerGeneration(int buildingId, int amount)
        {
            if (amount <= 0) return;

            generators[buildingId] = amount;
            RecalculatePower();
        }

        public void RemovePowerGeneration(int buildingId)
        {
            generators.Remove(buildingId);
            RecalculatePower();
        }

        public void AddPowerConsumption(int buildingId, int amount)
        {
            if (amount <= 0) return;

            consumers[buildingId] = amount;
            RecalculatePower();
        }

        public void RemovePowerConsumption(int buildingId)
        {
            consumers.Remove(buildingId);
            RecalculatePower();
        }

        private void RecalculatePower()
        {
            TotalGeneration = 0;
            foreach (var gen in generators.Values)
            {
                TotalGeneration += gen;
            }

            TotalConsumption = 0;
            foreach (var con in consumers.Values)
            {
                TotalConsumption += con;
            }

            PowerSurplus = TotalGeneration - TotalConsumption;

            OnPowerChanged?.Invoke(TotalGeneration, TotalConsumption);

            if (PowerSurplus < 0)
            {
                OnPowerShortage?.Invoke();
                EventManager.Instance?.TriggerEvent(GameEvents.POWER_SHORTAGE, PowerSurplus);
            }

            EventManager.Instance?.TriggerEvent(GameEvents.POWER_CHANGED, TotalGeneration, TotalConsumption);
        }

        public bool HasEnoughPower()
        {
            return PowerSurplus >= 0;
        }

        public float GetPowerPercentage()
        {
            if (TotalConsumption <= 0) return 1f;
            return Mathf.Clamp01((float)TotalGeneration / TotalConsumption);
        }

        public float GetSurplusPercentage()
        {
            if (TotalGeneration <= 0) return 0f;
            return (float)PowerSurplus / TotalGeneration;
        }

        public string GetPowerStatusText()
        {
            if (PowerSurplus >= 0)
            {
                return $"+{PowerSurplus} MW";
            }
            return $"{PowerSurplus} MW";
        }

        public PowerNetworkData GetNetworkData()
        {
            return new PowerNetworkData
            {
                TotalGeneration = TotalGeneration,
                TotalConsumption = TotalConsumption,
                PowerSurplus = PowerSurplus,
                GeneratorCount = generators.Count,
                ConsumerCount = consumers.Count
            };
        }
    }
}
