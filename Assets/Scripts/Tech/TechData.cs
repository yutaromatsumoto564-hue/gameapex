using UnityEngine;
using System.Collections.Generic;
using ARIA.Core;

namespace ARIA.Tech
{
    [CreateAssetMenu(fileName = "NewTechData", menuName = "ARIA/Tech Data")]
    public class TechData : ScriptableObject
    {
        [Header("Basic Info")]
        public string TechId;
        public string TechName;
        public string Description;
        public Sprite Icon;

        [Header("Category")]
        public TechCategory Category;
        public int Tier;

        [Header("Requirements")]
        public List<string> PrerequisiteTechIds = new List<string>();
        public List<TechCost> ResearchCosts = new List<TechCost>();
        public float ResearchTime = 60f;

        [Header("Unlocks")]
        public List<string> UnlockedBuildingIds = new List<string>();
        public List<string> UnlockedCardIds = new List<string>();
        public List<TechBonus> Bonuses = new List<TechBonus>();
    }

    [System.Serializable]
    public class TechCost
    {
        public string ResourceId;
        public int Amount;
    }

    [System.Serializable]
    public class TechBonus
    {
        public TechBonusType Type;
        public float Value;
    }

    public enum TechCategory
    {
        Physics,
        Engineering,
        Society
    }

    public enum TechBonusType
    {
        ProductionSpeed,
        PowerEfficiency,
        BuildingHealth,
        TurretDamage,
        TurretRange,
        StorageCapacity,
        NetworkRange
    }
}
