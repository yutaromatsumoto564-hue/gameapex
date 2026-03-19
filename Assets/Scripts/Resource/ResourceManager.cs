using UnityEngine;
using System;
using System.Collections.Generic;
using ARIA.Core;

namespace ARIA.Resource
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        [Header("Resource Database")]
        public List<ResourceData> AllResources = new List<ResourceData>();

        [Header("Starting Resources")]
        public List<ResourceStack> StartingResources = new List<ResourceStack>();

        private Dictionary<string, int> resources = new Dictionary<string, int>();
        private Dictionary<string, ResourceData> resourceDatabase = new Dictionary<string, ResourceData>();

        [Header("Network Capacity")]
        public int BaseCapacity = 100;

        public event Action<string, int> OnResourceChanged;
        public event Action OnResourcesUpdated;

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
            InitializeStartingResources();
        }

        private void InitializeDatabase()
        {
            resourceDatabase.Clear();
            
            // 如果没有配置资源，创建默认资源
            if (AllResources == null || AllResources.Count == 0)
            {
                CreateDefaultResources();
            }
            
            foreach (var resource in AllResources)
            {
                if (resource != null && !string.IsNullOrEmpty(resource.ResourceId))
                {
                    resourceDatabase[resource.ResourceId] = resource;
                }
            }
        }
        
        private void CreateDefaultResources()
        {
            AllResources = new List<ResourceData>
            {
                CreateResourceData("res_crystal", "水晶", "基础资源", ResourceCategory.Basic),
                CreateResourceData("res_metal", "金属", "基础资源", ResourceCategory.Basic),
                CreateResourceData("res_energy", "能量", "基础资源", ResourceCategory.Basic),
                CreateResourceData("res_organic", "有机物", "基础资源", ResourceCategory.Organic),
                CreateResourceData("res_rare", "稀有矿石", "稀有资源", ResourceCategory.Advanced),
            };
            Debug.Log("Created default resources");
        }
        
        private ResourceData CreateResourceData(string id, string name, string desc, ResourceCategory category)
        {
            ResourceData data = ScriptableObject.CreateInstance<ResourceData>();
            data.ResourceId = id;
            data.ResourceName = name;
            data.Description = desc;
            data.Category = category;
            data.BaseValue = 10;
            return data;
        }

        private void InitializeStartingResources()
        {
            resources.Clear();
            
            // 如果没有配置初始资源，给一些默认资源
            if (StartingResources == null || StartingResources.Count == 0)
            {
                CreateDefaultStartingResources();
            }
            
            foreach (var stack in StartingResources)
            {
                if (stack.Resource != null)
                {
                    AddResource(stack.Resource.ResourceId, stack.Amount);
                }
            }
        }
        
        private void CreateDefaultStartingResources()
        {
            StartingResources = new List<ResourceStack>();
            
            // 给玩家一些初始资源
            var energyRes = GetResourceData("res_energy");
            var metalRes = GetResourceData("res_metal");
            var crystalRes = GetResourceData("res_crystal");
            
            if (energyRes != null) StartingResources.Add(new ResourceStack { Resource = energyRes, Amount = 100 });
            if (metalRes != null) StartingResources.Add(new ResourceStack { Resource = metalRes, Amount = 50 });
            if (crystalRes != null) StartingResources.Add(new ResourceStack { Resource = crystalRes, Amount = 30 });
            
            Debug.Log($"创建了 {StartingResources.Count} 种默认初始资源");
        }

        public ResourceData GetResourceData(string resourceId)
        {
            return resourceDatabase.TryGetValue(resourceId, out ResourceData data) ? data : null;
        }

        public int GetResourceAmount(string resourceId)
        {
            return resources.TryGetValue(resourceId, out int amount) ? amount : 0;
        }

        public bool HasResource(string resourceId, int amount)
        {
            return GetResourceAmount(resourceId) >= amount;
        }

        public bool HasResources(List<Building.ResourceIO> requirements)
        {
            foreach (var req in requirements)
            {
                if (!HasResource(req.ResourceId, req.Amount))
                {
                    return false;
                }
            }
            return true;
        }

        public void AddResource(string resourceId, int amount)
        {
            if (amount <= 0) return;

            if (!resources.ContainsKey(resourceId))
            {
                resources[resourceId] = 0;
            }

            resources[resourceId] += amount;

            OnResourceChanged?.Invoke(resourceId, resources[resourceId]);
            OnResourcesUpdated?.Invoke();

            EventManager.Instance?.TriggerEvent(GameEvents.RESOURCE_ADDED, resourceId, amount);
        }

        public bool RemoveResource(string resourceId, int amount)
        {
            if (amount <= 0) return true;
            if (!HasResource(resourceId, amount)) return false;

            resources[resourceId] -= amount;

            if (resources[resourceId] <= 0)
            {
                resources.Remove(resourceId);
            }

            OnResourceChanged?.Invoke(resourceId, GetResourceAmount(resourceId));
            OnResourcesUpdated?.Invoke();

            EventManager.Instance?.TriggerEvent(GameEvents.RESOURCE_REMOVED, resourceId, amount);
            return true;
        }

        public int GetTotalCapacity()
        {
            int capacity = BaseCapacity;

            var storageBuildings = Building.BuildingManager.Instance?.GetBuildingsByCategory(Building.BuildingCategory.Storage);
            if (storageBuildings != null)
            {
                foreach (var building in storageBuildings)
                {
                    capacity += building.Data.StorageCapacity;
                }
            }

            return capacity;
        }

        public int GetTotalStored()
        {
            int total = 0;
            foreach (var kvp in resources)
            {
                total += kvp.Value;
            }
            return total;
        }

        public bool HasAvailableCapacity(int requiredAmount = 1)
        {
            return GetTotalStored() + requiredAmount <= GetTotalCapacity();
        }

        public float GetStoragePercentage()
        {
            int capacity = GetTotalCapacity();
            if (capacity <= 0) return 0f;
            return (float)GetTotalStored() / capacity;
        }

        public List<ResourceStack> GetAllResources()
        {
            List<ResourceStack> stacks = new List<ResourceStack>();
            foreach (var kvp in resources)
            {
                ResourceData data = GetResourceData(kvp.Key);
                if (data != null && kvp.Value > 0)
                {
                    stacks.Add(new ResourceStack { Resource = data, Amount = kvp.Value });
                }
            }
            return stacks;
        }

        public List<ResourceStack> GetResourcesByCategory(ResourceCategory category)
        {
            return GetAllResources().FindAll(s => s.Resource.Category == category);
        }

        public int SellResource(string resourceId, int amount)
        {
            if (!RemoveResource(resourceId, amount)) return 0;

            ResourceData data = GetResourceData(resourceId);
            return data != null ? data.BaseValue * amount : 0;
        }

        public ResourceSaveData GetSaveData()
        {
            List<string> names = new List<string>();
            List<int> amounts = new List<int>();

            foreach (var kvp in resources)
            {
                names.Add(kvp.Key);
                amounts.Add(kvp.Value);
            }

            return new ResourceSaveData
            {
                ResourceNames = names.ToArray(),
                ResourceAmounts = amounts.ToArray()
            };
        }

        public void ApplySaveData(ResourceSaveData saveData)
        {
            resources.Clear();
            for (int i = 0; i < saveData.ResourceNames.Length; i++)
            {
                resources[saveData.ResourceNames[i]] = saveData.ResourceAmounts[i];
            }
            OnResourcesUpdated?.Invoke();
        }
    }

    [System.Serializable]
    public class ResourceStack
    {
        public ResourceData Resource;
        public int Amount;
    }
}
