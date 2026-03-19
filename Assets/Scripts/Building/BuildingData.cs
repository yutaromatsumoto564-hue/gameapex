using UnityEngine;
using System.Collections.Generic;

namespace ARIA.Building
{
    [CreateAssetMenu(fileName = "NewBuildingData", menuName = "ARIA/Building Data")]
    public class BuildingData : ScriptableObject
    {
        [Header("Basic Info")]
        public string BuildingId;
        public string BuildingName;
        public string Description;
        public Sprite Icon;
        
        [Header("Size")]
        public int SizeX = 2;
        public int SizeY = 2;
        
        [Header("Stats")]
        public int MaxHealth = 500;
        public int PowerConsumption = 0;
        public int PowerGeneration = 0;
        public int NetworkRange = 5;
        
        [Header("Production")]
        public BuildingCategory Category;
        public float ProductionTime = 3f;
        public List<ResourceIO> Inputs = new List<ResourceIO>();
        public List<ResourceIO> Outputs = new List<ResourceIO>();
        
        [Header("Defense")]
        public int Damage = 0;
        public float AttackRange = 0f;
        public float AttackSpeed = 1f;
        public bool CanTargetAir = false;
        
        [Header("Storage")]
        public int StorageCapacity = 0;
        
        [Header("Tech Requirements")]
        public List<string> RequiredTechIds = new List<string>();
        
        [Header("Upgrade")]
        public string NextUpgradeId = "";
        public int UpgradeCost = 0;
        public List<ResourceIO> UpgradeResources = new List<ResourceIO>();
    }

    [System.Serializable]
    public class ResourceIO
    {
        public string ResourceId;
        public int Amount;
    }

    public enum BuildingCategory
    {
        Resource,
        Production,
        Power,
        Storage,
        Defense,
        Special
    }
}
