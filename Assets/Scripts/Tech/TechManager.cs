using UnityEngine;
using System.Collections.Generic;
using ARIA.Core;
using ARIA.Resource;

namespace ARIA.Tech
{
    public class TechManager : MonoBehaviour
    {
        public static TechManager Instance { get; private set; }

        [Header("Tech Database")]
        public List<TechData> AllTechs = new List<TechData>();

        [Header("Starting Techs")]
        public List<string> StartingTechIds = new List<string>();

        private Dictionary<string, TechData> techDatabase = new Dictionary<string, TechData>();
        private HashSet<string> unlockedTechs = new HashSet<string>();
        private Dictionary<string, float> researchingTechs = new Dictionary<string, float>();

        public event System.Action<TechData> OnTechUnlocked;
        public event System.Action<TechData> OnResearchStarted;
        public event System.Action<TechData, float> OnResearchProgress;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeDatabase();
        }

        private void Start()
        {
            UnlockStartingTechs();
        }

        private void Update()
        {
            UpdateResearch();
        }

        private void InitializeDatabase()
        {
            techDatabase.Clear();
            foreach (var tech in AllTechs)
            {
                if (!string.IsNullOrEmpty(tech.TechId))
                {
                    techDatabase[tech.TechId] = tech;
                }
            }
        }

        private void UnlockStartingTechs()
        {
            foreach (var techId in StartingTechIds)
            {
                UnlockTech(techId);
            }
        }

        private void UpdateResearch()
        {
            List<string> completedTechs = new List<string>();

            foreach (var kvp in researchingTechs)
            {
                string techId = kvp.Key;
                float progress = kvp.Value;

                TechData tech = GetTechData(techId);
                if (tech == null) continue;

                progress += Time.deltaTime;
                researchingTechs[techId] = progress;

                OnResearchProgress?.Invoke(tech, progress / tech.ResearchTime);

                if (progress >= tech.ResearchTime)
                {
                    completedTechs.Add(techId);
                }
            }

            foreach (var techId in completedTechs)
            {
                CompleteResearch(techId);
            }
        }

        public TechData GetTechData(string techId)
        {
            return techDatabase.TryGetValue(techId, out TechData data) ? data : null;
        }

        public bool IsTechUnlocked(string techId)
        {
            return unlockedTechs.Contains(techId);
        }

        public bool IsTechResearching(string techId)
        {
            return researchingTechs.ContainsKey(techId);
        }

        public float GetResearchProgress(string techId)
        {
            if (!researchingTechs.TryGetValue(techId, out float progress)) return 0f;

            TechData tech = GetTechData(techId);
            if (tech == null) return 0f;

            return progress / tech.ResearchTime;
        }

        public bool CanResearch(string techId)
        {
            TechData tech = GetTechData(techId);
            if (tech == null) return false;
            if (IsTechUnlocked(techId)) return false;
            if (IsTechResearching(techId)) return false;

            foreach (var prereq in tech.PrerequisiteTechIds)
            {
                if (!IsTechUnlocked(prereq))
                {
                    return false;
                }
            }

            foreach (var cost in tech.ResearchCosts)
            {
                if (!ResourceManager.Instance.HasResource(cost.ResourceId, cost.Amount))
                {
                    return false;
                }
            }

            return true;
        }

        public bool StartResearch(string techId)
        {
            if (!CanResearch(techId)) return false;

            TechData tech = GetTechData(techId);

            foreach (var cost in tech.ResearchCosts)
            {
                ResourceManager.Instance.RemoveResource(cost.ResourceId, cost.Amount);
            }

            researchingTechs[techId] = 0f;

            OnResearchStarted?.Invoke(tech);
            EventManager.Instance?.TriggerEvent(GameEvents.TECH_RESEARCH_START, tech);

            return true;
        }

        public void CancelResearch(string techId)
        {
            if (!IsTechResearching(techId)) return;

            TechData tech = GetTechData(techId);
            researchingTechs.Remove(techId);

            foreach (var cost in tech.ResearchCosts)
            {
                ResourceManager.Instance.AddResource(cost.ResourceId, cost.Amount);
            }
        }

        private void CompleteResearch(string techId)
        {
            researchingTechs.Remove(techId);
            UnlockTech(techId);
        }

        public void UnlockTech(string techId)
        {
            if (IsTechUnlocked(techId)) return;

            TechData tech = GetTechData(techId);
            if (tech == null) return;

            unlockedTechs.Add(techId);

            ApplyTechBonuses(tech);
            UnlockTechContent(tech);

            OnTechUnlocked?.Invoke(tech);
            EventManager.Instance?.TriggerEvent(GameEvents.TECH_UNLOCKED, tech);
            EventManager.Instance?.TriggerEvent(GameEvents.TECH_RESEARCH_COMPLETE, tech);
        }

        private void ApplyTechBonuses(TechData tech)
        {
            foreach (var bonus in tech.Bonuses)
            {
                ApplyBonus(bonus);
            }
        }

        private void ApplyBonus(TechBonus bonus)
        {
            switch (bonus.Type)
            {
                case TechBonusType.ProductionSpeed:
                    break;
                case TechBonusType.PowerEfficiency:
                    break;
                case TechBonusType.BuildingHealth:
                    break;
                case TechBonusType.TurretDamage:
                    break;
                case TechBonusType.TurretRange:
                    break;
                case TechBonusType.StorageCapacity:
                    break;
                case TechBonusType.NetworkRange:
                    break;
            }
        }

        private void UnlockTechContent(TechData tech)
        {
            foreach (var buildingId in tech.UnlockedBuildingIds)
            {
                Card.CardManager.Instance?.AddCard(buildingId, 1);
            }

            foreach (var cardId in tech.UnlockedCardIds)
            {
                Card.CardManager.Instance?.AddCard(cardId, 1);
            }
        }

        public List<TechData> GetAvailableTechs()
        {
            List<TechData> available = new List<TechData>();

            foreach (var tech in AllTechs)
            {
                if (CanResearch(tech.TechId))
                {
                    available.Add(tech);
                }
            }

            return available;
        }

        public List<TechData> GetUnlockedTechs()
        {
            List<TechData> result = new List<TechData>();

            foreach (var techId in unlockedTechs)
            {
                TechData tech = GetTechData(techId);
                if (tech != null)
                {
                    result.Add(tech);
                }
            }

            return result;
        }

        public List<TechData> GetTechsByCategory(TechCategory category)
        {
            List<TechData> result = new List<TechData>();

            foreach (var tech in AllTechs)
            {
                if (tech.Category == category)
                {
                    result.Add(tech);
                }
            }

            return result;
        }

        public TechSaveData GetSaveData()
        {
            TechSaveData saveData = new TechSaveData
            {
                UnlockedTechIds = new List<string>(unlockedTechs).ToArray(),
                ResearchingTechIds = new List<string>(researchingTechs.Keys).ToArray(),
                ResearchProgress = new List<float>(researchingTechs.Values).ToArray()
            };

            return saveData;
        }

        public void ApplySaveData(TechSaveData saveData)
        {
            unlockedTechs.Clear();
            researchingTechs.Clear();

            foreach (var techId in saveData.UnlockedTechIds)
            {
                unlockedTechs.Add(techId);
            }

            for (int i = 0; i < saveData.ResearchingTechIds.Length; i++)
            {
                researchingTechs[saveData.ResearchingTechIds[i]] = saveData.ResearchProgress[i];
            }
        }
    }

    [System.Serializable]
    public class TechSaveData
    {
        public string[] UnlockedTechIds;
        public string[] ResearchingTechIds;
        public float[] ResearchProgress;
    }
}
